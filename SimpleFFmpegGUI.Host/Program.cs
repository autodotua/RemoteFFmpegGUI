using CommandLine;
using CommandLine.Text;
using FzLib.Program.Runtime;
using JKang.IpcServiceFramework.Hosting;
using log4net;
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

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config", Watch = true)]

namespace SimpleFFmpegGUI
{
    public class Program
    {
        private const string DefaultPipeName = "ffpipe";

        public static ILog AppLog { get; private set; }

        public static IHostBuilder CreateHostBuilder(string pipeName)
        {
            try
            {
                FFmpegDbContext.Migrate();
            }
            catch (Exception ex)
            {
                AppLog.Error(ex);
                Console.WriteLine("数据库迁移失败：" + ex);
                Console.WriteLine("程序终止");
                Console.ReadKey();
                Environment.Exit(-1);
                return null;
            }

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

        public static void Main(string[] args)
        {
            InitializeLogs();
#if !DEBUG
            UnhandledExceptionCatcher catcher = new UnhandledExceptionCatcher();
            catcher.RegisterTaskCatcher();
            catcher.RegisterThreadsCatcher();
            catcher.UnhandledExceptionCatched += UnhandledException_UnhandledExceptionCatched;
#endif
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

        private static void InitializeLogs()
        {
            //本地日志
            AppLog = LogManager.GetLogger(typeof(Program));
            AppLog.Info("程序启动");

            //数据库日志
            Logger.Log += Logger_Log;
            Logger.LogSaveFailed += Logger_LogSaveFailed;
            void Logger_Log(object sender, LogEventArgs e)
            {
                switch (e.Log.Type)
                {
                    case 'E': AppLog.Error(e.Log.Message); break;
                    case 'W': AppLog.Warn(e.Log.Message); break;
                    case 'I': AppLog.Info(e.Log.Message); break;
                }
            }
            void Logger_LogSaveFailed(object sender, ExceptionEventArgs e)
            {
                AppLog.Error(e.Exception.Message, e.Exception);
            }
        }

        private static void UnhandledException_UnhandledExceptionCatched(object sender, FzLib.Program.Runtime.UnhandledExceptionEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"程序发生未捕获的异常，停止运行：\r\n {e.Exception}");
            AppLog.Error(e.Exception);
            Console.WriteLine("按任意键退出");
            Console.ReadKey();
        }
    }
}