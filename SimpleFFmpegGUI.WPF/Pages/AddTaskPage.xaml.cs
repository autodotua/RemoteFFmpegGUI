using Enterwell.Clients.Wpf.Notifications;
using FFMpegCore.Exceptions;
using FzLib;
using FzLib.Collection;
using FzLib.WPF;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using ModernWpf.FzExtension.CommonDialog;
using Newtonsoft.Json;
using SimpleFFmpegGUI.FFmpegArgument;
using SimpleFFmpegGUI.Manager;
using SimpleFFmpegGUI.Model;
using SimpleFFmpegGUI.WPF.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
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
    public class AddTaskPageViewModel : INotifyPropertyChanged
    {
        public AddTaskPageViewModel(QueueManager queue)
        {
            Queue = queue;
        }

        public IEnumerable TaskTypes => Enum.GetValues(typeof(TaskType));
        private TaskType type;

        public event PropertyChangedEventHandler PropertyChanged;

        public TaskType Type
        {
            get => type;
            set
            {
                this.SetValueAndNotify(ref type, value, nameof(Type));
                CanAddFile = value is TaskType.Code or TaskType.Concat;
            }
        }

        public QueueManager Queue { get; }

        private bool allowChangeType = true;

        public bool AllowChangeType
        {
            get => allowChangeType;
            set => this.SetValueAndNotify(ref allowChangeType, value, nameof(AllowChangeType));
        }

        private bool canAddFile;

        public bool CanAddFile
        {
            get => canAddFile;
            set => this.SetValueAndNotify(ref canAddFile, value, nameof(CanAddFile));
        }
    }

    /// <summary>
    /// Interaction logic for AddTaskPage.xaml
    /// </summary>
    public partial class AddTaskPage : UserControl
    {
        public AddTaskPageViewModel ViewModel { get; set; }

        public AddTaskPage(AddTaskPageViewModel viewModel)
        {
            ViewModel = viewModel;
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            DataContext = ViewModel;
            InitializeComponent();
            presetsPanel.ViewModel.CodeArgumentsViewModel = argumentsPanel.ViewModel;
        }

        private bool canInitializeType = true;

        private async void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.Type):
                    fileIOPanel.Update(ViewModel.Type);
                    await argumentsPanel.UpdateTypeAsync(ViewModel.Type);
                    await presetsPanel.UpdateTypeAsync(ViewModel.Type);
                    break;

                default:
                    break;
            }
        }

        private async void AddToQueueButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ViewModel.Type is TaskType.Code)
                {
                    FFmpegManager.TestOutputArguments(argumentsPanel.GetOutputArguments());
                }
            }
            catch (FFmpegArgumentException ex)
            {
                await CommonDialog.ShowErrorDialogAsync(ex.Message, null, title: "参数错误");
                return;
            }

            IsEnabled = false;
            try
            {
                List<InputArguments> inputs = fileIOPanel.GetInputs();
                OutputArguments args = argumentsPanel.GetOutputArguments();
                var tm = App.ServiceProvider.GetRequiredService<TaskManager>();

                switch (ViewModel.Type)
                {
                    case TaskType.Code://需要将输入文件单独加入任务
                        foreach (var input in inputs)
                        {
                            TaskInfo task = null;
                            await tm.AddTaskAsync(TaskType.Code, new List<InputArguments>() { input }, fileIOPanel.GetOutput(input), args);
                            Dispatcher.Invoke(() => App.ServiceProvider.GetService<TasksAndStatuses>().Tasks.Insert(0, UITaskInfo.FromTask(task)));
                        }
                        this.CreateMessage().QueueSuccess($"已加入{inputs.Count}个任务队列");
                        break;
                    case TaskType.Custom or TaskType.Compare://不存在文件输出
                        {
                            TaskInfo task = null;
                            await tm.AddTaskAsync(ViewModel.Type, inputs, null, args);
                            App.ServiceProvider.GetService<TasksAndStatuses>().Tasks.Insert(0, UITaskInfo.FromTask(task));
                            this.CreateMessage().QueueSuccess("已加入队列");
                        }
                        break;
                    default:
                        {
                            TaskInfo task = null;
                            await tm.AddTaskAsync(ViewModel.Type, inputs, fileIOPanel.GetOutput(inputs[0]), args);
                            App.ServiceProvider.GetService<TasksAndStatuses>().Tasks.Insert(0, UITaskInfo.FromTask(task));
                            this.CreateMessage().QueueSuccess("已加入队列");
                        }
                        break;
                }
                if (Config.Instance.ClearFilesAfterAddTask)
                {
                    fileIOPanel.Reset();
                }
                if ("queue".Equals((sender as Button).Tag as string))
                {
                    await Task.Run(() => App.ServiceProvider.GetService<QueueManager>().StartQueue());
                    this.CreateMessage().QueueSuccess("已开始队列");
                }
                SaveAsLastOutputArguments(args);
            }
            catch (Exception ex)
            {
                this.CreateMessage().QueueError("加入队列失败", ex);
            }
            finally
            {
                IsEnabled = true;
            }
        }

        public void SetAsClone(TaskInfo task)
        {
            canInitializeType = false;
            //ViewModel.AllowChangeType = false;
            ViewModel.Type = task.Type;
            fileIOPanel.Update(task.Type, task.Inputs, task.Output);
            argumentsPanel.Update(task.Type, task.Arguments);
        }

        public void SetFiles(IEnumerable<string> files, TaskType type)
        {
            canInitializeType = false;
            ViewModel.Type = type;
            fileIOPanel.Update(type, files.Select(p => new InputArguments() { FilePath = p }).ToList(), null);
        }

        private async void SaveToPresetButton_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void BrowseAndAddInputButton_Click(object sender, RoutedEventArgs e)
        {
            fileIOPanel.BrowseAndAddInput();
        }

        private void AddInputButton_Click(object sender, RoutedEventArgs e)
        {
            fileIOPanel.AddInput();
        }

        private void CommandBar_MouseEnter(object sender, MouseEventArgs e)
        {
            (sender as FrameworkElement).Focus();
        }

        private async void AddToRemoteHostButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ViewModel.Type is TaskType.Code)
                {
                    FFmpegManager.TestOutputArguments(argumentsPanel.GetOutputArguments());
                }
            }
            catch (FFmpegArgumentException ex)
            {
                await CommonDialog.ShowErrorDialogAsync(ex.Message, null, title: "参数错误");
                return;
            }

            var items = Config.Instance.RemoteHosts.Select(p =>
            new SelectDialogItem(p.Name, p.Address));
            var index = await CommonDialog.ShowSelectItemDialogAsync("请确保在远程主机的输入文件夹中有同名文件", items);
            if (index < 0)
            {
                return;
            }
            IsEnabled = false;
            try
            {
                var host = Config.Instance.RemoteHosts[index];
                List<InputArguments> inputs = fileIOPanel.GetInputs().Adapt<List<InputArguments>>();
                foreach (var i in inputs)
                {
                    //绝对路径，仅保留文件名
                    if (i.FilePath.Contains(':'))
                    {
                        i.FilePath = System.IO.Path.GetFileName(i.FilePath);
                    }
                    i.FilePath = ":" + i.FilePath;
                }
                string output = fileIOPanel.GetOutputFileName();
                OutputArguments args = argumentsPanel.GetOutputArguments();
                var data = new
                {
                    Inputs = inputs,
                    Output = output,
                    Argument = args,
                    Start = "queue".Equals((sender as Button).Tag as string)
                };
                await PostAsync(host, "Task/Add/" + ViewModel.Type.ToString(), data);

                if (Config.Instance.ClearFilesAfterAddTask)
                {
                    fileIOPanel.Reset();
                }
                SaveAsLastOutputArguments(args);
                this.CreateMessage().QueueSuccess("已加入到远程主机" + host.Name);
            }
            catch (Exception ex)
            {
                await CommonDialog.ShowErrorDialogAsync(ex, "加入远程主机失败");
            }
            finally
            {
                IsEnabled = true;
            }
        }

        public static async Task PostAsync(RemoteHost host, string subUrl, object data)
        {
            HttpClient client = new HttpClient();
            string str = JsonConvert.SerializeObject(data);
            var content = new StringContent(str, Encoding.UTF8, "application/json");
            string url = host.Address.TrimEnd('/') + "/" + subUrl.TrimStart('/');
            if (!string.IsNullOrEmpty(host.Token))
            {
                client.DefaultRequestHeaders.Add("Authorization", host.Token);
            }
            var response = await client.PostAsync(url, content);

            var responseString = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                if (string.IsNullOrWhiteSpace(responseString))
                {
                    throw new HttpRequestException($"{response.StatusCode}");
                }
                else
                {
                    throw new HttpRequestException($"{response.StatusCode}：{responseString}");
                }
            }
        }

        private void SaveAsLastOutputArguments(OutputArguments arguments)
        {
            if (!Config.Instance.RememberLastArguments)
            {
                return;
            }
            Config.Instance.LastOutputArguments.AddOrSetValue(ViewModel.Type, arguments);
            Config.Instance.Save();
        }

        private void ClearFilesButton_Click(object sender, RoutedEventArgs e)
        {
            fileIOPanel.Reset();
        }

        private async void FFmpegArgsButton_Click(object sender, RoutedEventArgs e)
        {
            this.GetWindow().Activate();
            try
            {
                OutputArguments args = argumentsPanel.GetOutputArguments();
                await CommonDialog.ShowOkDialogAsync("输出参数", FFmpegManager.TestOutputArguments(args));
            }
            catch (Exception ex)
            {
                await CommonDialog.ShowErrorDialogAsync(ex, "获取参数失败");
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (canInitializeType)
            {
                ViewModel.Type = TaskType.Code;
                canInitializeType = false;
            }
        }
    }
}