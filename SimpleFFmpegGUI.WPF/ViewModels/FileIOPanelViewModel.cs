using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FzLib;
using FzLib.WPF.Converters;
using Mapster;
using Microsoft.Win32;
using ModernWpf.FzExtension.CommonDialog;
using SimpleFFmpegGUI.Model;
using SimpleFFmpegGUI.WPF.Messages;
using SimpleFFmpegGUI.WPF.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using CommonDialog = ModernWpf.FzExtension.CommonDialog.CommonDialog;
using Path = System.IO.Path;

namespace SimpleFFmpegGUI.WPF.ViewModels
{
    public partial class FileIOPanelViewModel : ViewModelBase
    {
        /// <summary>
        /// 是否可以修改输入文件数量
        /// </summary>
        [ObservableProperty]
        private bool canChangeInputsCount;

        /// <summary>
        /// 最多输入文件的个数
        /// </summary>
        [ObservableProperty]
        private int maxInputsCount = int.MaxValue;

        private int minInputsCount = 1;

        /// <summary>
        /// 输出目录
        /// </summary>
        [ObservableProperty]
        private string outputDir;

        /// <summary>
        /// 输出文件名
        /// </summary>
        [ObservableProperty]
        private string outputFileName;

        /// <summary>
        /// 是否可用视频分割
        /// </summary>     
        [ObservableProperty]
        private bool showTimeClip;

        /// <summary>
        /// 任务类型
        /// </summary>
        [NotifyPropertyChangedFor(nameof(CanSetOutputFileName))]
        [ObservableProperty]
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

        /// <summary>
        /// 是否可以设置输出文件名
        /// </summary>
        public bool CanSetOutputFileName => !(Type == TaskType.Code && Inputs.Count > 1);

        public ObservableCollection<InputArgumentsDetail> Inputs { get; } = new ObservableCollection<InputArgumentsDetail>();

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
            }
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
        /// 重置
        /// </summary>
        public void Reset(bool keepFiles)
        {
            var files = keepFiles ?
                Inputs.Select(p => p.FilePath).ToList() :
                null;
            Inputs.Clear();
            int count = keepFiles ?
                Math.Max(MinInputsCount, Math.Min(files.Count, MaxInputsCount))
                : MinInputsCount;
            while (Inputs.Count < count)
            {
                Inputs.Add(new InputArgumentsDetail());
            }
            if (keepFiles)
            {
                for (int i = 0; i < Math.Min(files.Count, Inputs.Count); i++)
                {
                    Inputs[i].FilePath = files[i];
                }
            }
        }

        /// <summary>
        /// 更新任务类型
        /// </summary>
        /// <param name="type"></param>
        public void UpdateType(TaskType type)
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
        public void Update(TaskType type, List<InputArguments> inputs, string output)
        {
            UpdateType(type);
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
            if( inputs.Count > MaxInputsCount)
            {
                QueueErrorMessage("输入文件超过该类型最大数量");
            }
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


        [RelayCommand]
        private async Task BrowseFile(InputArgumentsDetail input)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog().AddAllFilesFilter();
            SendMessage(new FileDialogMessage(dialog));
            string path = dialog.FileName;
            if (path != null)
            {
                if (input.Image2)
                {
                    string seqFilename = FileSystemUtility.GetSequence(path);
                    if (seqFilename != null)
                    {
                        bool rename = await CommonDialog.ShowYesNoDialogAsync("图像序列", $"指定的文件可能是图像序列中的一个，是否将输入路径修改为{seqFilename}？");
                        if (rename)
                        {
                            path = seqFilename;
                        }
                    }
                }
                input.FilePath = path;
            }
        }

        [RelayCommand]
        private void RemoveFile(InputArgumentsDetail input)
        {
            Inputs.Remove(input);
        }

        [RelayCommand]
        private void BrowseOutputFile()
        {
            var dialog = new OpenFolderDialog();
            SendMessage(new FileDialogMessage(dialog));
            string path = dialog.FolderName;
            if (path != null)
            {
                OutputDir = path;
            }
        }

        [RelayCommand]
        private async Task ClipAsync(InputArgumentsDetail input)
        {
            try
            {
                Debug.Assert(input != null);
                if (string.IsNullOrEmpty(input.FilePath))
                {
                 QueueErrorMessage("请先设置文件地址");
                    return;
                }
                if (!File.Exists(input.FilePath))
                {
                   QueueErrorMessage($"找不到文件{input.FilePath}");
                    return;
                }
                SendMessage(new WindowEnableMessage(false));
                (TimeSpan From, TimeSpan To)? result = null;

                var handle = WeakReferenceMessenger.Default.Send(new WindowHandleMessage()).Handle;

                Process p = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = FzLib.Program.App.ProgramFilePath,
                        RedirectStandardOutput = true,
                    }
                };
                p.StartInfo.ArgumentList.Add("cut");
                p.StartInfo.ArgumentList.Add(handle.ToString());
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
                SendMessage(new WindowEnableMessage(true));
            }
        }
    }
}