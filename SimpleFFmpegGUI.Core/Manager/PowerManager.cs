using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFFmpegGUI.Manager
{
    /// <summary>
    /// 计算机电源管理，控制关机
    /// </summary>
    public class PowerManager
    {
        private bool shutdownAfterQueueFinished = false;
        public bool ShutdownAfterQueueFinished
        {
            get { return shutdownAfterQueueFinished; }
            set
            {
                shutdownAfterQueueFinished = value;
                Logger.Info("收到队列结束后自动关机命令：" + value.ToString());
            }
        }
        private static readonly string shutdownCommand = $"-s -t 180 -c \"{FzLib.Program.App.ProgramName}\"";
        private static readonly string abortShutdownCommand = "-a";


        private void Shutdown(bool shutdown)
        {
            using Process process = new Process();
            process.StartInfo.FileName = "shutdown";
            process.StartInfo.Arguments= shutdown ? shutdownCommand : abortShutdownCommand;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            process.WaitForExit();
            process.Close();
        }

        public void Shutdown()
        {
            Logger.Warn("收到关机命令");
            Shutdown(true);
        }
        public void AbortShutdown()
        {
            Logger.Warn("收到终止关机命令");
            Shutdown(false);
        }
    }
}
