using System;
using System.Collections.Generic;
using Instances;
using SimpleFFmpegGUI.Dto;
using SimpleFFmpegGUI.Model;
using System.IO;
using System.Linq;
using Task = System.Threading.Tasks.Task;
using Tasks = System.Threading.Tasks;
using FFMpegCore;
using FFMpegCore.Enums;
using FzLib.IO;
using SimpleFFmpegGUI.FFMpegArgumentExtension;
using System.Threading;
using System.Text.RegularExpressions;
using FFMpegCore.Exceptions;
using System.ComponentModel;
using FzLib;

namespace SimpleFFmpegGUI.Manager
{
    public class FFmpegManager : INotifyPropertyChanged
    {
        public FFmpegManager(TaskInfo task)
        {
            this.task = task;
        }

        public TaskInfo Task => task;
        private bool hasRun = false;
        private DateTime pauseStartTime;
        private bool paused;

        public bool Paused
        {
            get => paused;
            set => this.SetValueAndNotify(ref paused, value, nameof(Paused));
        }

        private static readonly Regex rSsim = new Regex(@"SSIM ([YUVAll]+:[0-9\.\(\) ]+)+", RegexOptions.Compiled);
        private static readonly Regex rPsnr = new Regex(@"PSNR (([yuvaverageminmax]+:[0-9\. ]+)+)", RegexOptions.Compiled);
        private CancellationTokenSource cancel;
        public ProgressDto Progress { get; private set; }
        private readonly TaskInfo task;
        private FFmpegProcess Process { get; set; }
        private string lastOutput;

        public event EventHandler StatusChanged;

        public StatusDto GetStatus()
        {
            if (Process == null)
            {
                return new StatusDto(task);
            }
            return new StatusDto(task, Progress, lastOutput, paused);
        }

        private TimeSpan GetVideoDuration(InputArguments arg)
        {
            var path = arg.FilePath;
            TimeSpan realLength = FFProbe.Analyse(path).Duration;
            if (arg == null || arg.From == null && arg.To == null && arg.Duration == null)
            {
                return realLength;
            }

            TimeSpan start = TimeSpan.Zero;
            if (arg.From.HasValue)
            {
                start = arg.From.Value;
            }
            if (arg.To == null && arg.Duration == null)
            {
                if (realLength <= arg.From.Value)
                {
                    throw new Exception("开始时间在视频结束时间之后");
                }
                return realLength - arg.From.Value;
            }
            else if (arg.Duration.HasValue)
            {
                TimeSpan endTime = (arg.From.HasValue ? arg.From.Value : TimeSpan.Zero) + arg.Duration.Value;
                if (endTime > realLength)
                {
                    throw new Exception("裁剪后的结束时间在视频结束时间之后");
                }
                return arg.Duration.Value;
            }
            else if (arg.To.HasValue)
            {
                if (arg.To.Value > realLength)
                {
                    throw new Exception("裁剪后的结束时间在视频结束时间之后");
                }
                return arg.To.Value - start;
            }

            throw new Exception("未知情况");
        }

        private ProgressDto GetProgress(TaskInfo task)
        {
            var p = new ProgressDto();
            if (task.Inputs.Count == 1)
            {
                p.VideoLength = GetVideoDuration(task.Inputs[0]);
            }
            else
            {
                p.VideoLength = TimeSpan.FromTicks(task.Inputs.Select(p => GetVideoDuration(p).Ticks).Sum());
            }
            p.StartTime = DateTime.Now;
            return p;
        }

