using FFMpegCore;
using FFMpegCore.Enums;
using FzLib.IO;
using Instances;
using SimpleFFmpegGUI.Dto;
using SimpleFFmpegGUI.FFMpegArgumentExtension;
using SimpleFFmpegGUI.Manager;
using SimpleFFmpegGUI.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Task = System.Threading.Tasks.Task;
using Tasks = System.Threading.Tasks;

namespace SimpleFFmpegGUI.Manager
{
    public class QueueManager
    {
        private DateTime pauseStartTime;
        private bool paused = false;
        public bool IsRunning => ProcessingTask != null;
        public bool IsPaused => ProcessingTask != null && paused;
        public TaskInfo ProcessingTask { get; private set; }
        public ProgressDto Progress { get; private set; }
        private bool cancelQueue = false;
        private CancellationTokenSource cancel = new CancellationTokenSource();

        public QueueManager()
        {
        }

        private IQueryable<TaskInfo> GetQueueTasks(FFmpegDbContext db)
        {
            return db.Tasks
                .Where(p => p.Status == TaskStatus.Queue)
                .Where(p => !p.IsDeleted);
        }

        public async void StartQueue()
        {
            if (ProcessingTask != null)
            {
                Logger.Warn("队列正在运行，开始队列失败");
                return;
            }
            Logger.Info("开始队列");
            FFmpegDbContext db = FFmpegDbContext.Get();
            List<TaskInfo> tasks;
            while (!cancelQueue && GetQueueTasks(db).Any())
            {
                tasks = GetQueueTasks(db).OrderBy(p => p.CreateTime).ToList();

                var task = tasks[0];

                ProcessingTask = task;
                task.Status = TaskStatus.Processing;
                task.StartTime = DateTime.Now;
                try
                {
                    db.SaveChanges();
                    await StartNewAsync(task);
                    task.Status = TaskStatus.Done;
                }
                catch (Exception ex)
                {
                    if (task.Status != TaskStatus.Cancel)
                    {
                        Logger.Error(task, "运行错误：" + ex.Message);
                        task.Status = TaskStatus.Error;
                        task.Message = ex.Message;
                    }
                    else
                    {
                        Logger.Warn(task, "任务被取消");
                    }
                }
                finally
                {
                    task.FinishTime = DateTime.Now;
                    ProcessingTask = null;
                    Progress = null;
                }
                db.Update(task);
                db.SaveChanges();
            }
            cancelQueue = false;
            Logger.Info("队列完成");
        }

        private ProgressDto GetProgress(TaskInfo task)
        {
            var p = new ProgressDto();
            if (task.Inputs.Count == 1)
            {
                p.Name = $"正在转码：{Path.GetFileName(task.Inputs[0])}";
                try
                {
                    if (task.Arguments.Input != null && task.Arguments.Input.From.HasValue && task.Arguments.Input.To.HasValue)
                    {
                        p.VideoLength = TimeSpan.FromSeconds(task.Arguments.Input.To.Value - task.Arguments.Input.From.Value);
                    }
                    else
                    {
                        p.VideoLength = FFProbe.Analyse(task.Inputs[0]).Duration;
                    }
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                p.Name = $"正在合并：{Path.GetFileName(task.Inputs[0])}等";
                try
                {
                    if (task.Arguments.Input != null && task.Arguments.Input.From.HasValue && task.Arguments.Input.To.HasValue)
                    {
                        p.VideoLength = TimeSpan.FromSeconds(task.Arguments.Input.To.Value - task.Arguments.Input.From.Value);
                    }
                    else
                    {
                        p.VideoLength = TimeSpan.FromTicks(task.Inputs.Select(p => FFProbe.Analyse(p).Duration.Ticks).Sum());
                    }
                }
                catch
                {
                    return null;
                }
            }
            p.StartTime = DateTime.Now;
            return p;
        }

        private ProgressDto GetConvertToTsProgress(string file)
        {
            var p = new ProgressDto();
            p.Name = $"正在生成临时TS文件：{Path.GetFileName(file)}";
            try
            {
                p.VideoLength = FFProbe.Analyse(file).Duration;
                p.StartTime = DateTime.Now;
                return p;
            }
            catch
            {
                return null;
            }
        }

        private async Tasks.Task<List<string>> ConvertToTsAsync(IEnumerable<string> files, string tempFolder)
        {
            List<string> tsFiles = new List<string>();
            if (!Directory.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder);
            }
            foreach (var file in files)
            {
                Progress = GetConvertToTsProgress(file);
                string path = FileSystem.GetNoDuplicateFile(Path.Combine(tempFolder, "join.ts"));
                tsFiles.Add(path);
                await FFMpegArguments.FromFileInput(file)
                          .OutputToFile(path, true, p =>
                              p.CopyChannel()
                              .WithBitStreamFilter(Channel.Video, Filter.H264_Mp4ToAnnexB)
                              .ForceFormat(VideoType.Ts))
                          .NotifyOnOutput(Output)
                          .ProcessAsynchronously();
            }
            return tsFiles;
        }

