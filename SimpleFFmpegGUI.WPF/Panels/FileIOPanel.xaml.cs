using FzLib;
using Mapster;
using Microsoft.DotNet.PlatformAbstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAPICodePack.FzExtension;
using SimpleFFmpegGUI.Manager;
using SimpleFFmpegGUI.Model;
using SimpleFFmpegGUI.WPF.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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
    public class FileIOPanelViewModel : INotifyPropertyChanged
    {
        public FileIOPanelViewModel()
        {
            for (int i = 0; i < MinInputsCount; i++)
            {
                Inputs.Add(new InputArgumentsDetail() { Index = i + 1, });
            }
            Inputs.CollectionChanged += Inputs_CollectionChanged;
        }

        private int maxInputsCount = int.MaxValue;

        public int MaxInputsCount
        {
            get => maxInputsCount;
            set => this.SetValueAndNotify(ref maxInputsCount, value, nameof(MaxInputsCount));
        }

        private int minInputsCount = 1;

        public int MinInputsCount
        {
            get => minInputsCount;
            set
            {
                this.SetValueAndNotify(ref minInputsCount, value, nameof(MinInputsCount));
                while (value > Inputs.Count)
                {
                    Inputs.Add(new InputArgumentsDetail());
                }
                while (value < Inputs.Count)
                {
                    Inputs.RemoveAt(Inputs.Count - 1);
                }
            }
        }

        private bool showTimeClip;

        public bool ShowTimeClip
        {
            get => showTimeClip;
            set => this.SetValueAndNotify(ref showTimeClip, value, nameof(ShowTimeClip));
        }

        private void Inputs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            for (int i = 0; i < Inputs.Count; i++)
            {
                Inputs[i].Index = i + 1;
                Inputs[i].CanDelete = Inputs.Count > MinInputsCount;
            }
        }

        public ObservableCollection<InputArgumentsDetail> Inputs { get; } = new ObservableCollection<InputArgumentsDetail>();
        private string output;

        public string Output
        {
            get => output;
            set => this.SetValueAndNotify(ref output, value, nameof(Output));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private bool canChangeInputsCount;

        public bool CanChangeInputsCount
        {
            get => canChangeInputsCount;
            set => this.SetValueAndNotify(ref canChangeInputsCount, value, nameof(CanChangeInputsCount));
        }

        public void Reset()
        {
            Inputs.Clear();
            while (Inputs.Count < MinInputsCount)
            {
                Inputs.Add(new InputArgumentsDetail());
            }
        }

        public void Update(TaskType type)
        {
            CanChangeInputsCount = type is TaskType.Code or TaskType.Concat;
            MinInputsCount = type switch
            {
                TaskType.Code => 1,
                TaskType.Combine or TaskType.Concat or TaskType.Compare => 2,
                _ => 0
            };
            MaxInputsCount = type switch
            {
                TaskType.Code or TaskType.Concat => int.MaxValue,
                TaskType.Combine or TaskType.Compare => 2,
                _ => 0
            };
            ShowTimeClip = type switch
            {
                TaskType.Code => true,
                _ => false
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="inputs"></param>
        /// <param name="output"></param>
        /// <returns>若所有文件都被接受，返回True；若文件数量超过允许范围，返回False</returns>
        public bool Update(TaskType type, List<InputArguments> inputs, string output)
        {
            Update(type);
            Inputs.Clear();

            foreach (var input in inputs.Take(MaxInputsCount))
            {
                var newInput = input.Adapt<InputArgumentsDetail>();
                newInput.Update();
                Inputs.Add(newInput);
            }
            Output = output;
            return inputs.Count <= MaxInputsCount;
        }
    }

    public partial class FileIOPanel : UserControl
    {
        public FileIOPanel()
        {
            DataContext = ViewModel;
            InitializeComponent();
        }

        public FileIOPanelViewModel ViewModel { get; } = App.ServiceProvider.GetService<FileIOPanelViewModel>();

        public List<InputArguments> GetInputs()
        {
            foreach (var input in ViewModel.Inputs)
            {
                input.Apply();
            }
            var inputs = ViewModel.Inputs.Where(p => !string.IsNullOrEmpty(p.FilePath));
            if (inputs.Count() < ViewModel.MinInputsCount)
            {
                throw new Exception("输入文件少于需要的文件数量");
            }
            return inputs.Cast<InputArguments>().ToList();
        }

        public string GetOutput()
        {
            return ViewModel.Output;
        }

        public void Reset()
        {
            ViewModel.Reset();
        }

        private async void ClipButton_Click(object sender, RoutedEventArgs e)
        {
            var input = (sender as FrameworkElement).DataContext as InputArgumentsDetail;
            Debug.Assert(input != null);
            if (string.IsNullOrEmpty(input.FilePath))
            {
                this.CreateMessage().QueueError("请先设置文件地址");
                return;
            }
            if (!File.Exists(input.FilePath))
            {
                this.CreateMessage().QueueError($"找不到文件{input.FilePath}");
                return;
            }
            (sender as Button).IsEnabled = false;
            var win = App.ServiceProvider.GetService<ClipWindow>();
            win.Owner = Window.GetWindow(this);
            try
            {
                await win.SetVideoAsync(input.FilePath, input.From, input.To);
            }
            catch (Exception ex)
            {
                this.CreateMessage().QueueError($"加载视频失败", ex);
                (sender as Button).IsEnabled = true;
                return;
            }
            if (win.ShowDialog() == true)
            {
                var clip = win.GetClipTime();
                input.From = clip.From;
                input.To = clip.To;
                input.Duration = null;
            }
            (sender as Button).IsEnabled = true;
        }

        private void BrowseFileButton_Click(object sender, RoutedEventArgs e)
        {
            var input = (sender as FrameworkElement).DataContext as InputArgumentsDetail;
            string path = new FileFilterCollection().AddAll().CreateOpenFileDialog().SetParent(Window.GetWindow(this)).GetFilePath();
            if (path != null)
            {
                input.FilePath = path;
            }
        }

        private void DeleteFileButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Inputs.Remove((sender as FrameworkElement).DataContext as InputArgumentsDetail);
        }

        public void Update(TaskType type, List<InputArguments> inputs, string output)
        {
            if (!ViewModel.Update(type, inputs, output))
            {
                Window.GetWindow(this).CreateMessage().QueueError("输入文件超过该类型最大数量");
            }
        }

        public void Update(TaskType type)
        {
            ViewModel.Update(type);
            Reset();
        }

        public void AddInput()
        {
            ViewModel.Inputs.Add(new InputArgumentsDetail());
        }

        public void BrowseAndAddInput()
        {
            string path = new FileFilterCollection().AddAll().CreateOpenFileDialog().SetParent(Window.GetWindow(this)).GetFilePath();
            if (path != null)
            {
                var input = new InputArgumentsDetail();
                input.FilePath = path;
                ViewModel.Inputs.Add(input);
            }
        }

        private void BrowseOutputFileButton_Click(object sender, RoutedEventArgs e)
        {
            string path = new FileFilterCollection().AddAll().CreateSaveFileDialog().SetParent(Window.GetWindow(this)).GetFilePath();
            if (path != null)
            {
                path = path.RemoveEnd(".*");
                ViewModel.Output = path;
            }
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            base.OnDragOver(e);
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Link;
            }
        }

        protected override void OnDrop(DragEventArgs e)
        {
            base.OnDrop(e);
            var files = e.Data.GetData(DataFormats.FileDrop) as string[];

            foreach (var file in ViewModel.Inputs.Where(p => string.IsNullOrEmpty(p.FilePath)).ToList())
            {
                ViewModel.Inputs.Remove(file);
            }
            foreach (string file in files)
            {
                if (ViewModel.Inputs.Count >= ViewModel.MinInputsCount && !ViewModel.CanChangeInputsCount)
                {
                    break;
                }
                if (File.Exists(file))
                {
                    ViewModel.Inputs.Add(new InputArgumentsDetail() { FilePath = file });
                }
            }
            while (ViewModel.Inputs.Count < ViewModel.MinInputsCount)
            {
                ViewModel.Inputs.Add(new InputArgumentsDetail());
            }
        }
    }
}