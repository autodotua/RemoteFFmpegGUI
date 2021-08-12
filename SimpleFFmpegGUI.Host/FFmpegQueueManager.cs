using FFMpegCore;
using FFMpegCore.Enums;
using SimpleFFmpegGUI.Dto;
using SimpleFFmpegGUI.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace SimpleFFmpegGUI.Host
{
    public class FFmpegQueueManager
    {
        private DateTime pauseStartTime;
        public TaskInfo ProcessingTask { get; private set; }
        public ProgressDto Progress { get; private set; }
        private bool cancelQueue = false;

        public FFmpegQueueManager()
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
            using FFmpegDbContext db = new FFmpegDbContext();
            List<TaskInfo> tasks;
            while (!cancelQueue && GetQueueTasks(db).Any())
            {
                tasks = GetQueueTasks(db).OrderBy(p => p.CreateTime).ToList();

                var task = tasks[0];

                Progress = new ProgressDto();
                if (task.Inputs.Count == 1)
                {
                    try
                    {
                        if (task.Arguments.Input != null && task.Arguments.Input.From.HasValue && task.Arguments.Input.To.HasValue)
                        {
                            Progress.VideoLength = TimeSpan.FromSeconds(task.Arguments.Input.To.Value - task.Arguments.Input.From.Value);
                        }
                        else
                        {
                            Progress.VideoLength = FFProbe.Analyse(task.Inputs[0]).Duration;
                        }
                    }
                    catch
                    {
                    }
                    Progress.StartTime = DateTime.Now;
                }
                ProcessingTask = task;
                task.Status = Model.TaskStatus.Processing;
                try
                {
                    task.StartTime = DateTime.Now;
                    db.SaveChanges();
                    await StartNewAsync(task);
                    task.Status = Model.TaskStatus.Done;
                }
                catch (Exception ex)
                {
                    if (task.Status != Model.TaskStatus.Cancel)
                    {
                        Logger.Error("运行错误：" + ex.Message);
                        task.Status = Model.TaskStatus.Error;
                        task.Message = ex.Message;
                    }
                    else
                    {
                        Logger.Error("任务被取消");
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

        public async Task StartNewAsync(TaskInfo task)
        {
            Logger.Info(task, "开始任务"); ProcessingTask = task;

            FFMpegArguments f = task.Inputs.Count() == 1 ?
             FFMpegArguments.FromFileInput(task.Inputs.First(), true, a => ApplyInputArguments(a, task.Arguments))
             : FFMpegArguments.FromConcatInput(task.Inputs);
            FFMpegArgumentProcessor p = f.OutputToFile(FzLib.IO.FileSystem.GetNoDuplicateFile(task.Output), true,
                a => ApplyOutputArguments(a, task.Arguments));
            p.NotifyOnOutput((data, type) =>
            {
                Logger.Info("FFmpeg输出：" + data);
                FFmpegOutput?.Invoke(this, new FFmpegOutputEventArgs(type == Instances.DataType.Error, data));
            });

            Logger.Info("FFmpeg参数为：" + p.Arguments);
            await p.ProcessAsynchronously();

            Logger.Info(task, "完成任务");
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
            Logger.Info(ProcessingTask, "暂停任务");
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
            Progress.PauseTime = DateTime.Now - pauseStartTime;
            Logger.Info(ProcessingTask, "恢复任务");
            ProcessExtension.ResumeProcess(GetFFmpegProcess().Id);
        }

        public void Cancel()
        {
            Logger.Info("取消队列");
            ProcessingTask.Status = Model.TaskStatus.Cancel;
            cancelQueue = true;
            GetFFmpegProcess().Kill();
        }

        public void CancelCurrent()
        {
            Logger.Info(ProcessingTask, "取消当前任务");
            ProcessingTask.Status = Model.TaskStatus.Cancel;
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
                fa.Seek(TimeSpan.FromSeconds(a.Input.From.Value)).WithDuration(TimeSpan.FromSeconds(a.Input.To.Value));
            }
        }

        private void ApplyOutputArguments(FFMpegArgumentOptions fa, CodeArguments a)
        {
            if (a.Video == null && a.Audio == null)
            {
                fa.CopyChannel(Channel.Both);
                return;
            }
            if (a.Video == null)
            {
                fa.CopyChannel(Channel.Video);
            }
            else
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
            if (a.Audio == null)
            {
                fa.CopyChannel(Channel.Audio);
            }
            else
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
        }
    }
}