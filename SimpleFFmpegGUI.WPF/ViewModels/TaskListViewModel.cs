using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FzLib;
using Microsoft.Extensions.DependencyInjection;
using ModernWpf.FzExtension.CommonDialog;
using SimpleFFmpegGUI.Manager;
using SimpleFFmpegGUI.WPF.Messages;
using SimpleFFmpegGUI.WPF.Model;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TaskStatus = SimpleFFmpegGUI.Model.TaskStatus;

namespace SimpleFFmpegGUI.WPF.ViewModels
{
    public partial class TaskListViewModel(QueueManager queue, TasksAndStatuses tasksAndStatuses, TaskManager taskManager) : ViewModelBase
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Tasks))]
        private bool showAllTasks;

        private readonly QueueManager queue = queue;
        private readonly TasksAndStatuses tasksAndStatuses = tasksAndStatuses;
        private readonly TaskManager taskManager = taskManager;

        public bool CanCancel => Tasks.SelectedTasks.All(p => p.Status is TaskStatus.Queue or TaskStatus.Processing);

        public bool CanReset => Tasks.SelectedTasks.All(p => p.Status is TaskStatus.Done or TaskStatus.Cancel or TaskStatus.Error);

        public bool CanStart => Tasks.SelectedTasks.All(p => p.Status is TaskStatus.Queue);

        public void NotifyCanExecute()
        {
            this.Notify(nameof(CanCancel), nameof(CanReset), nameof(CanStart));
        }

        public TaskCollectionBase Tasks => ShowAllTasks ?
                                       App.ServiceProvider.GetService<AllTasks>()
            : App.ServiceProvider.GetService<TasksAndStatuses>();

        public SelectionMode SelectionMode => ShowAllTasks ? SelectionMode.Single : SelectionMode.Extended;

        [RelayCommand]
        private async Task ResetAsync()
        {
            var tasks = tasksAndStatuses.SelectedTasks;
            Debug.Assert(tasks.Count > 0);
            foreach (var task in tasks)
            {
                await taskManager.ResetTaskAsync(task.Id);
                await task.UpdateSelfAsync();
                App.ServiceProvider.GetService<TasksAndStatuses>().NotifyTaskReseted(task);
            }

            NotifyCanExecute();
        }

        [RelayCommand]
        private async Task StartStandaloneAsync()
        {
            try
            {
                var tasks = tasksAndStatuses.SelectedTasks;
                Debug.Assert(tasks.Count > 0);
                foreach (var task in tasks)
                {
                    queue.StartStandalone(task.Id);
                    await task.UpdateSelfAsync();
                }
            }
            catch (Exception ex)
            {
             QueueErrorMessage("启动失败", ex);
            }

            NotifyCanExecute();
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            var tasks = tasksAndStatuses.SelectedTasks;
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
                SendMessage(new WindowEnableMessage(false));
                foreach (var task in tasks)
                {
                    await taskManager.CancelTaskAsync(task.Id);
                    await task.UpdateSelfAsync();
                }
            }
            catch (Exception ex)
            {
                QueueErrorMessage("取消失败", ex);
            }
            finally
            {
                SendMessage(new WindowEnableMessage(true));
            }

            NotifyCanExecute();
        }
    }
}