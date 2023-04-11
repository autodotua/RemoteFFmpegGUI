using FzLib;
using FzLib.WPF.Converters;
using Mapster;
using SimpleFFmpegGUI.Dto;
using SimpleFFmpegGUI.Manager;
using SimpleFFmpegGUI.Model;
using SimpleFFmpegGUI.WPF.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using TaskStatus = SimpleFFmpegGUI.Model.TaskStatus;

namespace SimpleFFmpegGUI.WPF.Model
{
    public class UITaskInfo : ModelBase, INotifyPropertyChanged
    {
        public UITaskInfo()
        {
            StartTimer();
        }
        PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
        private async void StartTimer()
        {
            while (await timer.WaitForNextTickAsync())
            {
                Stopwatch sw = Stopwatch.StartNew();
                await UpdateSnapshotAsync();
                sw.Stop();
                Debug.WriteLine(sw.ElapsedMilliseconds);
            }
        }
        private OutputArguments arguments;

        private DateTime createTime;

        private string ffmpegArguments;

        private DateTime? finishTime;

        private List<InputArguments> inputs;

        private string message;

        private string output;

        private FFmpegManager processManager;
        private int processPriority = Config.Instance.DefaultProcessPriority;
        private StatusDto processStatus;

        private string realOutput;

        private DateTime? startTime;

        private TaskStatus status;

        private TaskType type;

        public event PropertyChangedEventHandler PropertyChanged;

        public OutputArguments Arguments
        {
            get => arguments;
            set => this.SetValueAndNotify(ref arguments, value, nameof(Arguments));
        }

        public bool CancelButtonEnabled => Status is TaskStatus.Queue or TaskStatus.Processing;

        public Brush Color => Status switch
        {
            TaskStatus.Queue => System.Windows.Application.Current.FindResource("SystemControlForegroundBaseHighBrush") as Brush,
            TaskStatus.Processing => Brushes.Orange,
            TaskStatus.Done => Brushes.Green,
            TaskStatus.Error => Brushes.Red,
            TaskStatus.Cancel => Brushes.Gray,
            _ => throw new InvalidEnumArgumentException(),
        };

        public DateTime CreateTime
        {
            get => createTime;
            set => this.SetValueAndNotify(ref createTime, value, nameof(CreateTime));
        }

        public string FFmpegArguments
        {
            get => ffmpegArguments;
            set => this.SetValueAndNotify(ref ffmpegArguments, value, nameof(FFmpegArguments));
        }

        public DateTime? FinishTime
        {
            get => finishTime;
            set => this.SetValueAndNotify(ref finishTime, value, nameof(FinishTime));
        }

        public string InputDetailText
        {
            get
            {
                if (inputs.Count != 1)
                {
                    return string.Join(Environment.NewLine, inputs.Select(p => Path.GetFileName(p.FilePath)));
                }
                string name = Path.GetFileName(inputs[0].FilePath);
                return name
                    + (inputs[0].From.HasValue ? $" 开始：{inputs[0].From.Value:hh\\:mm\\:ss\\.fff}" : "")
                    + (inputs[0].To.HasValue ? $" 结束：{inputs[0].To.Value:hh\\:mm\\:ss\\.fff}" : "")
                    + (inputs[0].Duration.HasValue ? $" 经过：{inputs[0].Duration.Value:hh\\:mm\\:ss\\.fff}" : "");
            }
        }

        public string InputDetailToolTipText
        {
            get
            {
                if (inputs.Count != 1)
                {
                    return string.Join(Environment.NewLine, inputs.Select(p => p.FilePath));
                }
                return InputDetailText;
            }
        }

        public List<InputArguments> Inputs
        {
            get => inputs;
            set => this.SetValueAndNotify(ref inputs, value, nameof(Inputs), nameof(InputText), nameof(InputsText), nameof(IOText));
        }

        public string InputsText
        {
            get
            {
                if (inputs.Count == 0)
                {
                    return "未指定输入";
                }
                return string.Join("\r\n", inputs.Select(p => Path.GetFileName(p.FilePath)));
            }
        }

        public string InputText
        {
            get
            {
                if (inputs.Count == 0)
                {
                    return "未指定输入";
                }
                string path = Path.GetFileName(inputs[0].FilePath);
                return inputs.Count == 1 ? path : path + "等";
            }
        }

        public string IOText => $"{InputText} → {OutputText}";

        public bool IsIndeterminate => ProcessStatus == null || ProcessStatus.HasDetail == false || ProcessStatus.Progress.IsIndeterminate;

        public string Message
        {
            get => message;
            set => this.SetValueAndNotify(ref message, value, nameof(Message));
        }

        public string Output
        {
            get => output;
            set => this.SetValueAndNotify(ref output, value, nameof(Output), nameof(OutputText), nameof(IOText));
        }

        public string OutputText
        {
            get
            {
                string output = Output;
                if (!string.IsNullOrEmpty(RealOutput))
                {
                    output = Path.GetFileName(RealOutput);
                }
                else if (output == null)
                {
                    output = "未指定输出";
                }
                else
                {
                    output = Path.GetFileName(Output);
                }
                return output;
            }
        }

