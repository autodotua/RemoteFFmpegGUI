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
        public StatusPanelViewModel(QueueManager queue)
        {
            Queue = queue;
            Tasks.Refresh();
        }

        public TasksAndStatuses Tasks => App.ServiceProvider.GetService<TasksAndStatuses>();

        public QueueManager Queue { get; }

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

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            UITaskInfo task = (sender as Control).DataContext as UITaskInfo;
            Debug.Assert(task != null);
            Debug.Assert(task.ProcessManager != null);
            task.ProcessManager.Cancel();
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
                App.ServiceProvider.GetService<TasksAndStatuses>().DeleteTask();
            }
        }



        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            UITaskInfo task = (sender as Control).DataContext as UITaskInfo;
            Debug.Assert(task != null);
            Debug.Assert(task.ProcessManager != null);
            task.ProcessManager.Suspend();
        }

        private void ResumeButton_Click(object sender, RoutedEventArgs e)
        {
            UITaskInfo task = (sender as Control).DataContext as UITaskInfo;
            Debug.Assert(task != null);
            Debug.Assert(task.ProcessManager != null);
            task.ProcessManager.Resume();

        }
    }
}