        private ProgressDto GetConvertToTsProgress(string file)
        {
            var p = new ProgressDto();
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

        private async Tasks.Task<List<string>> ConvertToTsAsync(TaskInfo task, string tempFolder, CancellationToken cancellationToken)
        {
            List<string> tsFiles = new List<string>();
            if (!Directory.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder);
            }
            int i = 0;
            foreach (var file in task.Inputs)
            {
                Progress = GetConvertToTsProgress(file.FilePath);
                string path = FileSystem.GetNoDuplicateFile(Path.Combine(tempFolder, "join.ts"));
                tsFiles.Add(path);
                var p = FFMpegArguments.FromFileInput(file.FilePath)
                          .OutputToFile(path, true, p =>
                              p.CopyChannel()
                              .WithBitStreamFilter(Channel.Video, Filter.H264_Mp4ToAnnexB)
                              .ForceFormat(VideoType.Ts));
                await RunAsync(p, $"打包第{++i}个临时文件", cancellationToken);
            }
            return tsFiles;
        }

        private async Task RunCodeProcessAsync(string tempDir, CancellationToken cancellationToken)
        {
            FFMpegArguments f = null;
            string message = null;
            if (task.Inputs.Count() == 1)
            {
                f = FFMpegArguments.FromFileInput(task.Inputs[0].FilePath, true, a => ApplyInputArguments(a, task.Inputs[0]));

                Progress = GetProgress(task);
                message = $"正在转码：{Path.GetFileName(task.Inputs[0].FilePath)}";
            }
            else
            {
                throw new ArgumentException("普通编码，输入文件必须为1个");
            }
            GenerateOutputPath();
            var p = f.OutputToFile(task.RealOutput, true, a => ApplyOutputArguments(a, task.Arguments));
            await RunAsync(p, message, cancellationToken);
        }

        private async Task RunConcatProcessAsync(string tempDir, CancellationToken cancellationToken)
        {
            FFMpegArguments f = null;
            string message = null;
            if (task.Inputs.Count() < 2)
            {
                throw new ArgumentException("拼接视频，输入文件必须为2个或更多");
            }
            message = $"正在合并：{Path.GetFileName(task.Inputs[0].FilePath)}等";

            if (task.Arguments.Concat == null || task.Arguments.Concat.Type == ConcatType.ViaTs)
            {
                var tsFiles = await ConvertToTsAsync(task, tempDir, cancellationToken);
                f = FFMpegArguments.FromConcatInput(tsFiles, a => ApplyInputArguments(a, task.Inputs[0]));
                Progress = GetProgress(task);

                GenerateOutputPath();
                var p = f.OutputToFile(task.RealOutput, true, a => ApplyOutputArguments(a, task.Arguments));
                await RunAsync(p, message, cancellationToken);
            }
            else
            {
                string tempName = Guid.NewGuid().ToString() + ".txt";
                string tempPath = Path.Combine(tempDir, tempName);
                using (var stream = File.CreateText(tempPath))
                {
                    foreach (var file in task.Inputs)
                    {
                        stream.WriteLine($"file '{file.FilePath}'");
                    }
                }
                task.Arguments.Format = null;
                GenerateOutputPath();
                var p = FFMpegArguments.FromFileInput(tempPath, false,
                    o => o.WithCustomArgument("-f concat -safe 0"))
                     .OutputToFile(task.RealOutput, true, o => o.CopyChannel(Channel.Both));
                await RunAsync(p, message, cancellationToken);
            }
        }

        private async Task RunCombineProcessAsync(CancellationToken cancellationToken)
        {
            if (task.Inputs.Count() != 2)
            {
                throw new ArgumentException("合并音视频操作，输入文件必须为2个");
            }
            var video = FFProbe.Analyse(task.Inputs[0].FilePath);
            var audio = FFProbe.Analyse(task.Inputs[1].FilePath);
            if (video.VideoStreams.Count == 0)
            {
                throw new ArgumentException("输入1不含视频");
            }
            if (audio.AudioStreams.Count == 0)
            {
                throw new ArgumentException("输入2不含音频");
            }
            FFMpegArguments f = FFMpegArguments
                .FromFileInput(task.Inputs[0].FilePath)
                .AddFileInput(task.Inputs[1].FilePath);

            Progress = GetProgress(task);

            GenerateOutputPath();
            var p = f.OutputToFile(task.RealOutput, true, a =>
            {
                a.CopyChannel(Channel.Both);
                if (video.AudioStreams.Count != 0 || audio.VideoStreams.Count != 0)
                {
                    a.WithMapping(0, Channel.Video, 0)
                     .WithMapping(1, Channel.Audio, 0);
                }

                if (task.Arguments?.Combine?.Shortest ?? false)
                {
                    a.UsingShortest(true);
                }
            });
            await RunAsync(p, "正在合并音视频", cancellationToken);
        }

