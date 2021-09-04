using FzLib;
using Mapster;
using Microsoft.DotNet.PlatformAbstractions;
using Microsoft.Extensions.DependencyInjection;
using SimpleFFmpegGUI.Manager;
using SimpleFFmpegGUI.Model;
using SimpleFFmpegGUI.WPF.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace SimpleFFmpegGUI.WPF.Panels
{
    public class CodeArgumentsPanelViewModel : INotifyPropertyChanged
    {
        public CodeArgumentsPanelViewModel()
        {
            Concat.PropertyChanged += (s, e) => UpdateWhenConcatArgumentsChanged();
        }

        public OutputArguments Arguments { get; set; }
        private TaskType type;

        public void Update(TaskType type, OutputArguments argument = null)
        {
            this.type = type;

            if (argument != null)
            {
                Video = argument.Video.Adapt<VideoArgumentsWithSwitch>();
                Video?.Update();
                VideoOutputStrategy = argument.Video == null ?
                    (argument.DisableVideo ? ChannelOutputStrategy.Disable : ChannelOutputStrategy.Copy)
                    : ChannelOutputStrategy.Code;
                Audio = argument.Audio.Adapt<AudioArgumentsWithSwitch>();
                Audio?.Update();
                AudioOutputStrategy = argument.Audio == null ?
                 (argument.DisableVideo ? ChannelOutputStrategy.Disable : ChannelOutputStrategy.Copy)
                 : ChannelOutputStrategy.Code;
                Format = new FormatArgumentWithSwitch() { Format = argument.Format };
                Format.Update();
                Combine = argument.Combine;
                Concat = argument.Concat;
                Extra = argument.Extra;
            }

            CanSpecifyFormat = type is TaskType.Code or TaskType.Combine;
            CanSetVideoAndAudio = type is TaskType.Code or TaskType.Concat;
            CanSetCombine = type is TaskType.Combine;
            CanSetConcat = type is TaskType.Concat;
            if (type is TaskType.Concat)
            {
                UpdateWhenConcatArgumentsChanged();
            }
        }

        public OutputArguments GetArguments()
        {
            Video.Apply();
            Audio.Apply();
            Format.Apply();
            return new OutputArguments()
            {
                Video = VideoOutputStrategy == ChannelOutputStrategy.Code ? Video : null,
                Audio = AudioOutputStrategy == ChannelOutputStrategy.Code ? Audio : null,
                Format = Format.Format,
                Combine = Combine,
                Concat = Concat,
                Extra = Extra,
                DisableVideo = VideoOutputStrategy == ChannelOutputStrategy.Disable,
                DisableAudio = audioOutputStrategy == ChannelOutputStrategy.Disable,
            };
        }

        private void UpdateWhenConcatArgumentsChanged()
        {
            CanSetVideoAndAudio = Concat.Type == ConcatType.ViaTs;
            CanSpecifyFormat = Concat.Type == ConcatType.ViaTs;
        }

        private bool canSetVideoAndAudio;

        public bool CanSetVideoAndAudio
        {
            get => canSetVideoAndAudio;
            set
            {
                this.SetValueAndNotify(ref canSetVideoAndAudio, value, nameof(CanSetVideoAndAudio));
                if (value)
                {
                    VideoOutputStrategy = ChannelOutputStrategy.Code;
                    AudioOutputStrategy = ChannelOutputStrategy.Code;
                }
                else
                {
                    VideoOutputStrategy = ChannelOutputStrategy.Disable;
                    AudioOutputStrategy = ChannelOutputStrategy.Disable;
                }
            }
        }

        private bool canSetCombine;

        public bool CanSetCombine
        {
            get => canSetCombine;
            set => this.SetValueAndNotify(ref canSetCombine, value, nameof(CanSetCombine));
        }

        private bool canSetConcat;

        public bool CanSetConcat
        {
            get => canSetConcat;
            set => this.SetValueAndNotify(ref canSetConcat, value, nameof(CanSetConcat));
        }

        private bool canSpecifyFormat;

        public bool CanSpecifyFormat
        {
            get => canSpecifyFormat;
            set => this.SetValueAndNotify(ref canSpecifyFormat, value, nameof(CanSpecifyFormat));
        }

        public IEnumerable ChannelOutputStrategies => Enum.GetValues(typeof(ChannelOutputStrategy));

        private ChannelOutputStrategy videoOutputStrategy = ChannelOutputStrategy.Code;

        public ChannelOutputStrategy VideoOutputStrategy
        {
            get => videoOutputStrategy;
            set => this.SetValueAndNotify(ref videoOutputStrategy, value, nameof(VideoOutputStrategy));
        }

        private ChannelOutputStrategy audioOutputStrategy = ChannelOutputStrategy.Code;

        public ChannelOutputStrategy AudioOutputStrategy
        {
            get => audioOutputStrategy;
            set => this.SetValueAndNotify(ref audioOutputStrategy, value, nameof(AudioOutputStrategy));
        }

        private VideoArgumentsWithSwitch video = new VideoArgumentsWithSwitch();

        public VideoArgumentsWithSwitch Video
        {
            get => video;
            set => this.SetValueAndNotify(ref video, value, nameof(Video));
        }

        private AudioArgumentsWithSwitch audio = new AudioArgumentsWithSwitch();

        public AudioArgumentsWithSwitch Audio
        {
            get => audio;
            set => this.SetValueAndNotify(ref audio, value, nameof(Audio));
        }

        private FormatArgumentWithSwitch format = new FormatArgumentWithSwitch();

        public FormatArgumentWithSwitch Format
        {
            get => format;
            set => this.SetValueAndNotify(ref format, value, nameof(Format));
        }

        private CombineArguments combine = new CombineArguments();

        public CombineArguments Combine
        {
            get => combine;
            set => this.SetValueAndNotify(ref combine, value, nameof(Combine));
        }

        private ConcatArguments concat = new ConcatArguments();

        public ConcatArguments Concat
        {
            get => concat;
            set => this.SetValueAndNotify(ref concat, value, nameof(Concat));
        }

        public IEnumerable ConcatTypes => Enum.GetValues<ConcatType>();

        private string extra;

        public string Extra
        {
            get => extra;
            set => this.SetValueAndNotify(ref extra, value, nameof(Extra));
        }

        public string[] VideoCodes { get; } = new[] { "自动", "H265", "H264", "VP9" };
        public string[] AudioCodes { get; } = new[] { "自动", "AAC", "OPUS" };
        public VideoFormat[] Formats => VideoFormat.Formats;
        public int[] AudioBitrates { get; } = new[] { 32, 64, 96, 128, 192, 256, 320 };
        public int[] AudioSamplingRates { get; } = new[] { 8000, 16000, 32000, 44100, 48000, 96000, 192000 };

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public partial class CodeArgumentsPanel : UserControl
    {
        public CodeArgumentsPanel()
        {
            DataContext = ViewModel;
            InitializeComponent();
            Update(TaskType.Code);
        }

        public void Update(TaskType type)
        {
            ViewModel.Update(type);
        }

        public void Update(TaskInfo task)
        {
            ViewModel.Update(task.Type, task.Arguments);
        }

        public OutputArguments GetOutputArguments()
        {
            return ViewModel.GetArguments();
        }

        public CodeArgumentsPanelViewModel ViewModel { get; } = App.ServiceProvider.GetService<CodeArgumentsPanelViewModel>();
    }

    public enum ChannelOutputStrategy
    {
        [Description("重新编码")]
        Code,

        [Description("复制")]
        Copy,

        [Description("不导出")]
        Disable,
    }

    public class Int2StringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string s)
            {
                Dictionary<int, string> dic = null;
                try
                {
                    dic = s.Split(';').Select(p => p.Split(':')).ToDictionary(p => int.Parse(p[0]), p => p[1]);
                }
                catch (Exception ex)
                {
                    throw new Exception("无法解析参数", ex);
                }
                if (value is int i)
                {
                    if (!dic.ContainsKey(i))
                    {
                        throw new Exception("没有值" + i + "的转换目标");
                    }
                    return dic[i];
                }
                throw new ArgumentException("绑定值必须为整数");
            }
            throw new ArgumentException("参数必须为字符串");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}