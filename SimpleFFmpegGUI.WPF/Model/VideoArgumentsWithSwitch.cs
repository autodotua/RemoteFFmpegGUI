using FzLib;
using SimpleFFmpegGUI.FFmpegLib;
using SimpleFFmpegGUI.Model;
using System.ComponentModel;
using System.Linq;

namespace SimpleFFmpegGUI.WPF.Model
{
    public class VideoArgumentsWithSwitch : VideoCodeArguments, ITempArguments
    {
        public VideoArgumentsWithSwitch()
        {
            Code = "H265";
            Preset = 2;
            Crf = 25;
            AverageBitrate = 10;
            MaxBitrate = 20;
            MaxBitrateBuffer = 2;
            VideoArgumentsWithSwitch_PropertyChanged(this, new PropertyChangedEventArgs(nameof(Code)));
            PropertyChanged += VideoArgumentsWithSwitch_PropertyChanged;
        }

        private void VideoArgumentsWithSwitch_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(TwoPass) when TwoPass:
                    EnableCrf = false;
                    EnableAverageBitrate = true;
                    break;
                case nameof(EnableCrf) when EnableCrf:
                    TwoPass = false;
                    EnableAverageBitrate = false;
                    break;
                case nameof(Code):
                    var codec = VideoCodec.GetCodec(Code) ?? VideoCodec.General;
                    MaxPreset = codec.MaxSpeedLevel;
                    Preset = codec.DefaultSpeedLevel;
                    MaxCRF = codec.MaxCRF;
                    Crf = codec.DefaultCRF;
                    break;
            }
        }

        private bool enableCrf;

        public bool EnableCrf
        {
            get => enableCrf;
            set
            {
                this.SetValueAndNotify(ref enableCrf, value, nameof(EnableCrf));
                if (value && Crf == null)
                {
                    Crf = 25;
                }
            }
        }

        private bool enableSize;

        public bool EnableSize
        {
            get => enableSize;
            set
            {
                this.SetValueAndNotify(ref enableSize, value, nameof(EnableSize));
                if (value && string.IsNullOrWhiteSpace(Size))
                {
                    Size = "-1:1080";
                }
            }
        }

        private bool enableAspectRatio;

        public bool EnableAspectRatio
        {
            get => enableAspectRatio;
            set
            {
                this.SetValueAndNotify(ref enableAspectRatio, value, nameof(EnableAspectRatio));
                if (value && string.IsNullOrWhiteSpace(AspectRatio))
                {
                    AspectRatio = "16:9";
                }
            }
        }

        private bool enableFps;

        public bool EnableFps
        {
            get => enableFps;
            set
            {
                this.SetValueAndNotify(ref enableFps, value, nameof(EnableFps));
                if (value && Fps == null)
                {
                    Fps = 30;
                }
            }
        }

        private bool enableAverageBitrate;

        public bool EnableAverageBitrate
        {
            get => enableAverageBitrate;
            set
            {
                this.SetValueAndNotify(ref enableAverageBitrate, value, nameof(EnableAverageBitrate));
                if (value && AverageBitrate == null)
                {
                    AverageBitrate = 10;
                }
            }
        }

        private bool enableMaxBitrate;

        public bool EnableMaxBitrate
        {
            get => enableMaxBitrate;
            set
            {
                this.SetValueAndNotify(ref enableMaxBitrate, value, nameof(EnableMaxBitrate));
                if (value && MaxBitrate == null)
                {
                    MaxBitrate = 20;
                }
            }
        }

        private bool enablePixelFormat;

        public bool EnablePixelFormat
        {
            get => enablePixelFormat;
            set
            {
                this.SetValueAndNotify(ref enablePixelFormat, value, nameof(EnablePixelFormat));
                if (value && string.IsNullOrWhiteSpace(PixelFormat))
                {
                    PixelFormat = "yuv420p";
                }
            }
        }

        private int maxPreset;
        public int MaxPreset
        {
            get => maxPreset;
            set => this.SetValueAndNotify(ref maxPreset, value, nameof(MaxPreset));
        }
        private int maxCRF;
        public int MaxCRF
        {
            get => maxCRF;
            set => this.SetValueAndNotify(ref maxCRF, value, nameof(MaxCRF));
        }
        public void Apply()
        {
            Crf = EnableCrf ? Crf : null;
            Fps = EnableFps ? Fps : null;
            AverageBitrate = EnableAverageBitrate ? AverageBitrate : null;
            MaxBitrate = EnableMaxBitrate ? MaxBitrate : null;
            Size = EnableSize ? Size : null;
            PixelFormat = EnablePixelFormat ? PixelFormat : null;
            AspectRatio = EnableAspectRatio ? AspectRatio : null;
        }

        public void Update()
        {
            EnableCrf = Crf.HasValue;
            EnableFps = Fps.HasValue;
            EnableAverageBitrate = AverageBitrate.HasValue;
            EnableMaxBitrate = MaxBitrate.HasValue;
            EnableSize = !string.IsNullOrEmpty(Size);
            EnablePixelFormat = !string.IsNullOrEmpty(PixelFormat);
            EnableAspectRatio = !string.IsNullOrEmpty(AspectRatio);
            //从数据库提取的Argument，若某些开关没有启动，则其值为null，这不符合UI，因此需要对null值进行赋初始值。
            Crf ??= 25;
            MaxBitrate ??= 20;
            AverageBitrate = AverageBitrate ?? 10;
            MaxBitrateBuffer ??= 2;
        }
    }
}