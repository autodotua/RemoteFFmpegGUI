using FzLib;
using FzLib.WPF.Converters;
using Mapster;
using SimpleFFmpegGUI.Model;
using SimpleFFmpegGUI.WPF.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Path = System.IO.Path;

namespace SimpleFFmpegGUI.WPF.ViewModels
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
}