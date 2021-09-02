using FzLib;
using FzLib.WPF.Converters;
using Microsoft.Extensions.DependencyInjection;
using SimpleFFmpegGUI.Manager;
using SimpleFFmpegGUI.Model;
using SimpleFFmpegGUI.WPF;
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TaskStatus = SimpleFFmpegGUI.Model.TaskStatus;

namespace SimpleFFmpegGUI.WPF.Panels
{
    public class TaskListViewModel : INotifyPropertyChanged
    {
        public TaskListViewModel(QueueManager queue)
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

    public class TaskDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TaskInfo task = value as TaskInfo;
            Debug.Assert(task != null);
            switch (parameter as string)
            {
                case "IO":
                    return $"{Convert(task, targetType, nameof(task.Inputs), culture)} → {Convert(task, targetType, nameof(task.Output), culture)}";

                case nameof(TaskInfo.Inputs):
                    var inputs = task.Inputs;
                    if (inputs.Count == 0)
                    {
                        return "未指定输入";
                    }
                    string path = System.IO.Path.GetFileName(inputs[0].FilePath);
                    return inputs.Count == 1 ? path : (path + "等");

                case nameof(TaskInfo.Output):
                    var output = task.Output;
                    if (output == null)
                    {
                        return "未指定输出";
                    }
                    return System.IO.Path.GetFileName(output);

                case nameof(TaskInfo.Status):
                    return task.Status switch
                    {
                        TaskStatus.Processing => throw new NotImplementedException(),
                        _ => Enum2DescriptionConverter.GetDescription(task.Status)
                    };

                case "Color":
                    return task.Status switch
                    {
                        TaskStatus.Queue => App.Current.FindResource("SystemControlForegroundBaseHighBrush") as Brush,
                        TaskStatus.Processing => Brushes.Orange,
                        TaskStatus.Done => Brushes.Green,
                        TaskStatus.Error => Brushes.Red,
                        TaskStatus.Cancel => Brushes.Gray,
                    };

                case "Percent":
                    throw new NotImplementedException();

                default:
                    throw new NotSupportedException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}