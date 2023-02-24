using FFMpegCore;
using FzLib;
using FzLib.IO;
using FzLib.Program;
using SimpleFFmpegGUI.Dto;
using SimpleFFmpegGUI.FFmpegArgument;
using SimpleFFmpegGUI.FFmpegLib;
using SimpleFFmpegGUI.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace SimpleFFmpegGUI.Manager
{
    public class FFmpegManager : INotifyPropertyChanged
    {
        private static readonly Regex[] ErrorMessageRegexs = new[]
        {
            new Regex("Error.*",RegexOptions.Compiled),
            new Regex(@"\[.*\] *Unable.*",RegexOptions.Compiled),
            new Regex(@".*Invalid.*",RegexOptions.Compiled|RegexOptions.IgnoreCase),
            new Regex(@"Could find no file.*",RegexOptions.Compiled|RegexOptions.IgnoreCase),
            new Regex(@".* error",RegexOptions.Compiled|RegexOptions.IgnoreCase),
        };

        private static readonly Regex rPSNR = new Regex(@"PSNR (([yuvaverageminmax]+:[0-9\. ]+)+)", RegexOptions.Compiled);
        private static readonly Regex rSSIM = new Regex(@"SSIM ([YUVAll]+:[0-9\.\(\) ]+)+", RegexOptions.Compiled);
        private static readonly Regex rVMAF = new Regex(@"VMAF score: [0-9\.]+", RegexOptions.Compiled);
        private readonly TaskInfo task;
        private CancellationTokenSource cancel;
        private bool hasRun = false;
        private string lastOutput;
        private Logger logger = new Logger();
        private bool paused;

        private DateTime pauseStartTime;

        public FFmpegManager(TaskInfo task)
        {
            this.task = task;
        }

        public event EventHandler<FFmpegOutputEventArgs> FFmpegOutput;

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler StatusChanged;

        public bool Paused
        {
            get => paused;
            set => this.SetValueAndNotify(ref paused, value, nameof(Paused));
        }

        public ProgressDto Progress { get; private set; }
        public TaskInfo Task => task;
        private FFmpegProcess Process { get; set; }
        public FFmpegProcess LastProcess { get; private set; }

        public static string TestOutputArguments(OutputArguments arguments)
        {
            return ArgumentsGenerator.GetOutputArguments(arguments, arguments.Video?.TwoPass == true ? 2 : 0);
        }

        public void Cancel()
        {
            logger.Info(task, "取消当前任务");
            task.Status = TaskStatus.Cancel;
            cancel.Cancel();
        }

        public string GetErrorMessage()
        {
            var logs = LogManager.GetLogs('O', Task.Id, DateTime.Now.AddSeconds(-5));
            var log = logs.List
                .Where(p => ErrorMessageRegexs.Any(q => q.IsMatch(p.Message)))
                .OrderByDescending(p => p.Time).FirstOrDefault();
            return log?.Message;
        }

        public StatusDto GetStatus()
        {
            if (Process == null)
            {
                return new StatusDto(task);
            }
            return new StatusDto(task, Progress, lastOutput, paused);
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
            Progress.PauseTime += DateTime.Now - pauseStartTime;
            logger.Info(task, "恢复队列");
            ProcessExtension.ResumeProcess(Process.Id);
            StatusChanged?.Invoke(this, EventArgs.Empty);
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
                logger.Info(task, "开始任务");
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
                logger.Info(task, "完成任务");
            }
            finally
            {
                Progress = null;
                logger.Dispose();
            }
        }

        public void Suspend()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new PlatformNotSupportedException("暂停和恢复功能仅支持Windows");
            }
            if (Process == null)
            {
                throw new Exception("进程还未启动或该任务不允许暂停");
            }
            Paused = true;
            logger.Info(task, "暂停队列");
            pauseStartTime = DateTime.Now;
            ProcessExtension.SuspendProcess(Process.Id);
            StatusChanged?.Invoke(this, EventArgs.Empty);
        }

       private static HashSet<char> invalidFileNameChars=Path.GetInvalidFileNameChars().ToHashSet();

        private static string GenerateOutputPath(TaskInfo task)
        {
            string output = task.Output.Trim();
            var a = task.Arguments;
            if (string.IsNullOrEmpty(output))
            {
                if (task.Inputs.Count == 0)
                {
                    throw new Exception("没有指定输出路径，且输入文件为空");
                }
                output = task.Inputs[0].FilePath;
            }

            //删除非法字符
            string dir = Path.GetDirectoryName(output);
            string filename = Path.GetFileName(output);
            if (filename.Any(p => invalidFileNameChars.Contains(p)))
            {
                foreach (var c in invalidFileNameChars)
                {
                    filename = filename.Replace(c.ToString(), "");
                }
                output = Path.Combine(dir, filename);
            }


            //修改扩展名
            if (!string.IsNullOrEmpty(a?.Format))
            {
                VideoFormat format = VideoFormat.Formats.Where(p => p.Name == a.Format || p.Extension == a.Format).FirstOrDefault();
                if (format != null)
                {
                    string name = Path.GetFileNameWithoutExtension(output);
                    string extension = format.Extension;
                    output = Path.Combine(dir, name + "." + extension);
                }
            }
            
            //获取非重复文件名
            task.RealOutput = FileSystem.GetNoDuplicateFile(output);
            if (!new FileInfo(task.RealOutput).Directory.Exists)
            {
                new FileInfo(task.RealOutput).Directory.Create();
            }
            return task.RealOutput;
        }

        private ProgressDto GetProgress(TaskInfo task, bool onlyCalcFirstVideoDuration = false)
        {
            var p = new ProgressDto();
            if (task.Inputs.Count == 1 || onlyCalcFirstVideoDuration)
            {
                p.VideoLength = GetVideoDuration(task.Inputs[0]);
            }
            else
            {
                var durations = task.Inputs.Select(p => GetVideoDuration(p));
                p.VideoLength = durations.All(p => p.HasValue) ? TimeSpan.FromTicks(durations.Select(p => p.Value.Ticks).Sum()) : null;
            }
            p.StartTime = DateTime.Now;
            return p;
        }

        private TimeSpan? GetVideoDuration(InputArguments arg)
        {
            var path = arg.FilePath;
            TimeSpan realLength = default;
            try
            {
                realLength = FFProbe.Analyse(path).Duration;
            }
            catch (Exception ex)
            {
                return null;
            }
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

        private void Output(object sender, FFmpegOutputEventArgs e)
        {
            lastOutput = e.Data;
            logger.Output(task, e.Data);
            FFmpegOutput?.Invoke(this, new FFmpegOutputEventArgs(e.Data));
            StatusChanged?.Invoke(this, EventArgs.Empty);
        }

        private async Task RunAsync(string arguments, string desc, CancellationToken cancellationToken, string workingDir = null)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                logger.Info(task, "进程启动前就被要求取消");
                return;
            }
            logger.Info(task, "FFmpeg参数为：" + arguments);
            task.FFmpegArguments = string.IsNullOrEmpty(task.FFmpegArguments) ? arguments : task.FFmpegArguments + ";" + arguments;
            if (Progress != null)
            {
                Progress.Name = desc;
            }

            Process = new FFmpegProcess(arguments);
            Process.Output += Output;
            try
            {
                await Process.StartAsync(workingDir, cancellationToken);
            }
            finally
            {
                LastProcess = Process;
                Process = null;
            }
        }
        private async Task RunCodeProcessAsync(string tempDir, CancellationToken cancellationToken)
        {
            string message = null;
            if (task.Inputs.Count != 1)
            {
                throw new ArgumentException("普通编码，输入文件必须为1个");
            }
            GenerateOutputPath(task);
            if (task.Arguments.Video == null || !task.Arguments.Video.TwoPass)
            {
                Progress = GetProgress(task);
                message = $"正在转码：{Path.GetFileName(task.Inputs[0].FilePath)}";
                string arg = ArgumentsGenerator.GetArguments(task, 0);
                await RunAsync(arg, message, cancellationToken);
            }
            else
            {
                string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                Directory.CreateDirectory(tempDirectory);
                Progress = GetProgress(task);
                Progress.VideoLength *= 2;
                message = $"正在转码（Pass=1）：{Path.GetFileName(task.Inputs[0].FilePath)}";

                string arg = ArgumentsGenerator.GetArguments(task, 1, RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "NUL" : "/dev/null");
                await RunAsync(arg, message, cancellationToken, tempDirectory);
                Progress.BasePercent = 0.5;

                message = $"正在转码（Pass=2）：{Path.GetFileName(task.Inputs[0].FilePath)}";
                arg = ArgumentsGenerator.GetArguments(task, 2);
                await RunAsync(arg, message, cancellationToken, tempDirectory);
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

            Progress = GetProgress(task, true);
            GenerateOutputPath(task);

            var outputArgs = ArgumentsGenerator.GetOutputArguments(v => v.Copy(), a => a.Copy(),
                s => (video.AudioStreams.Count != 0 || audio.VideoStreams.Count != 0) ? s.Map(0, StreamChannel.Video, 0).Map(0, StreamChannel.Audio, 0) : s);
            string arg = ArgumentsGenerator.GetArguments(task.Inputs, outputArgs, task.RealOutput);

            await RunAsync(arg, "正在合并音视频", cancellationToken);
        }

        private async Task RunCompareProcessAsync(CancellationToken cancellationToken)
        {
            if (task.Inputs.Count != 2)
            {
                throw new FFmpegArgumentException("视频比较，输入文件必须为2个");
            }
            var v1 = FFProbe.Analyse(task.Inputs[0].FilePath);
            var v2 = FFProbe.Analyse(task.Inputs[1].FilePath);
            if (v1.VideoStreams.Count == 0)
            {
                throw new FFmpegArgumentException("输入1不含视频");
            }
            if (v2.VideoStreams.Count == 0)
            {
                throw new FFmpegArgumentException("输入2不含视频");
            }
            Progress = GetProgress(task, true);
            string argument = "-lavfi \"ssim;[0:v][1:v]psnr\" -f null -";
            string vmafModel = Directory.EnumerateFiles(App.ProgramDirectoryPath, "vmaf*.json").FirstOrDefault();
            if (vmafModel != null)
            {
                vmafModel = Path.GetFileName(vmafModel);
                argument = @$"-lavfi ""ssim;[0:v][1:v]psnr;[0:v]setpts=PTS-STARTPTS[reference]; [1:v]setpts=PTS-STARTPTS[distorted]; [distorted][reference]libvmaf=model_path={vmafModel.Replace('\\', '/')}:n_threads={Environment.ProcessorCount}""  -f null -";
            }
            var arg = ArgumentsGenerator.GetArguments(task.Inputs, argument);
            FFmpegOutput += CheckOutput;
            string ssim = null;
            string psnr = null;
            string vmaf = null;
            try
            {
                await RunAsync(arg, $"正在对比 {Path.GetFileName(task.Inputs[0].FilePath)} 和 {Path.GetFileName(task.Inputs[1].FilePath)}", cancellationToken);
                if (ssim == null || psnr == null)
                {
                    throw new Exception("对比视频失败，未识别到对比结果");
                }
                task.Message = ssim + Environment.NewLine + psnr + (vmaf == null ? "" : (Environment.NewLine + vmaf));
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
                if (rSSIM.IsMatch(e.Data))
                {
                    var match = rSSIM.Match(e.Data);
                    ssim = match.Value;
                    logger.Info(task, "对比结果（SSIM）：" + match.Value);
                }
                if (rPSNR.IsMatch(e.Data))
                {
                    var match = rPSNR.Match(e.Data);
                    psnr = match.Value;
                    logger.Info(task, "对比结果（PSNR）：" + match.Value);
                }
                if (rVMAF.IsMatch(e.Data))
                {
                    var match = rVMAF.Match(e.Data);
                    vmaf = match.Value;
                    logger.Info(task, "对比结果（VMAF）：" + match.Value);
                }
            }
        }

        private async Task RunConcatProcessAsync(string tempDir, CancellationToken cancellationToken)
        {
            if (task.Inputs.Count < 2)
            {
                throw new ArgumentException("拼接视频，输入文件必须为2个或更多");
            }
            string message = $"正在拼接：{task.Inputs.Count}个文件";

            if (task.Arguments.Concat == null || task.Arguments.Concat.Type == ConcatType.ViaTs)
            {

                throw new NotSupportedException("取消支持该方法的拼接");
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
                Progress = GetProgress(task);
                GenerateOutputPath(task);
                var input = new InputArguments()
                {
                    FilePath = tempPath,
                    Format = "concat",
                    Extra = "-safe 0"
                };

                string arg = ArgumentsGenerator.GetArguments(new InputArguments[] { input }, "-c copy", task.RealOutput);

                await RunAsync(arg, message, cancellationToken);
            }
        }

        private async Task RunCustomProcessAsync(CancellationToken cancellationToken)
        {
            await RunAsync(task.Arguments.Extra, null, cancellationToken);
        }
    }
}