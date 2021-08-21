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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SimpleFFmpegGUI
{
    internal class Options
    {
        [Option('p', Required = false, HelpText = "命名管道名称")]
        public string PipeName { get; set; }

        [Option('s', Default = false, Required = false, HelpText = "注册开机启动")]
        public bool RegisterStartup { get; set; }

        [Option('u', Default = false, Required = false, HelpText = "取消开机启动")]
        public bool UnregistereStartup { get; set; }

        [Option('d', Default = false, Required = false, HelpText = "设置工作目录为程序所在目录")]
        public bool WorkingDirectoryHere { get; set; }
    }

    public class Program
    {
        private const string DefaultPipeName = "ffpipe";

        private static void Main(string[] args)
        {
            string pipeName = DefaultPipeName;
            Parser.Default.ParseArguments<Options>(args)
                     .WithParsed(o =>
                     {
                         if (o.PipeName != null)
                         {
                             Console.WriteLine($"管道名设置为： {o.PipeName}");
                             pipeName = o.PipeName;
                         }
                         else
                         {
                             Console.WriteLine($"管道名未设置，默认为： {DefaultPipeName}");
                         }
                         if (o.RegisterStartup)
                         {
                             if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                             {
                                 throw new PlatformNotSupportedException("开机自启仅支持Windows");
                             }
                             if (FzLib.Program.Startup.IsRegistryKeyExist() != FzLib.IO.ShortcutStatus.Exist)
                             {
                                 List<string> args = new List<string>();
                                 if (o.WorkingDirectoryHere)
                                 {
                                     args.Add("-d");
                                 }
                                 if (o.PipeName != null)
                                 {
                                     args.Add("-p");
                                     args.Add(o.PipeName);
                                 }
                                 FzLib.Program.Startup.CreateRegistryKey(string.Join(' ', args));
                                 Console.WriteLine("已注册开机自启，参数为" + string.Join(' ', args));
                             }
                             else
                             {
                                 Console.WriteLine("已经是开机自启，无需注册");
                             }
                             Console.WriteLine("按任意键退出");
                             Console.ReadKey();
                             Environment.Exit(0);
                         }
                         if (o.UnregistereStartup)
                         {
                             if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                             {
                                 throw new PlatformNotSupportedException("开机自启仅支持Windows");
                             }
                             if (FzLib.Program.Startup.IsRegistryKeyExist() == FzLib.IO.ShortcutStatus.Exist)
                             {
                                 FzLib.Program.Startup.DeleteRegistryKey();
                                 Console.WriteLine("已取消开机自启");
                             }
                             else
                             {
                                 Console.WriteLine("未注册开机自启，无需取消");
                             }
                             Console.WriteLine("按任意键退出");
                             Console.ReadKey();
                             Environment.Exit(0);
                         }
                         if (o.WorkingDirectoryHere)
                         {
                             FzLib.Program.App.SetWorkingDirectoryToAppPath();
                             Console.WriteLine("工作目录设置为程序目录：" + FzLib.Program.App.ProgramDirectoryPath);
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
                        builder.AddNamedPipeEndpoint<IPipeService>(p =>
                        {
                            p.PipeName = pipeName;
                            p.IncludeFailureDetailsInResponse = true;
                        });
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
            ConsoleColor defaultColor = Console.ForegroundColor;
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