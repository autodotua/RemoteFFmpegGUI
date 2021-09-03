using FzLib;
using Mapster;
using SimpleFFmpegGUI.Dto;
using SimpleFFmpegGUI.Manager;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace SimpleFFmpegGUI.WPF.Model
{
    public class TasksAndStatuses : INotifyPropertyChanged
    {
        private ObservableCollection<TaskInfoWithUI> tasks;

        public TasksAndStatuses(QueueManager queue)
        {
            Queue = queue;
            queue.TaskManagersChanged += Queue_TaskManagersChanged;
        }

        private Dictionary<int, StatusDto> taskID2Status = new Dictionary<int, StatusDto>();

        private void Queue_TaskManagersChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                var manager = e.NewItems[0] as FFmpegManager;
                var status = new StatusDto(manager.Task);
                taskID2Status.Add(manager.Task.Id, status);
                var task = Tasks.FirstOrDefault(p => p.Id == manager.Task.Id);
                Debug.Assert(task != null);
                UpdateTask(task);
                if (manager == Queue.MainQueueManager)
                {
                    Statuses.Insert(0, status);
                }
                else
                {
                    Statuses.Add(status);
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
                task.ProcessStatus = null;
                UpdateTask(task);

                Statuses.Remove(status);
                manager.StatusChanged -= Manager_StatusChanged;
            }
        }

        private void Manager_StatusChanged(object sender, System.EventArgs e)
        {
            var manager = sender as FFmpegManager;
            var status = taskID2Status.GetValueOrDefault(manager.Task.Id);
            Debug.Assert(status != null);
            var newStatus = manager.GetStatus();

            var task = Tasks.FirstOrDefault(p => p.Id == newStatus.Task.Id);
            Debug.Assert(task != null);
            task.ProcessStatus = newStatus;

            newStatus.Adapt(status);
        }

        public void Refresh()
        {
            var tasks = TaskManager.GetTasks();
            Tasks = new ObservableCollection<TaskInfoWithUI>(tasks.List.Adapt<List<TaskInfoWithUI>>());
        }

        public ObservableCollection<TaskInfoWithUI> Tasks
        {
            get => tasks;
            set => this.SetValueAndNotify(ref tasks, value, nameof(Tasks));
        }

        public ObservableCollection<StatusDto> Statuses { get; } = new ObservableCollection<StatusDto>();

        public QueueManager Queue { get; }

        public event PropertyChangedEventHandler PropertyChanged;

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

        private TaskInfoWithUI selectedTask;

        public TaskInfoWithUI SelectedTask
        {
            get => selectedTask;
            set => this.SetValueAndNotify(ref selectedTask, value, nameof(SelectedTask));
        }
    }
}