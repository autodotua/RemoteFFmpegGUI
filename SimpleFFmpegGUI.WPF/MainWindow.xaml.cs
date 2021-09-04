using FzLib;
using Microsoft.Extensions.DependencyInjection;
using SimpleFFmpegGUI.Manager;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SimpleFFmpegGUI.WPF
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public QueueManager queue;

        public MainWindowViewModel(QueueManager queue)
        {
            this.queue = queue;
            queue.TaskManagersChanged += (s, e) => this.Notify(nameof(CanStartMainQueue));
        }

        public bool CanStartMainQueue => queue.MainQueueTask == null;

        public event PropertyChangedEventHandler PropertyChanged;
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly QueueManager queue;

        public MainWindowViewModel ViewModel { get; set; }

        public MainWindow(MainWindowViewModel viewModel, QueueManager queue)
        {
            ViewModel = viewModel;
            DataContext = ViewModel;
            InitializeComponent();
            this.queue = queue;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = App.ServiceProvider.GetService<AddTaskWindow>();
            dialog.Owner = this;
            dialog.TaskCreated += (s, e) => taskPanel.Refresh();
            dialog.Show();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (!TaskManager.HasQueueTasks())
            {
                this.CreateMessage().QueueError("没有排队中的任务");
                return;
            }
            queue.StartQueue();
        }

        private void Button_Click1(object sender, RoutedEventArgs e)
        {
            queue.MainQueueManager.Suspend();
        }

        private void Button_Click2(object sender, RoutedEventArgs e)
        {
            queue.MainQueueManager.Resume();
        }
    }
}