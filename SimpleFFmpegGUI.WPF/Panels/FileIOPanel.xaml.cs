using FzLib;
using FzLib.WPF;
using FzLib.WPF.Converters;
using Mapster;
using Microsoft.DotNet.PlatformAbstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAPICodePack.FzExtension;
using ModernWpf.FzExtension.CommonDialog;
using SimpleFFmpegGUI.Manager;
using SimpleFFmpegGUI.Model;
using SimpleFFmpegGUI.WPF.Model;
using SimpleFFmpegGUI.WPF.Pages;
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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace SimpleFFmpegGUI.WPF.Panels
{
    public class FileIOPanelViewModel : INotifyPropertyChanged
    {
        private bool canChangeInputsCount;

        private int maxInputsCount = int.MaxValue;

        private int minInputsCount = 1;

        private string outputDir;

        private string outputFileName;

        private bool showTimeClip;

        private TaskType type;

        public FileIOPanelViewModel()
        {
            for (int i = 0; i < MinInputsCount; i++)
            {
                Inputs.Add(new InputArgumentsDetail() { Index = i + 1, });
            }
            Inputs.CollectionChanged += Inputs_CollectionChanged;
            Config.Instance.PropertyChanged += (s, e) => this.Notify(nameof(OutputDirPlaceholder));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 是否可以修改输入文件数量
        /// </summary>
        public bool CanChangeInputsCount
        {
            get => canChangeInputsCount;
            set => this.SetValueAndNotify(ref canChangeInputsCount, value, nameof(CanChangeInputsCount));
        }

        /// <summary>
        /// 是否可以设置输出文件名
        /// </summary>
        public bool CanSetOutputFileName => !(Type == TaskType.Code && Inputs.Count > 1);

        public ObservableCollection<InputArgumentsDetail> Inputs { get; } = new ObservableCollection<InputArgumentsDetail>();

        /// <summary>
        /// 最多输入文件的个数
        /// </summary>
        public int MaxInputsCount
        {
            get => maxInputsCount;
            set => this.SetValueAndNotify(ref maxInputsCount, value, nameof(MaxInputsCount));
        }

        /// <summary>
        /// 最少输入文件的个数
        /// </summary>
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

        /// <summary>
        /// 输出目录
        /// </summary>
        public string OutputDir
        {
            get => outputDir;
            set => this.SetValueAndNotify(ref outputDir, value, nameof(OutputDir));
        }

        /// <summary>
        /// 输出目录的提示
        /// </summary>
        public string OutputDirPlaceholder => "若为空，则保存到" +
            Config.Instance.DefaultOutputDirType switch
            {
                DefaultOutputDirType.InputDir => DescriptionConverter.GetDescription(DefaultOutputDirType.InputDir),
                DefaultOutputDirType.InputNewDir => $"输入文件同级的{Config.Instance.DefaultOutputDirInputSubDirName}目录",
                DefaultOutputDirType.SpecialDir => Config.Instance.DefaultOutputDirSpecialDirPath,
                _ => throw new NotImplementedException()
            };

        /// <summary>
        /// 输出文件名
        /// </summary>
        public string OutputFileName
        {
            get => outputFileName;
            set => this.SetValueAndNotify(ref outputFileName, value, nameof(OutputFileName));
        }

        /// <summary>
        /// 是否可用视频分割
        /// </summary>
        public bool ShowTimeClip
        {
            get => showTimeClip;
            set => this.SetValueAndNotify(ref showTimeClip, value, nameof(ShowTimeClip));
        }

        /// <summary>
        /// 任务类型
        /// </summary>
        public TaskType Type
        {
            get => type;
            set => this.SetValueAndNotify(ref type, value, nameof(Type), nameof(CanSetOutputFileName));
        }

        /// <summary>
        /// 重置
        /// </summary>
        public void Reset()
        {
            Inputs.Clear();
            while (Inputs.Count < MinInputsCount)
            {
                Inputs.Add(new InputArgumentsDetail());
            }
        }

        /// <summary>
        /// 更新任务类型
        /// </summary>
        /// <param name="type"></param>
        public void Update(TaskType type)
        {
            Type = type;
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
            while (Inputs.Count < MinInputsCount)
            {
                Inputs.Add(new InputArgumentsDetail());
            }
            OutputDir = Path.GetDirectoryName(output);
            OutputFileName = Path.GetFileName(output);
            return inputs.Count <= MaxInputsCount;
        }

        private void Inputs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            for (int i = 0; i < Inputs.Count; i++)
            {
                Inputs[i].Index = i + 1;
                Inputs[i].CanDelete = Inputs.Count > MinInputsCount;
            }
            this.Notify(nameof(CanSetOutputFileName));
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

        public void AddInput()
        {
            if (ViewModel.Inputs.Count >= ViewModel.MaxInputsCount)
            {
                throw new NotSupportedException("无法继续增加输入文件");
            }
            ViewModel.Inputs.Add(new InputArgumentsDetail());
        }

        public void BrowseAndAddInput()
        {
            string path = new FileFilterCollection().AddAll().CreateOpenFileDialog().SetParent(this.GetWindow()).GetFilePath();
            if (path != null)
            {
                var input = new InputArgumentsDetail();
                input.FilePath = path;
                ViewModel.Inputs.Add(input);
            }
        }

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

        public string GetOutput(InputArguments inputArgs)
        {
            var input = inputArgs.FilePath;
            string dir = ViewModel.OutputDir;
            if (string.IsNullOrWhiteSpace(dir))//没有指定输出位置
            {
                dir = Config.Instance.DefaultOutputDirType switch
                {
                    DefaultOutputDirType.InputDir => Path.GetDirectoryName(input),
                    DefaultOutputDirType.InputNewDir => Path.Combine(Path.GetDirectoryName(input), Config.Instance.DefaultOutputDirInputSubDirName),
                    DefaultOutputDirType.SpecialDir => Config.Instance.DefaultOutputDirSpecialDirPath,
                    _ => throw new NotImplementedException()
                };
            }
            if (ViewModel.CanSetOutputFileName && !string.IsNullOrWhiteSpace(ViewModel.OutputFileName))
            {
                return Path.Combine(dir, ViewModel.OutputFileName);
            }
            return Path.Combine(dir, Path.GetFileName(input));
        }

        /// <summary>
        /// 用于添加到远程主机，获取输出文件名
        /// </summary>
        /// <returns></returns>
        public string GetOutputFileName()
        {
            if (ViewModel.CanSetOutputFileName)//需要可以设置输出文件名
            {
                if (!string.IsNullOrWhiteSpace(ViewModel.OutputFileName))//如果手动指定
                {
                    return ViewModel.OutputFileName;
                }
                if (ViewModel.Inputs.Where(p => !string.IsNullOrEmpty(p.FilePath)).Any())//如果未手动指定并且存在输入文件
                {
                    return Path.GetFileName(ViewModel.Inputs.Where(p => !string.IsNullOrEmpty(p.FilePath)).First().FilePath);
                }
            }
            return null;
        }

        public void Reset()
        {
            ViewModel.Reset();
        }

        public void Update(TaskType type, List<InputArguments> inputs, string output)
        {
            if (!ViewModel.Update(type, inputs, output))
            {
                this.GetWindow().CreateMessage().QueueError("输入文件超过该类型最大数量");
            }
        }

        public void Update(TaskType type)
        {
            ViewModel.Update(type);
            Reset();
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

        private async void BrowseFileButton_Click(object sender, RoutedEventArgs e)
        {
            var input = (sender as FrameworkElement).DataContext as InputArgumentsDetail;
            string path = new FileFilterCollection()
                .AddAll()
                .CreateOpenFileDialog()
                .SetParent(this.GetWindow())
                .GetFilePath();
            if (path != null)
            {
                if (input.Image2)
                {
                    string seqFilename = FileSystemUtility.GetSequence(path);
                    if (seqFilename != null)
                    {
                        bool rename =await CommonDialog.ShowYesNoDialogAsync("图像序列", $"指定的文件可能是图像序列中的一个，是否将输入路径修改为{seqFilename}？");
                        if (rename)
                        {
                            path = seqFilename;
                        }
                    }
                }


                input.FilePath = path;
            }
        }

        private void BrowseOutputFileButton_Click(object sender, RoutedEventArgs e)
        {
            string path = new FileFilterCollection().CreateOpenFileDialog().SetParent(this.GetWindow()).GetFolderPath();
            if (path != null)
            {
                ViewModel.OutputDir = path;
            }
        }

        private async void ClipButton_Click(object sender, RoutedEventArgs e)
        {
            try
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
                this.GetWindow().IsEnabled = false;
                (TimeSpan From, TimeSpan To)? result = null;
                Process p = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = FzLib.Program.App.ProgramFilePath,
                        RedirectStandardOutput = true,
                    }
                };
                p.StartInfo.ArgumentList.Add("cut");
                p.StartInfo.ArgumentList.Add(new WindowInteropHelper(this.GetWindow()).Handle.ToString());
                p.StartInfo.ArgumentList.Add(input.FilePath);
                p.StartInfo.ArgumentList.Add(input.From.HasValue ? input.From.Value.ToString() : "-");
                p.StartInfo.ArgumentList.Add(input.To.HasValue ? input.To.Value.ToString() : "-");
                p.Start();
                var output = await p.StandardOutput.ReadToEndAsync();
                string[] outputs = output.Split(',');
                if (outputs.Length == 2)
                {
                    if (TimeSpan.TryParse(outputs[0], out TimeSpan from))
                    {
                        if (TimeSpan.TryParse(outputs[1], out TimeSpan to))
                        {
                            result = (from, to);
                        }
                    }
                }
                if (result.HasValue)
                {
                    var time = result.Value;
                    input.From = time.From;
                    input.To = time.To;
                    input.Duration = null;
                }
            }
            catch (Exception ex)
            {
                await CommonDialog.ShowErrorDialogAsync(ex);
            }
            finally
            {
                this.GetWindow().IsEnabled = true;
            }
        }

        private void DeleteFileButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Inputs.Remove((sender as FrameworkElement).DataContext as InputArgumentsDetail);
        }
    }
}