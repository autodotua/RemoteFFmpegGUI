using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleFFmpegGUI.Manager
{
    public class FFmpegProcess
    {
        private readonly Process process = new Process();

        public int Id
        {
            get
            {
                if (!started)
                {
                    throw new Exception("进程还未开始运行");
                }
                return process.Id;
            }
        }

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
            };
            process.EnableRaisingEvents = true;
            process.OutputDataReceived += Process_OutputDataReceived;
            process.ErrorDataReceived += Process_OutputDataReceived;
        }

        private bool started = false;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (started)
            {
                throw new Exception("已经开始运行，不可再次运行");
            }
            started = true;
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(process.Kill);
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.Exited += (s, e) =>
            {
                try
                {
                    if (process.ExitCode == 0)
                    {
                        tcs.SetResult(true);
                    }
                    else
                    {
                        tcs.SetException(new Exception($"进程退出返回错误退出码：" + process.ExitCode));
                    }
                    Task.Delay(10000).ContinueWith(t => process.Dispose());
                }
                catch (Exception ex)
                {
                    tcs.SetException(new Exception($"进程处理程序发生错误：" + ex.Message, ex));
                }
            };
            return tcs.Task;
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
            {
                return;
            }
            Output?.Invoke(this, new FFmpegOutputEventArgs(e.Data));
        }

        public event EventHandler<FFmpegOutputEventArgs> Output;
    }
}