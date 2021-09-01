using Enterwell.Clients.Wpf.Notifications;
using FzLib;
using Microsoft.Extensions.DependencyInjection;
using SimpleFFmpegGUI.Manager;
using SimpleFFmpegGUI.Model;
using System;
using System.Collections;
using System.Collections.Generic;
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

namespace SimpleFFmpegGUI.WPF
{
    public class AddTaskWindowViewModel : INotifyPropertyChanged
    {
        public AddTaskWindowViewModel(QueueManager queue)
        {
            Queue = queue;
        }

        public IEnumerable TaskTypes => Enum.GetValues(typeof(TaskType));
        private TaskType type;

        public event PropertyChangedEventHandler PropertyChanged;

        public TaskType Type
        {
            get => type;
            set => this.SetValueAndNotify(ref type, value, nameof(Type));
        }

        public QueueManager Queue { get; }
    }

    /// <summary>
    /// Interaction logic for AddTaskWindow.xaml
    /// </summary>
    public partial class AddTaskWindow : Window
    {
        public AddTaskWindowViewModel ViewModel { get; set; }

        public AddTaskWindow(AddTaskWindowViewModel viewModel)
        {
            ViewModel = viewModel;
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            DataContext = ViewModel;
            InitializeComponent();
            ViewModel.Type = TaskType.Code;
            presetsPanel.CodeArgumentsViewModel = argumentsPanel.ViewModel;
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.Type):
                    fileIOPanel.Update(ViewModel.Type);
                    argumentsPanel.Update(ViewModel.Type);
                    presetsPanel.Update(ViewModel.Type);
                    break;

                default:
                    break;
            }
        }

        private void AddToQueueAndStartButton_Click(object sender, RoutedEventArgs e)
        {
            AddToQueue(true);
        }

        private void AddToQueueButton_Click(object sender, RoutedEventArgs e)
        {
            AddToQueue(false);
        }

        private void AddToQueue(bool start)
        {
            TaskManager.AddTask(ViewModel.Type, fileIOPanel.GetInputs(), fileIOPanel.GetOutput(), argumentsPanel.GetOutputArguments());
            if (start)
            {
                ViewModel.Queue.StartQueue();
            }
        }
    }
}