        private async Task RunCompareProcessAsync(CancellationToken cancellationToken)
        {
            if (task.Inputs.Count() != 2)
            {
                throw new ArgumentException("视频比较，输入文件必须为2个");
            }
            var v1 = FFProbe.Analyse(task.Inputs[0].FilePath);
            var v2 = FFProbe.Analyse(task.Inputs[1].FilePath);
            if (v1.VideoStreams.Count == 0)
            {
                throw new ArgumentException("输入1不含视频");
            }
            if (v2.VideoStreams.Count == 0)
            {
                throw new ArgumentException("输入2不含视频");
            }
            var p = FFMpegArguments
                 .FromFileInput(task.Inputs[0].FilePath)
                 .AddFileInput(task.Inputs[1].FilePath)
                 .OutputToUrl("", o =>
                 {
                     o.WithCustomArgument("-lavfi \"ssim;[0:v][1:v]psnr\" -f null -");
                 });
            FFmpegOutput += CheckOutput;
            string ssim = null;
            string psnr = null;
            try
            {
                await RunAsync(p, null, cancellationToken);
                if (ssim == null || psnr == null)
                {
                    throw new Exception("对比视频失败，为识别到对比结果");
                }
                task.Message = ssim + Environment.NewLine + psnr;
            }
            finally
            {
                FFmpegOutput -= CheckOutput;
            }

            void CheckOutput(object sender, FFmpegOutputEventArgs e)
            {
                if (!e.Data.StartsWith('['))
                {
                    return;
                }
                if (rSsim.IsMatch(e.Data))
                {
                    var match = rSsim.Match(e.Data);
                    ssim = match.Value;
                    Logger.Info(task, "对比结果（SSIM）：" + match.Value);
                }
                if (rPsnr.IsMatch(e.Data))
                {
                    var match = rPsnr.Match(e.Data);
                    psnr = match.Value;
                    Logger.Info(task, "对比结果（PSNR）：" + match.Value);
                }
            }
        }

        private async Task RunCustomProcessAsync(CancellationToken cancellationToken)
        {
            Type type = typeof(FFMpegArguments);
            FFMpegArguments fa = Activator.CreateInstance(typeof(FFMpegArguments), true) as FFMpegArguments;

            var p = fa
                 .OutputToUrl("", o =>
                 {
                     o.WithCustomArgument(task.Arguments.Extra);
                 });

            await RunAsync(p, null, cancellationToken);
        }

        public async Task RunAsync()
        {
            if (hasRun)
            {
                throw new Exception("一个实例只可运行一次");
            }
            hasRun = true;
            cancel = new CancellationTokenSource();
            try
            {
                Logger.Info(task, "开始任务");
                string tempDir = ConfigHelper.TempDir;
                await (task.Type switch
                {
                    TaskType.Code => RunCodeProcessAsync(tempDir, cancel.Token),
                    TaskType.Combine => RunCombineProcessAsync(cancel.Token),
                    TaskType.Compare => RunCompareProcessAsync(cancel.Token),
                    TaskType.Custom => RunCustomProcessAsync(cancel.Token),
                    TaskType.Concat => RunConcatProcessAsync(tempDir, cancel.Token),
                    _ => throw new NotSupportedException("不支持的任务类型：" + task.Type),
                });

                if (Directory.Exists(tempDir))
                {
                    try
                    {
                        Directory.Delete(tempDir, true);
                    }
                    catch (Exception ex)
                    {
                    }
                }
                Logger.Info(task, "完成任务");
            }
            finally
            {
                Progress = null;
            }
        }

