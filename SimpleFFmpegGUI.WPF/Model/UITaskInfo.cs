using FzLib;
using FzLib.WPF.Converters;
using Mapster;
using SimpleFFmpegGUI.Dto;
using SimpleFFmpegGUI.Manager;
using SimpleFFmpegGUI.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Media;
using TaskStatus = SimpleFFmpegGUI.Model.TaskStatus;

namespace SimpleFFmpegGUI.WPF.Model
{
    public class UITaskInfo : ModelBase, INotifyPropertyChanged
    {
        public string Title => DescriptionConverter.GetDescription(Type) + "：" + InputText;
        public string IOText => $"{InputText} → {OutputText}";

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

        public string InputDetailText
        {
            get
            {
                if (inputs.Count != 1)
                {
                    return InputText;
                }
                string name = Path.GetFileName(inputs[0].FilePath);
                return name
                    + (inputs[0].From.HasValue ? $" 开始：{inputs[0].From.Value:hh\\:mm\\:ss\\.fff}" : "")
                    + (inputs[0].To.HasValue ? $" 结束：{inputs[0].To.Value:hh\\:mm\\:ss\\.fff}" : "")
                    + (inputs[0].Duration.HasValue ? $" 经过：{inputs[0].Duration.Value:hh\\:mm\\:ss\\.fff}" : "");
            }
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

        public string OutputText
        {
            get
            {
                string output = Output;

                if (output == null)
                {
                    output = "未指定输出";
                }
                else
                {
                    output = Path.GetFileName(Output);
                }
                if (!string.IsNullOrEmpty(RealOutput))
                {
                    output += $"（实际输出：{Path.GetFileName(RealOutput)}）";
                }
                return output;
            }
        }

        public string StatusText => Status switch
        {
            TaskStatus.Processing => Percent.ToString("0.00%"),
            _ => DescriptionConverter.GetDescription(Status)
        };

        public Brush Color => Status switch
        {
            TaskStatus.Queue => System.Windows.Application.Current.FindResource("SystemControlForegroundBaseHighBrush") as Brush,
            TaskStatus.Processing => Brushes.Orange,
            TaskStatus.Done => Brushes.Green,
            TaskStatus.Error => Brushes.Red,
            TaskStatus.Cancel => Brushes.Gray,
        };

        private StatusDto processStatus;

        public StatusDto ProcessStatus
        {
            get => processStatus;
            set => this.SetValueAndNotify(ref processStatus, value,
                nameof(ProcessStatus),
                nameof(Percent),
                nameof(Status),
                nameof(StatusText));
        }

        public FFmpegManager ProcessManager { get; set; }
        public double Percent => ProcessStatus == null || ProcessStatus.HasDetail == false ? 0 : ProcessStatus.Progress.Percent;
        public bool CancelButtonEnabled => Status is TaskStatus.Queue or TaskStatus.Processing;
        public bool ResetButtonEnabled => Status is TaskStatus.Done or TaskStatus.Cancel or TaskStatus.Error;
        public bool StartButtonEnabled => Status is TaskStatus.Queue;
        private TaskType type;

        public TaskType Type
        {
            get => type;
            set => this.SetValueAndNotify(ref type, value, nameof(Type));
        }

        private TaskStatus status;

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

        private List<InputArguments> inputs;

        public List<InputArguments> Inputs
        {
            get => inputs;
            set => this.SetValueAndNotify(ref inputs, value, nameof(Inputs), nameof(InputText), nameof(InputsText), nameof(IOText));
        }

        private string output;

        public string Output
        {
            get => output;
            set => this.SetValueAndNotify(ref output, value, nameof(Output), nameof(OutputText), nameof(IOText));
        }

        private string realOutput;

        public string RealOutput
        {
            get => realOutput;
            set => this.SetValueAndNotify(ref realOutput, value, nameof(RealOutput));
        }

        private OutputArguments arguments;

        public OutputArguments Arguments
        {
            get => arguments;
            set => this.SetValueAndNotify(ref arguments, value, nameof(Arguments));
        }

        private DateTime createTime;

        public DateTime CreateTime
        {
            get => createTime;
            set => this.SetValueAndNotify(ref createTime, value, nameof(CreateTime));
        }

        private DateTime? startTime;

        public DateTime? StartTime
        {
            get => startTime;
            set => this.SetValueAndNotify(ref startTime, value, nameof(StartTime));
        }

        private DateTime? finishTime;

        public DateTime? FinishTime
        {
            get => finishTime;
            set => this.SetValueAndNotify(ref finishTime, value, nameof(FinishTime));
        }

        private string message;

        public string Message
        {
            get => message;
            set => this.SetValueAndNotify(ref message, value, nameof(Message));
        }

        private string fFmpegArguments;

        public event PropertyChangedEventHandler PropertyChanged;

        public string FFmpegArguments
        {
            get => fFmpegArguments;
            set => this.SetValueAndNotify(ref fFmpegArguments, value, nameof(FFmpegArguments));
        }

        public TaskInfo GetTask()
        {
            return TaskManager.GetTask(Id);
        }

        public TaskInfo ToTask()
        {
            return this.Adapt<TaskInfo>();
        }

        public static UITaskInfo FromTask(TaskInfo task)
        {
            return task.Adapt<UITaskInfo>();
        }

        public void UpdateSelf()
        {
            TaskManager.GetTask(Id).Adapt(this);
        }
    }
}