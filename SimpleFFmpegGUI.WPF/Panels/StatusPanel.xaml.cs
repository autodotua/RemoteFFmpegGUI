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
    public class StatusPanelViewModel : INotifyPropertyChanged
    {
        private bool created = false;

        public StatusPanelViewModel(QueueManager queue)
        {
            Debug.Assert(!created);
            created = true;
            Queue = queue;
            queue.TaskManagersChanged += (s, e) => this.Notify(nameof(IsRunning));
        }

        public TasksAndStatuses Tasks => App.ServiceProvider.GetService<TasksAndStatuses>();

        public QueueManager Queue { get; }

        public bool IsRunning => Queue.Tasks.Any();

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public partial class StatusPanel : UserControl
    {
        public StatusPanel()
        {
            DataContext = ViewModel;
            InitializeComponent();
        }

        public StatusPanelViewModel ViewModel => App.ServiceProvider.GetService<StatusPanelViewModel>();

        private async void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (!await CommonDialog.ShowYesNoDialogAsync("取消任务", "是否取消任务？"))
            {
                return;
            }
            UITaskInfo task = (sender as Control).DataContext as UITaskInfo;
            Debug.Assert(task != null);
            Debug.Assert(task.ProcessManager != null);
            try
            {
                (sender as FrameworkElement).IsEnabled = false;
                await task.ProcessManager.CancelAsync();
            }
            catch (Exception ex)
            {
                this.CreateMessage().QueueError("取消失败", ex);
            }
            finally
            {
                (sender as FrameworkElement).IsEnabled = true;
            }
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            UITaskInfo task = (sender as Control).DataContext as UITaskInfo;
            Debug.Assert(task != null);
            Debug.Assert(task.ProcessManager != null);
            try
            {
                task.ProcessManager.Suspend();
            }
            catch (Exception ex)
            {
                this.CreateMessage().QueueError("该任务无法暂停", ex);
            }
        }

        private void ResumeButton_Click(object sender, RoutedEventArgs e)
        {
            UITaskInfo task = (sender as Control).DataContext as UITaskInfo;
            Debug.Assert(task != null);
            Debug.Assert(task.ProcessManager != null);
            try
            {
                task.ProcessManager.Resume();
            }
            catch (Exception ex)
            {
                this.CreateMessage().QueueError("恢复失败", ex);
            }
        }
    }
}