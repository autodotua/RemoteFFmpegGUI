using CommandLine;
using Microsoft.Extensions.Hosting;
using SimpleFFmpegGUI;
using System.Runtime.InteropServices;

string pipeName = Startup.DefaultPipeName;
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
                     Console.WriteLine($"管道名未设置，默认为： {Startup.DefaultPipeName}");
                 }
                 if (o.WorkingDirectoryHere)
                 {
                     FzLib.Program.App.SetWorkingDirectoryToAppPath();
                     Console.WriteLine("工作目录设置为程序目录：" + FzLib.Program.App.ProgramDirectoryPath);
                 }
             });

var builder = Host.CreateDefaultBuilder(args);
ConsoleLogger.StartListen();
Startup.InitializeServices(builder,pipeName);
builder.Build().Run();