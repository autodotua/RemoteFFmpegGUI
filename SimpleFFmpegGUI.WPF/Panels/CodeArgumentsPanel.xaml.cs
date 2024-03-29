﻿using FzLib;
using FzLib.Collection;
using Mapster;
using Microsoft.DotNet.PlatformAbstractions;
using Microsoft.Extensions.DependencyInjection;
using SimpleFFmpegGUI.FFmpegLib;
using SimpleFFmpegGUI.Manager;
using SimpleFFmpegGUI.Model;
using SimpleFFmpegGUI.WPF.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    public enum ChannelOutputStrategy
    {
        [Description("重新编码")]
        Code,

        [Description("复制")]
        Copy,

        [Description("不导出")]
        Disable,
    }

    public partial class CodeArgumentsPanel : UserControl
    {
        private bool canApplyDefaultPreset = true;

        public CodeArgumentsPanel()
        {
            DataContext = ViewModel;
            InitializeComponent();
        }
        public CodeArgumentsPanelViewModel ViewModel { get; } = App.ServiceProvider.GetService<CodeArgumentsPanelViewModel>();

        public OutputArguments GetOutputArguments()
        {
            return ViewModel.GetArguments();
        }

        public void SetAsClone()
        {
            canApplyDefaultPreset = false;
        }

        public async void Update(TaskType type)
        {
            bool updated = false;
            if (canApplyDefaultPreset)//允许修改参数
            {
                if (Config.Instance.RememberLastArguments)//记住上次输出参数
                {
                    if (Config.Instance.LastOutputArguments.GetOrDefault(type) is OutputArguments lastArguments)
                    {
                        ViewModel.Update(type, lastArguments);
                        //(await this.CreateMessageAsync()).QueueSuccess($"已加载上次任务的参数");
                        updated = true;
                    }
                }
                if (!updated)//记住上次输出参数为False，或不存在上次的参数
                {
                    if (PresetManager.GetDefaultPreset(type) is CodePreset defaultPreset)
                    {
                        ViewModel.Update(type, defaultPreset.Arguments);
                        (await this.CreateMessageAsync()).QueueSuccess($"已加载默认预设“{defaultPreset.Name}”");
                        updated = true;
                    }
                }
            }
            if (!updated)
            {
                ViewModel.Update(type);
            }
        }

        public void Update(TaskType type, OutputArguments arguments)
        {
            ViewModel.Update(type, arguments);
        }
        protected override void OnDragOver(DragEventArgs e)
        {
            base.OnDragOver(e);
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];
                if (files.Length == 1 && File.Exists(files[0]))
                {
                    e.Effects = DragDropEffects.Link;
                }
            }
        }

        protected override async void OnDrop(DragEventArgs e)
        {
            base.OnDrop(e);
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];
                if (files.Length == 1 && File.Exists(files[0]))
                {
                    var file = files[0];
                    try
                    {
                        IsEnabled = false;
                        var info = await MediaInfoManager.GetMediaInfoAsync(file);
                        var videoArgs = MediaInfoManager.ConvertToVideoArguments(info);
                        ViewModel.Video = videoArgs.Adapt<VideoArgumentsWithSwitch>();
                        if (videoArgs.Crf.HasValue)
                        {
                            ViewModel.Video.EnableCrf = true;
                        }
                        if (videoArgs.AverageBitrate.HasValue)
                        {
                            ViewModel.Video.EnableAverageBitrate = true;
                        }
                        if (videoArgs.MaxBitrate.HasValue)
                        {
                            ViewModel.Video.EnableMaxBitrate = true;
                        }
                        this.CreateMessage().QueueSuccess("已加载指定视频参数");
                    }
                    catch (Exception ex)
                    {
                        this.CreateMessage().QueueError("解析视频编码参数失败", ex);
                    }
                    finally
                    {
                        IsEnabled = true;
                    }

                }
            }

        }
    }

    public class CodeArgumentsPanelViewModel : INotifyPropertyChanged
    {
        private AudioArgumentsWithSwitch audio = new AudioArgumentsWithSwitch();

        private ChannelOutputStrategy audioOutputStrategy = ChannelOutputStrategy.Code;

        private bool canSetCombine;

        private bool canSetConcat;

        private bool canSetVideoAndAudio;

        private bool canSpecifyFormat;

        private CombineArguments combine = new CombineArguments();

        private string extra;

        private FormatArgumentWithSwitch format = new FormatArgumentWithSwitch();

        private bool syncModifiedTime;

        private TaskType type;

        private VideoArgumentsWithSwitch video = new VideoArgumentsWithSwitch();

        private ChannelOutputStrategy videoOutputStrategy = ChannelOutputStrategy.Code;

        public CodeArgumentsPanelViewModel()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public OutputArguments Arguments { get; set; }
        public IEnumerable AspectRatios { get; } = new[] { "16:9", "4:3", "1:1", "3:4", "16:9", "2.35" };

        public AudioArgumentsWithSwitch Audio
        {
            get => audio;
            set => this.SetValueAndNotify(ref audio, value, nameof(Audio));
        }

        public IEnumerable AudioBitrates { get; } = new[] { 32, 64, 96, 128, 192, 256, 320 };

        public IEnumerable AudioCodecs { get; } = new[] { "自动", "AAC", "OPUS" };

        public ChannelOutputStrategy AudioOutputStrategy
        {
            get => audioOutputStrategy;
            set
            {
                this.SetValueAndNotify(ref audioOutputStrategy, value, nameof(AudioOutputStrategy));
                if (value == ChannelOutputStrategy.Code && Audio == null)
                {
                    Audio = new AudioArgumentsWithSwitch();
                }
            }
        }

        public IEnumerable AudioSamplingRates { get; } = new[] { 8000, 16000, 32000, 44100, 48000, 96000, 192000 };

        public bool CanSetCombine
        {
            get => canSetCombine;
            set => this.SetValueAndNotify(ref canSetCombine, value, nameof(CanSetCombine));
        }

        public bool CanSetConcat
        {
            get => canSetConcat;
            set => this.SetValueAndNotify(ref canSetConcat, value, nameof(CanSetConcat));
        }

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

        public bool CanSpecifyFormat
        {
            get => canSpecifyFormat;
            set => this.SetValueAndNotify(ref canSpecifyFormat, value, nameof(CanSpecifyFormat));
        }

        public IEnumerable ChannelOutputStrategies => Enum.GetValues(typeof(ChannelOutputStrategy));

        public CombineArguments Combine
        {
            get => combine;
            set => this.SetValueAndNotify(ref combine, value, nameof(Combine));
        }

        public string Extra
        {
            get => extra;
            set => this.SetValueAndNotify(ref extra, value, nameof(Extra));
        }

        public FormatArgumentWithSwitch Format
        {
            get => format;
            set => this.SetValueAndNotify(ref format, value, nameof(Format));
        }

        public IEnumerable Formats => VideoFormat.Formats;

        public IEnumerable Fpses => new double[] { 10, 20, 23.976, 24, 25, 29.97, 30, 48, 59.94, 60, 120 };

        public IEnumerable PixelFormats { get; } = new[] { "yuv420p", "yuvj420p", "yuv422p", "yuvj422p", "rgb24", "gray", "yuv420p10le" };

        public IEnumerable Sizes { get; } = new[] { "-1:2160", "-1:1440", "-1:1080", "-1:720", "-1:576", "-1:480" };

        public bool SyncModifiedTime
        {
            get => syncModifiedTime;
            set => this.SetValueAndNotify(ref syncModifiedTime, value, nameof(SyncModifiedTime));
        }

        public VideoArgumentsWithSwitch Video
        {
            get => video;
            set => this.SetValueAndNotify(ref video, value, nameof(Video));
        }

        public IEnumerable VideoCodecs { get; } = new[] { "自动" }.Concat(VideoCodec.VideoCodecs.Select(p => p.Name));

        public ChannelOutputStrategy VideoOutputStrategy
        {
            get => videoOutputStrategy;
            set
            {
                this.SetValueAndNotify(ref videoOutputStrategy, value, nameof(VideoOutputStrategy));
                if (value == ChannelOutputStrategy.Code && Audio == null)
                {
                    Video = new VideoArgumentsWithSwitch();
                }
            }
        }

        public OutputArguments GetArguments()
        {
            if (VideoOutputStrategy == ChannelOutputStrategy.Code)
            {
                Video.Apply();
            }
            if (AudioOutputStrategy == ChannelOutputStrategy.Code)
            {
                Audio.Apply();
            }
            Format.Apply();
            return new OutputArguments()
            {
                Video = VideoOutputStrategy == ChannelOutputStrategy.Code ? Video.Adapt<VideoCodeArguments>() : null,
                Audio = AudioOutputStrategy == ChannelOutputStrategy.Code ? Audio.Adapt<AudioCodeArguments>() : null,
                Format = Format.Format,
                Combine = Combine,
                Extra = Extra,
                SyncModifiedTime = SyncModifiedTime,
                DisableVideo = VideoOutputStrategy == ChannelOutputStrategy.Disable,
                DisableAudio = audioOutputStrategy == ChannelOutputStrategy.Disable,
            };
        }

        public void Update(TaskType type, OutputArguments argument = null)
        {
            this.type = type;
            CanSpecifyFormat = type is TaskType.Code or TaskType.Combine or TaskType.Concat;
            CanSetVideoAndAudio = type is TaskType.Code;
            CanSetCombine = type is TaskType.Combine;
            CanSetConcat = type is TaskType.Concat;
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
                 (argument.DisableAudio ? ChannelOutputStrategy.Disable : ChannelOutputStrategy.Copy)
                 : ChannelOutputStrategy.Code;
                Format = new FormatArgumentWithSwitch() { Format = argument.Format };
                Format.Update();
                Combine = argument.Combine;
                Extra = argument.Extra;
                SyncModifiedTime = argument.SyncModifiedTime;
            }
        }
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