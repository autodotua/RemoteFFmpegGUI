﻿using FzLib;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace SimpleFFmpegGUI.WPF.Model
{
    public abstract class TaskCollectionBase : INotifyPropertyChanged
    {
        public abstract void Refresh();

        private ObservableCollection<UITaskInfo> tasks;

        public ObservableCollection<UITaskInfo> Tasks
        {
            get => tasks;
            set => this.SetValueAndNotify(ref tasks, value, nameof(Tasks));
        }

        private UITaskInfo selectedTask;

        public event PropertyChangedEventHandler PropertyChanged;

        public UITaskInfo SelectedTask
        {
            get => selectedTask;
            set => this.SetValueAndNotify(ref selectedTask, value, nameof(SelectedTask));
        }

        public IList<UITaskInfo> SelectedTasks=>Tasks.Where(p=>p.IsSelected).ToList();
    }
}