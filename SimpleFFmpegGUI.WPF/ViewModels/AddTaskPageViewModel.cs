using CommunityToolkit.Mvvm.ComponentModel;
using FzLib;
using Microsoft.Extensions.DependencyInjection;
using SimpleFFmpegGUI.Manager;
using SimpleFFmpegGUI.Model;
using SimpleFFmpegGUI.WPF.Model;
using SimpleFFmpegGUI.WPF.Panels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;
using SimpleFFmpegGUI.FFmpegArgument;
using SimpleFFmpegGUI.WPF.Messages;
using FzLib.Collection;

namespace SimpleFFmpegGUI.WPF.ViewModels
{
    public partial class AddTaskPageViewModel : ViewModelBase
    {
        public AddTaskPageViewModel(QueueManager queue, TaskManager taskManager, TasksAndStatuses tasksAndStatuses)
        {
            Queue = queue;
            this.taskManager = taskManager;
            this.tasksAndStatuses = tasksAndStatuses;
        }

        public FileIOPanelViewModel FileIOViewModel { get; set; }
        public CodeArgumentsPanelViewModel CodeArgumentsViewModel { get; set; }
        public PresetsPanelViewModel PresetsViewModel { get; set; }

        public IEnumerable TaskTypes => Enum.GetValues(typeof(TaskType));

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanAddFile))]
        private TaskType type;

        public QueueManager Queue { get; }


        [ObservableProperty]
        private bool allowChangeType = true;
        private readonly TaskManager taskManager;
        private readonly TasksAndStatuses tasksAndStatuses;

        public bool CanAddFile => Type is TaskType.Code or TaskType.Concat;

        private async void AddToQueue(bool addToQueue)
        {
            var args = CodeArgumentsViewModel.GetArguments();
            try
            {
                if (Type is TaskType.Code)
                {
                    FFmpegManager.TestOutputArguments(args);
                }
            }
            catch (FFmpegArgumentException ex)
            {
                QueueErrorMessage("参数错误", ex);
                return;
            }

            SendMessage(new WindowEnableMessage(false));
            try
            {
                List<InputArguments> inputs = FileIOViewModel.GetInputs();

                switch (Type)
                {
                    case TaskType.Code://需要将输入文件单独加入任务
                        foreach (var input in inputs)
                        {
                            TaskInfo task = await taskManager.AddTaskAsync(TaskType.Code, new List<InputArguments>() { input }, FileIOViewModel.GetOutput(input), args);
                            App.ServiceProvider.GetService<TasksAndStatuses>().Tasks.Insert(0, UITaskInfo.FromTask(task));
                        }
                        QueueSuccessMessage($"已加入{inputs.Count}个任务队列");
                        break;
                    case TaskType.Custom or TaskType.Compare://不存在文件输出
                        {
                            TaskInfo task = await taskManager.AddTaskAsync(Type, inputs, null, args);
                            App.ServiceProvider.GetService<TasksAndStatuses>().Tasks.Insert(0, UITaskInfo.FromTask(task));
                            QueueSuccessMessage("已加入队列");
                        }
                        break;
                    default:
                        {
                            TaskInfo task = await taskManager.AddTaskAsync(Type, inputs, FileIOViewModel.GetOutput(inputs[0]), args);
                            App.ServiceProvider.GetService<TasksAndStatuses>().Tasks.Insert(0, UITaskInfo.FromTask(task));
                            QueueSuccessMessage("已加入队列");
                        }
                        break;
                }
                if (Config.Instance.ClearFilesAfterAddTask)
                {
                    FileIOViewModel.Reset(false);
                }
                if (addToQueue)
                {
                    await Task.Run(() => App.ServiceProvider.GetService<QueueManager>().StartQueue());
                    QueueSuccessMessage("已开始队列");
                }
                SaveAsLastOutputArguments(args);
            }
            catch (Exception ex)
            {
                QueueErrorMessage("加入队列失败", ex);
            }
            finally
            {
                SendMessage(new WindowEnableMessage(true));
            }
        }
        private void SaveAsLastOutputArguments(OutputArguments arguments)
        {
            if (!Config.Instance.RememberLastArguments)
            {
                return;
            }
            Config.Instance.LastOutputArguments.AddOrSetValue(Type, arguments);
            Config.Instance.Save();
        }
    }
}