        private async Task RunAsync(FFMpegArgumentProcessor processor, string desc, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                Logger.Info(task, "进程启动前就被要求取消");
                return;
            }
            Logger.Info(task, "FFmpeg参数为：" + processor.Arguments);
            task.FFmpegArguments = string.IsNullOrEmpty(task.FFmpegArguments) ? processor.Arguments : task.FFmpegArguments + ";" + processor.Arguments;
            if (Progress != null)
            {
                Progress.Name = desc;
            }
            Process = new FFmpegProcess(processor.Arguments);
            Process.Output += Output;
            try
            {
                await Process.StartAsync(cancellationToken);
            }
            finally
            {
                Process = null;
            }
        }

        private void Output(object sender, FFmpegOutputEventArgs e)
        {
            lastOutput = e.Data;
            Logger.Output(task, e.Data);
            FFmpegOutput?.Invoke(this, new FFmpegOutputEventArgs(e.Data));
            StatusChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ApplyInputArguments(FFMpegArgumentOptions fa, InputArguments a)
        {
            if (a == null)
            {
                return;
            }
            if (a.From.HasValue && a.To.HasValue)
            {
                fa.Seek(a.From.Value);
            }
            if (a.To.HasValue)
            {
                fa.To(a.To.Value);
            }
            if (a.Duration.HasValue)
            {
                fa.WithDuration(a.Duration.Value);
            }
        }

        private void ApplyOutputArguments(FFMpegArgumentOptions fa, OutputArguments a)
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
                    _ => null
                };
                if (code != null)
                {
                    fa.WithVideoCodec(code);
                }
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
                    "opus" => FFMpeg.GetCodec("libopus"),
                    _ => null
                };
                if (code != null)
                {
                    fa.WithAudioCodec(code);
                }
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

        private void GenerateOutputPath()
        {
            string output = task.Output;
            var a = task.Arguments;
            if (string.IsNullOrEmpty(output))
            {
                if (task.Inputs.Count == 0)
                {
                    throw new Exception("没有指定输出路径，且输入文件为空");
                }
                output = task.Inputs[0].FilePath;
            }
            if (!string.IsNullOrEmpty(a?.Format))
            {
                VideoFormat format = VideoFormat.Formats.Where(p => p.Name == a.Format || p.Extension == a.Format).FirstOrDefault();
                if (format != null)
                {
                    string dir = Path.GetDirectoryName(output);
                    string name = Path.GetFileNameWithoutExtension(output);
                    string extension = format.Extension;
                    output = Path.Combine(dir, name + "." + extension);
                }
            }
            task.RealOutput = FileSystem.GetNoDuplicateFile(output);
        }

        public event EventHandler<FFmpegOutputEventArgs> FFmpegOutput;

        public event PropertyChangedEventHandler PropertyChanged;

        public void Suspend()
        {
            if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                throw new PlatformNotSupportedException("暂停和恢复功能仅支持Windows");
            }
            if (Process == null)
            {
                throw new Exception("进程还未启动，不可暂停");
            }
            Paused = true;
            Logger.Info(task, "暂停队列");
            pauseStartTime = DateTime.Now;
            ProcessExtension.SuspendProcess(Process.Id);
        }

        public void Resume()
        {
            if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                throw new PlatformNotSupportedException("暂停和恢复功能仅支持Windows");
            }
            if (Process == null)
            {
                throw new Exception("进程还未启动，不可暂停或恢复");
            }
            Paused = false;
            Progress.PauseTime = DateTime.Now - pauseStartTime;
            Logger.Info(task, "恢复队列");
            ProcessExtension.ResumeProcess(Process.Id);
        }

        public void Cancel()
        {
            Logger.Info(task, "取消当前任务");
            task.Status = TaskStatus.Cancel;
            cancel.Cancel();
        }
    }
}