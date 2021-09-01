using Microsoft.Extensions.DependencyInjection;
using SimpleFFmpegGUI.Manager;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
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

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            ServiceProvider = serviceCollection.BuildServiceProvider();
            MainWindow = ServiceProvider.GetService<MainWindow>();
            MainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<QueueManager>();

            services.AddSingleton<MainWindow>();
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<TaskListViewModel>();

            services.AddTransient<AddTaskWindow>();

            services.AddTransient<CodeArgumentsPanelViewModel>();
            services.AddTransient<FileIOPanelViewModel>();
            services.AddSingleton<AddTaskWindowViewModel>();
            services.AddTransient<PresetsPanelViewModel>();
        }
    }
}