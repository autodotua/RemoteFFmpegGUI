using FzLib;
using Microsoft.Extensions.DependencyInjection;
using SimpleFFmpegGUI.Manager;
using SimpleFFmpegGUI.Model;
using SimpleFFmpegGUI.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SimpleFFmpegGUI.WPF.Panels
{
    public class TaskListViewModel : INotifyPropertyChanged
    {
        public TaskListViewModel()
        {
            var tasks = TaskManager.GetTasks();
            Tasks = new ObservableCollection<TaskInfo>(tasks.List);
        }

        private ObservableCollection<TaskInfo> tasks;

        public ObservableCollection<TaskInfo> Tasks
        {
            get => tasks;
            set => this.SetValueAndNotify(ref tasks, value, nameof(Tasks));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public partial class TaskList : UserControl
    {
        public TaskList()
        {
            DataContext = ViewModel;
            InitializeComponent();
        }

        public TaskListViewModel ViewModel => App.ServiceProvider.GetService<TaskListViewModel>();
    }
}