        private async Tasks.Task<FFMpegArgumentProcessor> GetCodeProcessorAsync(TaskInfo task, string tempDir)
        {
            FFMpegArguments f = null;
            if (task.Inputs.Count() == 1)
            {
                f = FFMpegArguments.FromFileInput(task.Inputs.First(), true, a => ApplyInputArguments(a, task.Arguments));

                Progress = GetProgress(task);
            }
            else
            {
                var tsFiles = await ConvertToTsAsync(task.Inputs, tempDir);
                f = FFMpegArguments.FromConcatInput(tsFiles, a => ApplyInputArguments(a, task.Arguments));
                Progress = GetProgress(task);
            }
            task.Output = GetFileName(task.Arguments, task.Output);
            return f.OutputToFile(task.Output, true, a => ApplyOutputArguments(a, task.Arguments));
        }

        public async Task StartNewAsync(TaskInfo task)
        {
            Logger.Info(task, "开始任务"); ProcessingTask = task;

            string tempDir = Path.Combine(Path.GetDirectoryName(task.Output), "temp");
            paused = false;
            if (!task.Inputs.Any())
            {
                throw new ArgumentException("没有输入文件");
            }

            FFMpegArgumentProcessor p = task.Type switch
            {
                TaskType.Code => await GetCodeProcessorAsync(task, tempDir),
                _ => throw new NotSupportedException("不支持的任务类型：" + task.Type),
            };
            Logger.Info("FFmpeg参数为：" + p.Arguments);
            await p.NotifyOnOutput(Output)
                    .CancellableThrough(cancel.Token)
                    .ProcessAsynchronously();

            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
            Logger.Info(task, "完成任务");
        }

        private void Output(string data, DataType type)
        {
            Logger.Output(data);
            FFmpegOutput?.Invoke(this, new FFmpegOutputEventArgs(type == Instances.DataType.Error, data));
        }

        public void Suspend()
        {
            if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                throw new PlatformNotSupportedException("暂停和恢复功能仅支持Windows");
            }
            if (ProcessingTask == null)
            {
                return;
            }
            paused = true;
            Logger.Info(ProcessingTask, "暂停队列");
            pauseStartTime = DateTime.Now;
            ProcessExtension.SuspendProcess(GetFFmpegProcess().Id);
        }

