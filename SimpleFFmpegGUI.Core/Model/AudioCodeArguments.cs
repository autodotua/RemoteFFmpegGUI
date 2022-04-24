using FzLib;
using System.ComponentModel;

namespace SimpleFFmpegGUI.Model
{
    public class AudioCodeArguments : INotifyPropertyChanged
    {
        private int? bitrate;
        private string code;
        private int? samplingRate;

        public event PropertyChangedEventHandler PropertyChanged;

        public int? Bitrate
        {
            get => bitrate;
            set => this.SetValueAndNotify(ref bitrate, value, nameof(Bitrate));
        }

        public string Code
        {
            get => code;
            set => this.SetValueAndNotify(ref code, value, nameof(Code));
        }
        public int? SamplingRate
        {
            get => samplingRate;
            set => this.SetValueAndNotify(ref samplingRate, value, nameof(SamplingRate));
        }

    }
}