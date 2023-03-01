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

        /// <summary>
        /// 码率
        /// </summary>
        public int? Bitrate
        {
            get => bitrate;
            set => this.SetValueAndNotify(ref bitrate, value, nameof(Bitrate));
        }

        /// <summary>
        /// 编码
        /// </summary>
        public string Code
        {
            get => code;
            set => this.SetValueAndNotify(ref code, value, nameof(Code));
        }

        /// <summary>
        /// 采样率
        /// </summary>
        public int? SamplingRate
        {
            get => samplingRate;
            set => this.SetValueAndNotify(ref samplingRate, value, nameof(SamplingRate));
        }
    }
}