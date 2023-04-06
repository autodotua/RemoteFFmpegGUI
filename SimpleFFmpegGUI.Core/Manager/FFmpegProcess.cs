using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleFFmpegGUI.Manager
{
    /// <summary>
    /// FFmpeg进程
    /// </summary>
    public class FFmpegProcess
    {
        private readonly Process process = new Process();

        private bool started = false;

        public FFmpegProcess(string argument)
        {
            process.StartInfo = new ProcessStartInfo()
            {
                FileName = "ffmpeg",
                Arguments = argument,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                StandardErrorEncoding = System.Text.Encoding.UTF8,
                StandardOutputEncoding = System.Text.Encoding.UTF8,
            };
            process.EnableRaisingEvents = true;
            process.OutputDataReceived += Process_OutputDataReceived;
            process.ErrorDataReceived += Process_OutputDataReceived;
        }

        public event EventHandler<FFmpegOutputEventArgs> Output;

        /// <summary>
        /// CPU使用率
        /// </summary>
        public double CpuUsage => process.HasExited ? process.TotalProcessorTime.TotalSeconds / Environment.ProcessorCount / RunningTime.TotalSeconds : throw new Exception("进程还未结束");

        /// <summary>
        /// 进程ID
        /// </summary>
        public int Id => !started ? throw new Exception("进程还未开始运行") : process.Id;

        /// <summary>
        /// 运行时间
        /// </summary>
        public TimeSpan RunningTime => process.HasExited ? process.ExitTime - process.StartTime : throw new Exception("进程还未结束");

        private TaskCompletionSource<bool> tcs;
        /// <summary>
        /// 启动进程
        /// </summary>
        /// <param name="workingDir"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public Task StartAsync(string workingDir, CancellationToken? cancellationToken)
        {
            if (started)
            {
                throw new Exception("已经开始运行，不可再次运行");
            }
            started = true;

            if (!string.IsNullOrEmpty(workingDir))
            {
                //2Pass时会生成文件名相同的临时文件，如果多个FFmpeg一起运行会冲突，因此需要设置单独的工作目录
                process.StartInfo.WorkingDirectory = workingDir;
            }
            tcs = new TaskCompletionSource<bool>();
            bool exit = false;
            cancellationToken?.Register(() =>
            {
                if (!exit)
                {
                    exit = true;
                    process.Kill();
                }
            });
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.Exited += async (s, e) =>
             {
                 exit = true;
                 try
                 {
                     await Task.Delay(1000);
                     if (process.ExitCode == 0)
                     {
                         tcs.SetResult(true);
                     }
                     else if (exit)
                     {
                         tcs.SetException(new TaskCanceledException("进程被取消"));
                     }
                     else
                     {
                         tcs.SetException(new Exception($"进程退出：" + process.ExitCode));
                     }
                     await Task.Delay(10000);
                     process.Dispose();
                 }
                 catch (Exception ex)
                 {
                     tcs.SetException(new Exception($"进程处理程序发生错误：" + ex.Message, ex));
                 }
             };
            return tcs.Task;
        }

        public Task WaitForExitAsync()
        {
            if(tcs==null)
            {
                throw new Exception("进程还未开始");
            }
            return tcs.Task;
        }

        /// <summary>
        /// 进程输出事件接收
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
            {
                return;
            }
            Output?.Invoke(this, new FFmpegOutputEventArgs(e.Data));
        }
    }
}