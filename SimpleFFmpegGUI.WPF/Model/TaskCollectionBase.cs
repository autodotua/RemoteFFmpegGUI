using CommunityToolkit.Mvvm.ComponentModel;
using FzLib;
using SimpleFFmpegGUI.WPF.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleFFmpegGUI.WPF.Model
{
    public abstract partial class TaskCollectionBase : ViewModelBase
    {
        public abstract Task RefreshAsync();

        [ObservableProperty]
        private ObservableCollection<UITaskInfo> tasks;

        [ObservableProperty]
        private UITaskInfo selectedTask;

        public IList<UITaskInfo> SelectedTasks=>Tasks.Where(p=>p.IsSelected).ToList();
    }
}