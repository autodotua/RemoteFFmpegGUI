using FzLib;
using SimpleFFmpegGUI.Model;
using System.ComponentModel;

namespace SimpleFFmpegGUI.WPF.Model
{
    public class AudioArgumentsWithSwitch : AudioCodeArguments, ITempArguments
    {
        public AudioArgumentsWithSwitch()
        {
            Bitrate = 128;
            SamplingRate = 48000;
            Code = "自动";
        }

        private bool enableBitrate;

        public bool EnableBitrate
        {
            get => enableBitrate;
            set => this.SetValueAndNotify(ref enableBitrate, value, nameof(EnableBitrate));
        }

        private bool enableSamplingRate;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool EnableSamplingRate
        {
            get => enableSamplingRate;
            set => this.SetValueAndNotify(ref enableSamplingRate, value, nameof(EnableSamplingRate));
        }

        public void Apply()
        {
            Bitrate = EnableBitrate ? Bitrate : null;
            SamplingRate = EnableSamplingRate ? SamplingRate : null;
        }

        public void Update()
        {
            EnableBitrate = Bitrate.HasValue;
            EnableSamplingRate = SamplingRate.HasValue;
        }
    }
}