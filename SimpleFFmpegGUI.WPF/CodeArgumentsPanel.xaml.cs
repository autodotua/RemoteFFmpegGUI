using FzLib;
using Microsoft.Extensions.DependencyInjection;
using SimpleFFmpegGUI.Manager;
using SimpleFFmpegGUI.Model;
using SimpleFFmpegGUI.WPF;
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

namespace SimpleFFmpegGUI.WPF
{
    public class VideoArgumentsWithSwitch : VideoCodeArguments
    {
        public VideoArgumentsWithSwitch()
        {
            Code = "H265";
            Preset = 2;
            Crf = 25;
            AverageBitrate = 10;
            MaxBitrate = 20;
            MaxBitrateBuffer = 2;
        }

        public bool EnableCrf { get; set; }
        public bool EnableSize { get; set; }
        public bool EnableFps { get; set; }
        public bool EnableAverageBitrate { get; set; }
        public bool EnableMaxBitrate { get; set; }
    }

    public class AudioArgumentsWithSwitch : AudioCodeArguments
    {
        public AudioArgumentsWithSwitch()
        {
               Bitrate = 128;
            SamplingRate = 48000;
        }

        public bool EnableBitrate { get; set; }
        public bool EnableSamplingRate { get; set; }
    }

    public class FormatArgumentWithSwitch
    {
        public bool EnableFormat { get; set; }
        public string Format { get; set; } = "mp4";
    }

    public class CodeArgumentsPanelViewModel : INotifyPropertyChanged
    {
        public CodeArgumentsPanelViewModel()
        {
        }

        public OutputArguments Arguments { get; set; }

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
        }

        public CodeArgumentsPanelViewModel ViewModel => App.ServiceProvider.GetService<CodeArgumentsPanelViewModel>();
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