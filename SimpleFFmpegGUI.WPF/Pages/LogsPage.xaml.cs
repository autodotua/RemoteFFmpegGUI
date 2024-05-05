using Enterwell.Clients.Wpf.Notifications;
using FzLib;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using SimpleFFmpegGUI.Manager;
using SimpleFFmpegGUI.Model;
using SimpleFFmpegGUI.WPF.Model;
using System;
using System.Collections;
using System.Collections.Generic;
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

namespace SimpleFFmpegGUI.WPF.Pages
{
    public class LogsPageViewModel : INotifyPropertyChanged
    {
        public LogsPageViewModel(TaskManager taskManager, LogManager logManager)
        {
            taskManager.GetTasksAsync().ContinueWith(data =>
              {
                  Tasks = data.Result.List.Adapt<List<UITaskInfo>>();
              });
            this.taskManager = taskManager;
            this.logManager = logManager;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private IList<Log> logs;

        public IList<Log> Logs
        {
            get => logs;
            set => this.SetValueAndNotify(ref logs, value, nameof(Logs));
        }

        private DateTime from = DateTime.Now.AddDays(-1);

        public DateTime From
        {
            get => from;
            set => this.SetValueAndNotify(ref from, value, nameof(From));
        }

        private DateTime to = DateTime.Today.AddDays(1);

        public DateTime To
        {
            get => to;
            set => this.SetValueAndNotify(ref to, value, nameof(To));
        }

        private List<UITaskInfo> tasks;

        public List<UITaskInfo> Tasks
        {
            get => tasks;
            set => this.SetValueAndNotify(ref tasks, value, nameof(Tasks));
        }

        private UITaskInfo selectedTask;

        public UITaskInfo SelectedTask
        {
            get => selectedTask;
            set => this.SetValueAndNotify(ref selectedTask, value, nameof(SelectedTask));
        }

        public async Task FillLogsAsync()
        {
            Logs = (await logManager.GetLogsAsync(type: Type, taskId: SelectedTask?.Id ?? 0, from: From, to: To)).List;
        }

        private int typeIndex;
        private readonly TaskManager taskManager;
        private readonly LogManager logManager;

        public int TypeIndex
        {
            get => typeIndex;
            set => this.SetValueAndNotify(ref typeIndex, value, nameof(TypeIndex));
        }

        public char? Type => typeIndex switch
        {
            0 => null,
            1 => 'E',
            2 => 'W',
            3 => 'I',
            4 => 'O'
        };
    }

    /// <summary>
    /// Interaction logic for LogsPage.xaml
    /// </summary>
    public partial class LogsPage : UserControl
    {
        public LogsPageViewModel ViewModel { get; set; }

        public LogsPage(LogsPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = ViewModel;
            InitializeComponent();
        }

        private async void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.FillLogsAsync();
        }

        public async void FillLogs(int taskID)
        {
            var task = ViewModel.Tasks.FirstOrDefault(p => p.Id == taskID);
            Debug.Assert(task != null);
            ViewModel.SelectedTask = task;
            ViewModel.From = DateTime.MinValue;
            await ViewModel.FillLogsAsync();
        }
    }
}