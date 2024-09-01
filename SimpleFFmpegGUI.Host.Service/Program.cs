using SimpleFFmpegGUI;
using SimpleFFmpegGUI.Host.Service;

var builder = Host.CreateDefaultBuilder(args);
builder.ConfigureServices(services =>
{
    services.AddHostedService<Worker>();
    services.AddWindowsService(options =>
    {
        options.ServiceName = "SimpleFFmpegHost";
    });
});

SimpleFFmpegGUI.Program.StartAsService(builder);
var host = builder.Build();
host.Run();

