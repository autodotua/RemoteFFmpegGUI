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
using System.Linq;

namespace SimpleFFmpegGUI.WPF.Model
{ 
    public class TasksAndStatuses : INotifyPropertyChanged
    {
        private ObservableCollection<UITaskInfo> tasks;

        public TasksAndStatuses(QueueManager queue)
        {
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
                UpdateTask(task);
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
                UpdateTask(task);

                Statuses.Remove(status);
                manager.StatusChanged -= Manager_StatusChanged;
            }
            ProcessingTasks = Tasks.Where(p => p.ProcessStatus != null).ToList();
        }

        private List<UITaskInfo> processingTasks;

        public List<UITaskInfo> ProcessingTasks
        {
            get => processingTasks;
            private set => this.SetValueAndNotify(ref processingTasks, value, nameof(ProcessingTasks));
        }

        private void Manager_StatusChanged(object sender, System.EventArgs e)
        {
            var manager = sender as FFmpegManager;
            var newStatus = manager.GetStatus();

            var task = Tasks.FirstOrDefault(p => p.Id == newStatus.Task.Id);
            Debug.Assert(task != null);
            Debug.Assert(task.ProcessStatus != null);

            newStatus.Adapt(task.ProcessStatus);
        }

        public void Refresh()
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

        private void UpdateTask(UITaskInfo task)
        {
            TaskManager.GetTask(task.Id).Adapt(task);
        }

        public void DeleteTask()
        {
            Debug.Assert(SelectedTask != null);
            TaskManager.DeleteTask(SelectedTask.Id, Queue);
            Tasks.Remove(SelectedTask);
        }

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

        private UITaskInfo selectedTask;

        public UITaskInfo SelectedTask
        {
            get => selectedTask;
            set => this.SetValueAndNotify(ref selectedTask, value, nameof(SelectedTask));
        }
    }
}