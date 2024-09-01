using System.Diagnostics;
using System.IO;

namespace SimpleFFmpegGUI.Host.Service
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private Process _process;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Perform background work here
                await Task.Delay(1000, stoppingToken); // Example delay
            }
            //try
            //{
            //    string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SimpleFFmpegGUI.Host.exe");
            //    _process = new Process
            //    {
            //        StartInfo = new ProcessStartInfo
            //        {
            //            FileName = exePath,
            //            UseShellExecute = false,
            //            RedirectStandardOutput = true,
            //            RedirectStandardError = true,
            //            CreateNoWindow = true
            //        }
            //    };
            //    _process.Start();

            //    _logger.LogInformation("Process started successfully.");

            //    while (!stoppingToken.IsCancellationRequested)
            //    {
            //        if (_process.HasExited)
            //        {
            //            _logger.LogWarning("Process exited unexpectedly.");
            //            break;
            //        }

            //        await Task.Delay(1000, stoppingToken); // Adjust delay as needed
            //    }
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex, "An error occurred while starting the process.");
            //}
            //finally
            //{
            //    if (_process != null && !_process.HasExited)
            //    {
            //        _process.Kill();
            //        _logger.LogInformation("Process terminated.");
            //    }
            //}
        }
    }
}
