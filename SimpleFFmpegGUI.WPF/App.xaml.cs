using FzLib.Program.Runtime;
using Microsoft.Extensions.DependencyInjection;
using SimpleFFmpegGUI.Manager;
using SimpleFFmpegGUI.WPF.Model;
using SimpleFFmpegGUI.WPF.Panels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SimpleFFmpegGUI.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static ServiceProvider ServiceProvider { get; private set; }

        public static DateTime AppStartTime { get; } = DateTime.Now;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

#if !DEBUG

            UnhandledException.RegistAll();
            UnhandledException.UnhandledExceptionCatched += UnhandledException_UnhandledExceptionCatched;

#endif

            Unosquare.FFME.Library.FFmpegDirectory = FzLib.Program.App.ProgramDirectoryPath;
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            ServiceProvider = serviceCollection.BuildServiceProvider();

            MainWindow = ServiceProvider.GetService<MainWindow>();
            MainWindow.Show();
        }

        private void UnhandledException_UnhandledExceptionCatched(object sender, FzLib.Program.Runtime.UnhandledExceptionEventArgs e)
        {
            try
            {
                File.AppendAllText("error.log", DateTime.Now.ToString() + Environment.NewLine + e.Exception.ToString() + Environment.NewLine + Environment.NewLine);
            }
            catch (Exception ex)
            {
            }
            MessageBox.Show(e.Exception.ToString(), FzLib.Program.App.ProgramName + " - 未捕获的异常", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<Config>();

            services.AddSingleton<QueueManager>();
            services.AddSingleton<TasksAndStatuses>();
            services.AddSingleton<AllTasks>();

            services.AddSingleton<MainWindow>();
            services.AddTransient<MainWindowViewModel>();

            services.AddTransient<AddTaskWindow>();
            services.AddTransient<AddTaskWindowViewModel>();

            services.AddTransient<MediaInfoWindow>();
            services.AddTransient<MediaInfoWindowViewModel>();

            services.AddTransient<LogsWindow>();
            services.AddTransient<LogsWindowViewModel>();

            services.AddTransient<ClipWindow>();
            services.AddTransient<ClipWindowViewModel>();

            services.AddTransient<TasksWindow>();
            services.AddTransient<TasksWindowViewModel>();

            services.AddTransient<SettingWindow>();
            services.AddTransient<SettingWindowViewModel>();

            services.AddTransient<PresetsWindow>();
            services.AddTransient<PresetsWindowViewModel>();

            services.AddTransient<TaskListViewModel>();
            services.AddTransient<CodeArgumentsPanelViewModel>();
            services.AddTransient<FileIOPanelViewModel>();
            services.AddTransient<PresetsPanelViewModel>();
            services.AddTransient<StatusPanelViewModel>();
        }
    }
}