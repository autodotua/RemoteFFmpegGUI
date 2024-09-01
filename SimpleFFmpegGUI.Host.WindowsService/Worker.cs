using System.Diagnostics;
using System.IO;

namespace SimpleFFmpegGUI.Host.Service
{
    public class Worker : BackgroundService
    {
        //private readonly ILogger<Worker> _logger;
        //private Process _process;

        //public Worker(ILogger<Worker> logger)
        //{
        //    _logger = logger;
        //}

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken); 
            }
        }
    }
}
