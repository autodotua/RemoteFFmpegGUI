using JKang.IpcServiceFramework.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleFFmpegGUI.Model;
using System;
using System.Linq;

namespace SimpleFFmpegGUI.Host
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            using (var db = new FFmpegDbContext())
            {
                foreach (var item in db.Tasks.Where(p => p.Status == TaskStatus.Processing))
                {
                    item.Status = TaskStatus.Error;
                    item.Message = "状态异常：启动时处于正在运行状态";
                }
                db.SaveChanges();
            }
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                    .ConfigureServices(services =>
                    {
                        services.AddScoped<IPipeService, PipeService>();
                    })
                    .ConfigureIpcHost(builder =>
                    {
                        // configure IPC endpoints
                        builder.AddNamedPipeEndpoint<IPipeService>(pipeName: "pipeinternal");
                    })
                    .ConfigureLogging(builder =>
                    {
                    });
        }
    }
}