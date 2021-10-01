using FzLib;
using FzLib.Collection;
using Mapster;
using SimpleFFmpegGUI.Dto;
using SimpleFFmpegGUI.Manager;
using SimpleFFmpegGUI.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System;
using System.Windows.Shell;

namespace SimpleFFmpegGUI.WPF.Model
{
    public class TasksAndStatuses : INotifyPropertyChanged
    {
        private ObservableCollection<UITaskInfo> tasks;

        public TasksAndStatuses(QueueManager queue)
        {
            Refresh();
            Queue = queue;
            queue.TaskManagersChanged += Queue_TaskManagersChanged;
        }

        private void Queue_TaskManagersChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                var manager = e.NewItems[0] as FFmpegManager;
                var unstartStatus = new StatusDto(manager.Task);
                var task = Tasks.FirstOrDefault(p => p.Id == manager.Task.Id);
                Debug.Assert(task != null);
                task.UpdateSelf();
                task.ProcessStatus = unstartStatus;
                task.ProcessManager = manager;
                if (manager == Queue.MainQueueManager)
                {
                    Statuses.Insert(0, unstartStatus);
                }
                else
                {
                    Statuses.Add(unstartStatus);
                }
                manager.StatusChanged += Manager_StatusChanged;
            }
            else
            {
                var manager = e.OldItems[0] as FFmpegManager;
                var status = Statuses.FirstOrDefault(p => p.Task.Id == manager.Task.Id);
                Debug.Assert(status != null);

                var task = Tasks.FirstOrDefault(p => p.Id == manager.Task.Id);
                Debug.Assert(task != null);
                task.ProcessManager = null;
                task.ProcessStatus = null;
                task.UpdateSelf();

                Statuses.Remove(status);
                manager.StatusChanged -= Manager_StatusChanged;
                GetMainWindowAnd(async mainWindow =>
                {
                    if (task.Status is TaskStatus.Error or TaskStatus.Cancel)
                    {
                        mainWindow.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Error;
                        await System.Threading.Tasks.Task.Delay(1000);
                    }
                    mainWindow.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
                    Debug.Write("finish");
                });
            }
            ProcessingTasks = Tasks.Where(p => p.ProcessStatus != null).ToList();
        }

        private List<UITaskInfo> processingTasks;

        public List<UITaskInfo> ProcessingTasks
        {
            get => processingTasks;
            private set => this.SetValueAndNotify(ref processingTasks, value, nameof(ProcessingTasks));
        }

        private void Manager_StatusChanged(object sender, EventArgs e)
        {
            var manager = sender as FFmpegManager;
            var newStatus = manager.GetStatus();

            var task = Tasks.FirstOrDefault(p => p.Id == newStatus.Task.Id);
            Debug.Assert(task != null);
            Debug.Assert(task.ProcessStatus != null);

            task.ProcessStatus = newStatus;
            if (manager == Queue.MainQueueManager || Queue.Managers.Count == 1)//主队列，或者只有一个任务，则在状态栏上显示进度
            {
                GetMainWindowAnd(mainWindow =>
                {
                    if (newStatus.HasDetail)
                    {
                        mainWindow.TaskbarItemInfo.ProgressState =
                        manager.Paused ? TaskbarItemProgressState.Paused : TaskbarItemProgressState.Normal;
                        mainWindow.TaskbarItemInfo.ProgressValue = newStatus.Progress.Percent;
                    }
                    else
                    {
                        mainWindow.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Indeterminate;
                    }
                });
            }
        }

        private static void GetMainWindowAnd(Action<MainWindow> action)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var mainWindow = App.ServiceProvider.GetService<MainWindow>();
                Debug.Assert(mainWindow != null);
                action(mainWindow);
            });
        }

        private void Refresh()
        {
            var tasks = TaskManager.GetTasks();
            Tasks = new ObservableCollection<UITaskInfo>(tasks.List.Adapt<List<UITaskInfo>>());
        }

        public ObservableCollection<UITaskInfo> Tasks
        {
            get => tasks;
            set => this.SetValueAndNotify(ref tasks, value, nameof(Tasks));
        }

        public ObservableCollection<StatusDto> Statuses { get; } = new ObservableCollection<StatusDto>();

        public QueueManager Queue { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        private UITaskInfo selectedTask;

        public UITaskInfo SelectedTask
        {
            get => selectedTask;
            set => this.SetValueAndNotify(ref selectedTask, value, nameof(SelectedTask));
        }
    }
}