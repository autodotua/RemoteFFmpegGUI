using CommunityToolkit.Mvvm.ComponentModel;
using FzLib;
using FzLib.Collection;
using Mapster;
using Newtonsoft.Json.Linq;
using SimpleFFmpegGUI.FFmpegLib;
using SimpleFFmpegGUI.Manager;
using SimpleFFmpegGUI.Model;
using SimpleFFmpegGUI.WPF.ViewModels;
using SimpleFFmpegGUI.WPF.Panels;
using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleFFmpegGUI.WPF.ViewModels
{
    public partial class CodeArgumentsPanelViewModel : ViewModelBase
    {
        private readonly PresetManager presetManager;

        private AudioArgumentsViewModel audio = new AudioArgumentsViewModel();

        [ObservableProperty]
        private ChannelOutputStrategy audioOutputStrategy = ChannelOutputStrategy.Copy;

        [ObservableProperty]
        private bool canApplyDefaultPreset = true;

        [ObservableProperty]
        private bool canSetCombine;

        [ObservableProperty]
        private bool canSetConcat;

        [ObservableProperty]
        private bool canSetVideoAndAudio;

        [ObservableProperty]
        private bool canSpecifyFormat;

        [ObservableProperty]
        private CombineArguments combine = new CombineArguments();

        [ObservableProperty]
        private string extra;

        [ObservableProperty]
        private FormatArgumentViewModel format = new FormatArgumentViewModel();

        [ObservableProperty]
        private ProcessedOptions processedOptions = new ProcessedOptions();

        private TaskType type;

        [ObservableProperty]
        private VideoArgumentsViewModel video = new VideoArgumentsViewModel();

        [ObservableProperty]
        private ChannelOutputStrategy videoOutputStrategy = ChannelOutputStrategy.Copy;

        public CodeArgumentsPanelViewModel(PresetManager presetManager)
        {
            this.presetManager = presetManager;
        }

        public OutputArguments Arguments { get; set; }

        public IEnumerable AspectRatios { get; } = new[] { "16:9", "4:3", "1:1", "3:4", "16:9", "2.35" };

        public AudioArgumentsViewModel Audio
        {
            get => audio;
            set => this.SetValueAndNotify(ref audio, value, nameof(Audio));
        }

        public IEnumerable AudioBitrates { get; } = new[] { 32, 64, 96, 128, 192, 256, 320 };

        public IEnumerable AudioCodecs { get; } = new[] { "自动", "AAC", "OPUS" };

        public IEnumerable AudioSamplingRates { get; } = new[] { 8000, 16000, 32000, 44100, 48000, 96000, 192000 };

        public IEnumerable ChannelOutputStrategies => Enum.GetValues(typeof(ChannelOutputStrategy));

        public IEnumerable Formats => VideoFormat.Formats;

        public IEnumerable Fpses => new double[] { 10, 20, 23.976, 24, 25, 29.97, 30, 48, 59.94, 60, 120 };

        public IEnumerable PixelFormats { get; } = new[] { "yuv420p", "yuvj420p", "yuv422p", "yuvj422p", "rgb24", "gray", "yuv420p10le" };

        public IEnumerable Sizes { get; } = new[] { "-1:2160", "-1:1440", "-1:1080", "-1:720", "-1:576", "-1:480" };

        public IEnumerable VideoCodecs { get; } = new[] { "自动" }.Concat(VideoCodec.VideoCodecs.Select(p => p.Name));

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
                ProcessedOptions = ProcessedOptions,
                DisableVideo = VideoOutputStrategy == ChannelOutputStrategy.Disable,
                DisableAudio = AudioOutputStrategy == ChannelOutputStrategy.Disable,
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
                Video = argument.Video.Adapt<VideoArgumentsViewModel>();
                Video?.Update();
                VideoOutputStrategy = argument.Video == null ?
                    argument.DisableVideo ? ChannelOutputStrategy.Disable : ChannelOutputStrategy.Copy
                    : ChannelOutputStrategy.Code;
                Audio = argument.Audio.Adapt<AudioArgumentsViewModel>();
                Audio?.Update();
                AudioOutputStrategy = argument.Audio == null ?
                 argument.DisableAudio ? ChannelOutputStrategy.Disable : ChannelOutputStrategy.Copy
                 : ChannelOutputStrategy.Code;
                Format = new FormatArgumentViewModel() { Format = argument.Format };
                Format.Update();
                Combine = argument.Combine;
                ProcessedOptions = argument.ProcessedOptions ?? new ProcessedOptions();
                Extra = argument.Extra;
            }
        }

        public async Task UpdateTypeAsync(TaskType type)
        {
            bool updated = false;
            if (CanApplyDefaultPreset)//允许修改参数
            {
                if (Config.Instance.RememberLastArguments)//记住上次输出参数
                {
                    if (Config.Instance.LastOutputArguments.GetOrDefault(type) is OutputArguments lastArguments)
                    {
                        Update(type, lastArguments);
                        //(await this.CreateMessageAsync()).QueueSuccess($"已加载上次任务的参数");
                        updated = true;
                    }
                }
                if (!updated)//记住上次输出参数为False，或不存在上次的参数
                {
                    if ((await presetManager.GetDefaultPresetAsync(type)) is CodePreset defaultPreset)
                    {
                        Update(type, defaultPreset.Arguments);
                        QueueSuccessMessage($"已加载默认预设“{defaultPreset.Name}”");
                        updated = true;
                    }
                }
            }
            if (!updated)
            {
                Update(type);
            }
        }

        partial void OnCanSetVideoAndAudioChanged(bool value)
        {
            if (CanSetVideoAndAudio)
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

        partial void OnVideoOutputStrategyChanged(ChannelOutputStrategy value)
        {
            if (value == ChannelOutputStrategy.Code && Video == null)
            {
                Video = new VideoArgumentsViewModel();
            }
        }
        partial void OnAudioOutputStrategyChanged(ChannelOutputStrategy value)
        {
            if (value == ChannelOutputStrategy.Code && Audio == null)
            {
                Audio = new AudioArgumentsViewModel();
            }
        }
    }
}