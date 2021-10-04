using FzLib;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using ModernWpf.FzExtension.CommonDialog;
using SimpleFFmpegGUI.Manager;
using SimpleFFmpegGUI.Model;
using SimpleFFmpegGUI.WPF;
using SimpleFFmpegGUI.WPF.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace SimpleFFmpegGUI.WPF.Panels
{
    public class TaskListViewModel : INotifyPropertyChanged
    {
        public TaskListViewModel(QueueManager queue)
        {
            Queue = queue;
        }

        public TaskCollectionBase Tasks => ShowAllTasks ?
               App.ServiceProvider.GetService<AllTasks>()
            : App.ServiceProvider.GetService<TasksAndStatuses>();

        public QueueManager Queue { get; }
        public bool ShowAllTasks { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public partial class TaskList : UserControl
    {
        public TaskList()
        {
            InitializeComponent();
            (Content as FrameworkElement).DataContext = ViewModel;
        }

        public bool ShowAllTasks
        {
            get => ViewModel.ShowAllTasks;
            set => ViewModel.ShowAllTasks = value;
        }

        public TaskListViewModel ViewModel { get; } = App.ServiceProvider.GetService<TaskListViewModel>();

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var task = (sender as FrameworkElement).DataContext as UITaskInfo;
            Debug.Assert(task != null);
            TaskManager.CancelTask(task.Id, ViewModel.Queue);
            task.UpdateSelf();
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            bool delete = true;
            if (App.ServiceProvider.GetService<TasksAndStatuses>().SelectedTask.Status == SimpleFFmpegGUI.Model.TaskStatus.Processing)
            {
                delete = await CommonDialog.ShowYesNoDialogAsync("删除", "任务正在处理，是否删除？");
            }
            if (delete)
            {
                var task = (sender as FrameworkElement).DataContext as UITaskInfo;
                App.ServiceProvider.GetService<TasksAndStatuses>().Tasks.Remove(task);
                Debug.Assert(task != null);
                TaskManager.DeleteTask(task.Id, ViewModel.Queue);
                task.UpdateSelf();
            }
        }

        private void CloneButton_Click(object sender, RoutedEventArgs e)
        {
            AddTaskWindow win = App.ServiceProvider.GetService<AddTaskWindow>();
            win.SetAsClone(((sender as FrameworkElement).DataContext as UITaskInfo).ToTask());
            win.Owner = Window.GetWindow(this);
            win.Show();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            var task = (sender as FrameworkElement).DataContext as UITaskInfo;
            Debug.Assert(task != null);
            TaskManager.ResetTask(task.Id, ViewModel.Queue);
            task.UpdateSelf();
            App.ServiceProvider.GetService<TasksAndStatuses>().NotifyTaskReseted(task);
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var task = (sender as FrameworkElement).DataContext as UITaskInfo;
                Debug.Assert(task != null);
                ViewModel.Queue.StartStandalone(task.Id);
                task.UpdateSelf();
            }
            catch (Exception ex)
            {
                this.CreateMessage().QueueError("启动失败", ex);
            }
        }

        private void ArgumentsButton_Click(object sender, RoutedEventArgs e)
        {
            var task = (sender as FrameworkElement).DataContext as UITaskInfo;
            Debug.Assert(task != null);
            var panel = new CodeArgumentsPanel
            {
                IsHitTestVisible = false
            };
            panel.ViewModel.Update(task.Type, task.Arguments);
            ScrollViewer scr = new ScrollViewer();
            scr.Content = panel;
            Window win = new Window()
            {
                Owner = Window.GetWindow(this),
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = scr,
                Width = 600,
                Height = 800,
                Title = "详细参数 - FFmpeg工具箱"
            };
            win.Show();
        }

        private void OpenOutputFileButton_Click(object sender, RoutedEventArgs e)
        {
            var task = (sender as FrameworkElement).DataContext as UITaskInfo;
            Debug.Assert(task != null);
            OpenOutputFileOrFolder(task, false);
        }

        private void OpenOutputFileOrFolder(UITaskInfo task, bool folder)
        {
            if (string.IsNullOrWhiteSpace(task.RealOutput))
            {
                this.CreateMessage().QueueError("输出为空");
                return;
            }
            if (!File.Exists(task.RealOutput))
            {
                this.CreateMessage().QueueError("找不到目标文件：" + task.RealOutput);
                return;
            }
            OpenFileOrFolder(task.RealOutput, folder);
        }

        private void OpenOutputDirButton_Click(object sender, RoutedEventArgs e)
        {
            var task = (sender as FrameworkElement).DataContext as UITaskInfo;
            Debug.Assert(task != null);

            OpenOutputFileOrFolder(task, true);
        }

        private void LogsButton_Click(object sender, RoutedEventArgs e)
        {
            var task = (sender as FrameworkElement).DataContext as UITaskInfo;
            Debug.Assert(task != null);
            var logWin = App.ServiceProvider.GetService<LogsWindow>();
            logWin.FillLogs(task.Id);
            logWin.Show();
        }

        private void OpenInputFileOrFolder(InputArguments input, bool folder)
        {
            string path = input.FilePath;
            if (!File.Exists(path))
            {
                this.CreateMessage().QueueError("找不到文件：" + path);
                return;
            }
            OpenFileOrFolder(path, folder);
        }

        private void OpenInputFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if ((e.OriginalSource as MenuItem).DataContext is InputArguments i)
            {
                OpenInputFileOrFolder(i, false);
            }
        }

        private void OpenInputDirMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if ((e.OriginalSource as MenuItem).DataContext is InputArguments i)
            {
                OpenInputFileOrFolder(i, true);
            }
        }

        private void OpenFileOrFolder(string path, bool folder)
        {
            var p = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    UseShellExecute = true
                }
            };
            if (folder)
            {
                p.StartInfo.FileName = "explorer.exe";
                p.StartInfo.Arguments = @$"/select,""{path}""";
            }
            else
            {
                p.StartInfo.FileName = path;
            }
            p.Start();
        }
    }
}