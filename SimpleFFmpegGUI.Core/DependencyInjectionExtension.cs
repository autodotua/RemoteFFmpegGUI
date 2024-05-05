using Microsoft.Extensions.DependencyInjection;
using SimpleFFmpegGUI.Manager;
using SimpleFFmpegGUI.Model;

namespace SimpleFFmpegGUI
{
    public static class DependencyInjectionExtension
    {
        public static void AddFFmpegServices(this ServiceCollection services)
        {
            services.AddDbContext<FFmpegDbContext>()
                .AddTransient<Logger>()
                .AddTransient<LogManager>()
                .AddTransient<ConfigManager>()
                .AddTransient<PresetManager>()
                .AddTransient<TaskManager>()
                .AddSingleton<QueueManager>();
        }
    }
}