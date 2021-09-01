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

        public event PropertyChangedEventHandler PropertyChanged;

        public bool EnableMaxBitrate
        {
            get => enableMaxBitrate;
            set => this.SetValueAndNotify(ref enableMaxBitrate, value, nameof(EnableMaxBitrate));
        }

        public void Apply()
        {
            Crf = EnableCrf ? Crf : null;
            Width = EnableSize ? Width : null;
            Height = EnableSize ? Height : null;
            Fps = EnableFps ? Fps : null;
            AverageBitrate = EnableAverageBitrate ? AverageBitrate : null;
            MaxBitrate = EnableMaxBitrate ? MaxBitrate : null;
        }

        public void Update()
        {
            EnableCrf = Crf.HasValue;
            EnableSize = Width.HasValue && Height.HasValue;
            EnableFps = Fps.HasValue;
            EnableAverageBitrate = AverageBitrate.HasValue;
            EnableMaxBitrate = MaxBitrate.HasValue;
        }
    }
}