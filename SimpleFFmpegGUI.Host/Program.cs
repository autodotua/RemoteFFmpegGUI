using CommandLine;
using CommandLine.Text;
using JKang.IpcServiceFramework.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SimpleFFmpegGUI.Model;
using System;
using System.Linq;

namespace SimpleFFmpegGUI
{
    internal class Options
    {
        [Option('p', "pipename", Required = false, HelpText = "命名管道名称")]
        public string PipeName { get; set; }
    }

    public class Program
    {
        private const string DefaultPipeName = "ffpipe";

        private static void Main(string[] args)
        {
            string pipeName = DefaultPipeName;
            Parser.Default.ParseArguments<Options>(args)
                     .WithParsed<Options>(o =>
                     {
                         if (o.PipeName != null)
                         {
                             Console.WriteLine($"管道名设置为： {o.PipeName}");
                             pipeName = o.PipeName;
                         }
                         else
                         {
                             Console.WriteLine($"管道名未设置，默认为为： {DefaultPipeName}");
                         }
                     });
            CreateHostBuilder(pipeName).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string pipeName)
        {
            return Host.CreateDefaultBuilder()
                    .ConfigureServices(services =>
                    {
                        services.AddSingleton<IPipeService, PipeService>();
                    })
                    .ConfigureIpcHost(builder =>
                    {
                        builder.AddNamedPipeEndpoint<IPipeService>(pipeName: pipeName);
                    })
                    .ConfigureLogging(builder =>
                    {
                    });
        }
    }
}