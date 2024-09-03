using SimpleFFmpegGUI;
using SimpleFFmpegGUI.Host.Service;
using System.Diagnostics;

var builder = Host.CreateDefaultBuilder(args);
builder.ConfigureServices(services =>
{
    services.AddHostedService<Worker>();
    services.AddWindowsService(options =>
    {
        options.ServiceName = "SimpleFFmpegHost";
    });
});
Directory.SetCurrentDirectory(Path.GetDirectoryName(Environment.ProcessPath));
Startup.InitializeServices(builder);
builder.Build().Run();