        public void Resume()
        {
            if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                throw new PlatformNotSupportedException("暂停和恢复功能仅支持Windows");
            }
            if (ProcessingTask == null)
            {
                return;
            }
            paused = false;
            Progress.PauseTime = DateTime.Now - pauseStartTime;
            Logger.Info(ProcessingTask, "恢复队列");
            ProcessExtension.ResumeProcess(GetFFmpegProcess().Id);
        }

        public void Cancel()
        {
            Logger.Info("取消队列");
            ProcessingTask.Status = TaskStatus.Cancel;
            cancelQueue = true;
            cancel.Cancel();
        }

        public void CancelCurrent()
        {
            Logger.Info(ProcessingTask, "取消当前任务");
            ProcessingTask.Status = TaskStatus.Cancel;
            GetFFmpegProcess().Kill();
        }

        public event EventHandler<FFmpegOutputEventArgs> FFmpegOutput;

        private Process GetFFmpegProcess()
        {
            Process current = Process.GetCurrentProcess();
            var ps = Process.GetProcessesByName("ffmpeg")
                .Where(p => Path.GetDirectoryName(p.MainModule.FileName)
                == Path.GetDirectoryName(current.MainModule.FileName))
                .Where(p => p.StartTime > current.StartTime);
            if (ps.Count() != 1)
            {
                throw new Exception("存在多个或不存在FFmpeg进程");
            }
            return ps.First();
        }

        private void ApplyInputArguments(FFMpegArgumentOptions fa, CodeArguments a)
        {
            if (a.Input == null)
            {
                return;
            }
            if (a.Input.From.HasValue && a.Input.To.HasValue)
            {
                fa.Seek(TimeSpan.FromSeconds(a.Input.From.Value))
                    .WithDuration(TimeSpan.FromSeconds(a.Input.To.Value - a.Input.From.Value));
            }
        }

        private void ApplyOutputArguments(FFMpegArgumentOptions fa, CodeArguments a)
        {
            if (a.DisableVideo && a.DisableAudio)
            {
                throw new Exception("不能同时禁用视频和音频");
            }
            if (a.Video == null && a.Audio == null && !a.DisableAudio && !a.DisableVideo)
            {
                fa.CopyChannel(Channel.Both);
                return;
            }
            if (a.DisableVideo)
            {
                a.Video = null;
            }
            if (a.DisableAudio)
            {
                a.Audio = null;
            }

            if (a.Video == null)
            {
                if (a.DisableVideo)
                {
                    fa.DisableChannel(Channel.Video);
                }
                else
                {
                    fa.CopyChannel(Channel.Video);
                }
            }
            if (a.Audio == null)
            {
                if (a.DisableAudio)
                {
                    fa.DisableChannel(Channel.Audio);
                }
                else
                {
                    fa.CopyChannel(Channel.Audio);
                }
            }

            if (a.Video != null)
            {
                Codec code = a.Video.Code.ToLower().Replace(".", "") switch
                {
                    "h265" => FFMpeg.GetCodec("libx265"),
                    "h264" => FFMpeg.GetCodec("libx264"),
                    "vp9" => FFMpeg.GetCodec("libvpx-vp9"),
                    _ => throw new NotSupportedException("不支持的格式：" + a.Video.Code)
                };
                fa.WithVideoCodec(code);
                fa.WithSpeedPreset((Speed)a.Video.Preset);
                if (a.Video.Crf.HasValue)
                {
                    fa.WithConstantRateFactor(a.Video.Crf.Value);
                }
                if (a.Video.Fps.HasValue)
                {
                    fa.WithFramerate(a.Video.Fps.Value);
                }
                if (a.Video.AverageBitrate.HasValue)
                {
                    fa.WithVideoMBitrate(a.Video.AverageBitrate.Value);
                }
                if (a.Video.MaxBitrate.HasValue && a.Video.MaxBitrateBuffer.HasValue)
                {
                    fa.WithVideoMMaxBitrate(a.Video.MaxBitrate.Value, a.Video.MaxBitrateBuffer.Value);
                }
                if (a.Video.Width.HasValue && a.Video.Height.HasValue)
                {
                    fa.WithVideoFilters(o =>
                    {
                        if (a.Video.Width.HasValue && a.Video.Height.HasValue)
                        {
                            o.Scale(a.Video.Width.Value, a.Video.Height.Value);
                        }
                    });
                }
            }
            if (a.Audio != null)
            {
                Codec code = a.Audio.Code.ToLower().Replace(".", "") switch
                {
                    "aac" => FFMpeg.GetCodec("aac"),
                    "ac3" => FFMpeg.GetCodec("ac3"),
                    _ => throw new NotSupportedException("不支持的格式：" + a.Video.Code)
                };
                fa.WithAudioCodec(code);
                if (a.Audio.Bitrate.HasValue)
                {
                    fa.WithAudioBitrate(a.Audio.Bitrate.Value);
                }
                if (a.Audio.SamplingRate.HasValue)
                {
                    fa.WithAudioSamplingRate(a.Audio.SamplingRate.Value);
                }
            }

            if (!string.IsNullOrWhiteSpace(a.Extra))
            {
                fa.WithArguments(a.Extra);
            }
            if (!string.IsNullOrWhiteSpace(a.Format))
            {
                fa.ForceFormat(a.Format);
            }
        }

        private string GetFileName(CodeArguments a, string output)
        {
            string result = output;
            if (!string.IsNullOrEmpty(a.Format))
            {
                VideoFormat format = VideoFormat.Formats.Where(p => p.Name == a.Format || p.Extension == a.Format).FirstOrDefault();
                if (format != null)
                {
                    string dir = Path.GetDirectoryName(output);
                    string name = Path.GetFileNameWithoutExtension(output);
                    string extension = format.Extension;
                    result = Path.Combine(dir, name + "." + extension);
                }
            }
            return FileSystem.GetNoDuplicateFile(result);
        }
    }
}