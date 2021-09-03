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
    public class TaskListViewModel : INotifyPropertyChanged
    {
        public TaskListViewModel(QueueManager queue)
        {
            Queue = queue;
            Refresh();
        }

        public void Refresh()
        {
            var tasks = TaskManager.GetTasks();
            Tasks = new ObservableCollection<TaskInfoWithUI>(tasks.List.Adapt<List<TaskInfoWithUI>>());
        }

        private ObservableCollection<TaskInfoWithUI> tasks;

        public ObservableCollection<TaskInfoWithUI> Tasks
        {
            get => tasks;
            set => this.SetValueAndNotify(ref tasks, value, nameof(Tasks));
        }

        private TaskInfoWithUI selectedTask;

        public TaskInfoWithUI SelectedTask
        {
            get => selectedTask;
            set => this.SetValueAndNotify(ref selectedTask, value, nameof(SelectedTask));
        }

        public QueueManager Queue { get; }

        private void UpdateTask(TaskInfoWithUI task)
        {
            TaskManager.GetTask(task.Id).Adapt(task);
        }

        public void DeleteTask()
        {
            Debug.Assert(SelectedTask != null);
            TaskManager.DeleteTask(SelectedTask.Id, Queue);
            Tasks.Remove(SelectedTask);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void ResetTask()
        {
            Debug.Assert(SelectedTask != null);
            TaskManager.ResetTask(SelectedTask.Id, Queue);
            UpdateTask(SelectedTask);
        }

        public void CancelTask()
        {
            Debug.Assert(SelectedTask != null);
            TaskManager.CancelTask(SelectedTask.Id, Queue);
            UpdateTask(SelectedTask);
        }
    }

    public partial class TaskList : UserControl
    {
        public TaskList()
        {
            DataContext = ViewModel;
            InitializeComponent();
        }

        public TaskListViewModel ViewModel => App.ServiceProvider.GetService<TaskListViewModel>();

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.CancelTask();
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            bool delete = true;
            if (ViewModel.SelectedTask.Status == SimpleFFmpegGUI.Model.TaskStatus.Processing)
            {
                delete = await CommonDialog.ShowYesNoDialogAsync("删除", "任务正在处理，是否删除？");
            }
            if (delete)
            {
                ViewModel.DeleteTask();
            }
        }

        private void CloneButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ResetTask();
        }

        public void Refresh()
        {
            ViewModel.Refresh();
        }
    }
}