using FzLib;
using SimpleFFmpegGUI.Model;
using System.ComponentModel;

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
        }

        private bool enableCrf;

        public bool EnableCrf
        {
            get => enableCrf;
            set => this.SetValueAndNotify(ref enableCrf, value, nameof(EnableCrf));
        }

        private bool enableSize;

        public bool EnableSize
        {
            get => enableSize;
            set => this.SetValueAndNotify(ref enableSize, value, nameof(EnableSize));
        }

        private bool enableAspectRatio;

        public bool EnableAspectRatio
        {
            get => enableAspectRatio;
            set => this.SetValueAndNotify(ref enableAspectRatio, value, nameof(EnableAspectRatio));
        }

        private bool enableFps;

        public bool EnableFps
        {
            get => enableFps;
            set => this.SetValueAndNotify(ref enableFps, value, nameof(EnableFps));
        }

        private bool enableAverageBitrate;

        public bool EnableAverageBitrate
        {
            get => enableAverageBitrate;
            set => this.SetValueAndNotify(ref enableAverageBitrate, value, nameof(EnableAverageBitrate));
        }

        private bool enableMaxBitrate;

        public bool EnableMaxBitrate
        {
            get => enableMaxBitrate;
            set => this.SetValueAndNotify(ref enableMaxBitrate, value, nameof(EnableMaxBitrate));
        }

        private bool enablePixelFormat;

        public bool EnablePixelFormat
        {
            get => enablePixelFormat;
            set => this.SetValueAndNotify(ref enablePixelFormat, value, nameof(EnablePixelFormat));
        }

        public void Apply()
        {
            Crf = EnableCrf ? Crf : null;
            Fps = EnableFps ? Fps : null;
            AverageBitrate = EnableAverageBitrate ? AverageBitrate : null;
            MaxBitrate = EnableMaxBitrate ? MaxBitrate : null;
            Size = EnableSize ? Size : null;
            PixelFormat = EnablePixelFormat ? PixelFormat : null;
            AspectRatio = EnablePixelFormat ? AspectRatio : null;
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
        }
    }
}