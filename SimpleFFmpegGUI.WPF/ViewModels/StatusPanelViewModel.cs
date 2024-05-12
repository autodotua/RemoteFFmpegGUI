using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FzLib;
using Microsoft.Extensions.DependencyInjection;
using ModernWpf.FzExtension.CommonDialog;
using SimpleFFmpegGUI.Manager;
using SimpleFFmpegGUI.WPF.Model;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SimpleFFmpegGUI.WPF.ViewModels
{
    public partial class StatusPanelViewModel : ViewModelBase
    {
        private bool created = false;

        [ObservableProperty]
        private bool isEnabled = true;

        public StatusPanelViewModel(QueueManager queue, TasksAndStatuses tasks)
        {
            Debug.Assert(!created);
            created = true;
            Queue = queue;
            Tasks = tasks;
            queue.TaskManagersChanged += (s, e) => this.Notify(nameof(IsRunning));
        }

        public bool IsRunning => Queue.Tasks.Any();

        public QueueManager Queue { get; }

        public TasksAndStatuses Tasks { get; }
        [RelayCommand]
        private async Task CancelAsync(UITaskInfo task)
        {
            if (!await CommonDialog.ShowYesNoDialogAsync("取消任务", "是否取消任务？"))
            {
                return;
            }
            Debug.Assert(task != null);
            if (task == null)
            {
                return;
            }
            Debug.Assert(task.ProcessManager != null);
            try
            {
                IsEnabled = false;
                await task.ProcessManager.CancelAsync();
            }
            catch (Exception ex)
            {
                QueueErrorMessage("取消失败", ex);
            }
            finally
            {
                IsEnabled = true;
            }
        }

        [RelayCommand]
        private void Pause(UITaskInfo task)
        {
            Debug.Assert(task != null);
            Debug.Assert(task.ProcessManager != null);
            try
            {
                task.ProcessManager.Suspend();
            }
            catch (Exception ex)
            {
                QueueErrorMessage("该任务无法暂停", ex);
            }
        }

        [RelayCommand]
        private void Resume(UITaskInfo task)
        {
            Debug.Assert(task != null);
            Debug.Assert(task.ProcessManager != null);
            try
            {
                task.ProcessManager.Resume();
            }
            catch (Exception ex)
            {
                QueueErrorMessage("恢复失败", ex);
            }
        }
    }
}