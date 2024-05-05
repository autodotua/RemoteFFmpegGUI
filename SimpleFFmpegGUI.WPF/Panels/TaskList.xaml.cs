using FzLib.WPF;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using ModernWpf.FzExtension.CommonDialog;
using SimpleFFmpegGUI.Manager;
using SimpleFFmpegGUI.Model;
using SimpleFFmpegGUI.WPF;
using SimpleFFmpegGUI.WPF.Model;
using SimpleFFmpegGUI.WPF.Pages;
using SimpleFFmpegGUI.WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using TaskStatus = SimpleFFmpegGUI.Model.TaskStatus;

namespace SimpleFFmpegGUI.WPF.Panels
{
    public partial class TaskList : UserControl
    {
        public TaskList()
        {
            InitializeComponent();
            ViewModel = this.SetDataContext<TaskListViewModel>();
        }


        public static readonly DependencyProperty ShowAllTasksProperty = DependencyProperty.Register(
            nameof(ShowAllTasks), typeof(bool),
            typeof(TaskList)
            );

        public bool ShowAllTasks
        {
            get => (bool)GetValue(ShowAllTasksProperty);
            set => SetValue(ShowAllTasksProperty, value);
        }

        public TaskListViewModel ViewModel { get; }

        private void ArgumentsButton_Click(object sender, RoutedEventArgs e)
        {
            var task = ViewModel.Tasks.SelectedTask;
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
                Owner = this.GetWindow(),
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = scr,
                Width = 600,
                Height = 800,
                Title = "详细参数 - FFmpeg工具箱"
            };
            win.Show();
        }

        private async void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var tasks = App.ServiceProvider.GetRequiredService<TasksAndStatuses>().SelectedTasks;
            var tm = App.ServiceProvider.GetRequiredService<TaskManager>();
            Debug.Assert(tasks.Count > 0);
            if (tasks.Any(p => p.Status == TaskStatus.Processing))
            {
                if (!await CommonDialog.ShowYesNoDialogAsync("取消任务", "任务正在执行，是否取消？"))
                {
                    return;
                }
            }
            try
            {
                IsEnabled = false;
                foreach (var task in tasks)
                {
                    await tm.CancelTaskAsync(task.Id);
                    await task.UpdateSelfAsync();
                }
            }
            catch (Exception ex)
            {
                this.CreateMessage().QueueError("取消失败", ex);
            }
            finally
            {
                IsEnabled = true;
            }

            ViewModel.NotifyCanExecute();
        }

        private void CloneButton_Click(object sender, RoutedEventArgs e)
        {
            this.GetWindow<MainWindow>().AddNewTab<AddTaskPage>()
                .SetAsClone(((sender as FrameworkElement).DataContext as UITaskInfo).ToTask());
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
            bool delete = true;
            if (App.ServiceProvider.GetService<TasksAndStatuses>().SelectedTask.Status == SimpleFFmpegGUI.Model.TaskStatus.Processing)
            {
                delete = await CommonDialog.ShowYesNoDialogAsync("删除", "任务正在处理，是否删除？");
            }
            if (delete)
            {
                var task = ViewModel.Tasks.SelectedTask;
                App.ServiceProvider.GetService<TasksAndStatuses>().Tasks.Remove(task);
                Debug.Assert(task != null);
                //TaskManager.DeleteTask(task.Id, ViewModel.Queue);
                //task.UpdateSelf();
            }
        }

        private void LogsButton_Click(object sender, RoutedEventArgs e)
        {
            var task = ViewModel.Tasks.SelectedTask;
            Debug.Assert(task != null);

            this.GetWindow<MainWindow>().AddNewTab<LogsPage>().FillLogs(task.Id);
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

        private void OpenInputDirMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if ((e.OriginalSource as MenuItem).DataContext is InputArguments i)
            {
                OpenInputFileOrFolder(i, true);
            }
        }

        private void OpenInputFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if ((e.OriginalSource as MenuItem).DataContext is InputArguments i)
            {
                OpenInputFileOrFolder(i, false);
            }
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

        private void OpenOutputDirButton_Click(object sender, RoutedEventArgs e)
        {
            var task = ViewModel.Tasks.SelectedTask;
            Debug.Assert(task != null);

            OpenOutputFileOrFolder(task, true);
        }

        private void OpenOutputFileButton_Click(object sender, RoutedEventArgs e)
        {
            var task = ViewModel.Tasks.SelectedTask;
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
   
        private void UpdateDetailHeight()
        {
            bdDetail.Height = App.ServiceProvider.GetService<MainWindow>().IsUiCompressMode && !ShowAllTasks ? 108 : double.NaN;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateDetailHeight();
            App.ServiceProvider.GetService<MainWindow>().UiCompressModeChanged +=
                (s, e) => UpdateDetailHeight();
        }

        private void Tasks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.NotifyCanExecute();
        }
    }
}