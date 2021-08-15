using CommandLine;
using CommandLine.Text;
using JKang.IpcServiceFramework.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using SimpleFFmpegGUI.Model;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

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
            ConsoleLogger.StartListen();
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
                        builder.AddConsole();
                    });
        }
    }

    public class ConsoleLogger
    {
        private static ConsoleLogger instance;

        public static void StartListen()
        {
            if (instance == null)
            {
                new ConsoleLogger();
            }
        }

        private ConsoleLogger()
        {
            Logger.Log += Logger_Log;
        }

        private void Logger_Log(object sender, LogEventArgs e)
        {
            ConsoleColor defaultColor =Console.ForegroundColor;
            ConsoleColor color = e.Log.Type switch
            {
                'E' => ConsoleColor.Red,
                'D' => defaultColor,
                'I' => defaultColor,
                'W' => ConsoleColor.Yellow,
                'O' => ConsoleColor.Gray,
                _ => defaultColor
            };
            string type = e.Log.Type switch
            {
                'E' => "错误",
                'D' => "调试",
                'I' => "信息",
                'W' => "警告",
                'O' => "输出",
                _ => e.Log.Type.ToString().PadLeft(2)
            };
            string time = e.Log.Time.ToString("yyyy-MM-dd HH:mm:ss");
            Console.Write(time);
            Console.Write("    \t");
            Console.ForegroundColor = color;
            Console.Write(type);
            Console.ForegroundColor = defaultColor;
            Console.Write("    \t");
            Console.Write(e.Log.Message);
            Console.WriteLine();
        }
    }
}