        public double Percent => ProcessStatus == null || ProcessStatus.HasDetail == false ? 0 : ProcessStatus.Progress.Percent;

        public FFmpegManager ProcessManager
        {
            get => processManager;
            set
            {
                if (processManager != null)
                {
                    processManager.ProcessChanged -= Manager_ProcessChanged;
                }
                processManager = value;
                if (value != null)
                {
                    value.ProcessChanged += Manager_ProcessChanged;
                }
            }
        }

        public int ProcessPriority
        {
            get
            {
                return processPriority;
                //     ProcessManager.Process.Priority switch
                //{
                //    ProcessPriorityClass.RealTime => 0,
                //    ProcessPriorityClass.High => 1,
                //    ProcessPriorityClass.AboveNormal => 2,
                //    ProcessPriorityClass.Normal => 3,
                //    ProcessPriorityClass.BelowNormal => 4,
                //    ProcessPriorityClass.Idle => 5,
                //    _ => throw new InvalidEnumArgumentException(nameof(ProcessPriorityClass)),
                //};
            }
            set
            {
                processPriority = value;
                if (ProcessManager.Process != null)
                {

                    ProcessManager.Process.Priority = value switch
                    {
                        5 => ProcessPriorityClass.RealTime,
                        4 => ProcessPriorityClass.High,
                        3 => ProcessPriorityClass.AboveNormal,
                        2 => ProcessPriorityClass.Normal,
                        1 => ProcessPriorityClass.BelowNormal,
                        0 => ProcessPriorityClass.Idle,
                        _ => ProcessPriorityClass.Normal,
                    };
                }
                this.Notify(nameof(Manager), nameof(ProcessPriority));
            }
        }

        public StatusDto ProcessStatus
        {
            get => processStatus;
            set => this.SetValueAndNotify(ref processStatus, value,
                nameof(ProcessStatus),
                nameof(Percent),
                nameof(Status),
                nameof(StatusText),
                nameof(IsIndeterminate));
        }

        public string RealOutput
        {
            get => realOutput;
            set => this.SetValueAndNotify(ref realOutput, value, nameof(RealOutput));
        }

        public bool ResetButtonEnabled => Status is TaskStatus.Done or TaskStatus.Cancel or TaskStatus.Error;

        public bool StartButtonEnabled => Status is TaskStatus.Queue;

        public DateTime? StartTime
        {
            get => startTime;
            set => this.SetValueAndNotify(ref startTime, value, nameof(StartTime));
        }

        public TaskStatus Status
        {
            get => status;
            set => this.SetValueAndNotify(ref status, value, nameof(Status),
                nameof(ResetButtonEnabled),
                nameof(StartButtonEnabled),
                nameof(CancelButtonEnabled),
                nameof(StatusText),
                nameof(Color),
                nameof(Percent));
        }

        public string StatusText => Status switch
        {
            TaskStatus.Processing => IsIndeterminate ? "进行中" : Percent.ToString("0.00%"),
            _ => DescriptionConverter.GetDescription(Status)
        };

        public string Title => Type == TaskType.Custom ?
                                                                                                                                                                                                                                                                                                                              AttributeHelper.GetAttributeValue<NameDescriptionAttribute, string>(Type, p => p.Name)
            : AttributeHelper.GetAttributeValue<NameDescriptionAttribute, string>(Type, p => p.Name) + "：" + InputText;

        public TaskType Type
        {
            get => type;
            set => this.SetValueAndNotify(ref type, value, nameof(Type));
        }

        public static UITaskInfo FromTask(TaskInfo task)
        {
            return task.Adapt<UITaskInfo>();
        }

        public TaskInfo GetTask()
        {
            return TaskManager.GetTask(Id);
        }

        public TaskInfo ToTask()
        {
            return this.Adapt<TaskInfo>();
        }

        public void UpdateSelf()
        {
            TaskManager.GetTask(Id).Adapt(this);
        }
        private Uri snapshotSource;
        private TimeSpan lastTime = default;
        private async Task UpdateSnapshotAsync()
        {
            if (Type != TaskType.Code
                || ProcessStatus == null
                || !ProcessStatus.HasDetail
                || processStatus.Progress == null)
            {
                SnapshotSource = null;
                return;
            }
            var time = processStatus.Time + (Inputs[0].From ?? TimeSpan.Zero);
            if (processStatus.IsPaused || lastTime == time)
            {
                return;
            }
            lastTime = time;
            string path = await MediaInfoManager.GetSnapshotAsync(Inputs[0].FilePath, time);
            SnapshotSource = new Uri(path);
        }

        public Uri SnapshotSource
        {
            get => snapshotSource;
            set => this.SetValueAndNotify(ref snapshotSource, value, nameof(SnapshotSource));
        }

        private void Manager_ProcessChanged(object sender, ProcessChangedEventArgs e)
        {
            //进程改变后（比如二压），重新应用
            if (e.NewProcess != null)
            {
                ProcessPriority = processPriority;
            }
        }
    }
}