using FzLib;
using System.ComponentModel;

namespace SimpleFFmpegGUI.Model
{
    public class OutputArguments : INotifyPropertyChanged
    {
        private VideoCodeArguments video;

        public VideoCodeArguments Video
        {
            get => video;
            set => this.SetValueAndNotify(ref video, value, nameof(Video));
        }

        private AudioCodeArguments audio;

        public AudioCodeArguments Audio
        {
            get => audio;
            set => this.SetValueAndNotify(ref audio, value, nameof(Audio));
        }

        private string extra;

        public string Extra
        {
            get => extra;
            set => this.SetValueAndNotify(ref extra, value, nameof(Extra));
        }

        private bool disableVideo;

        public bool DisableVideo
        {
            get => disableVideo;
            set => this.SetValueAndNotify(ref disableVideo, value, nameof(DisableVideo));
        }

        private bool disableAudio;

        public bool DisableAudio
        {
            get => disableAudio;
            set => this.SetValueAndNotify(ref disableAudio, value, nameof(DisableAudio));
        }

        private string format;

        public string Format
        {
            get => format;
            set => this.SetValueAndNotify(ref format, value, nameof(Format));
        }

        private CombineArguments combine;

        public CombineArguments Combine
        {
            get => combine;
            set => this.SetValueAndNotify(ref combine, value, nameof(Combine));
        }

        private ConcatArguments concat;

        public ConcatArguments Concat
        {
            get => concat;
            set => this.SetValueAndNotify(ref concat, value, nameof(Concat));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}