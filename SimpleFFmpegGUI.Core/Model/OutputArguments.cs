using FzLib;
using System.ComponentModel;

namespace SimpleFFmpegGUI.Model
{
    public class OutputArguments : INotifyPropertyChanged
    {
        private AudioCodeArguments audio;
        private CombineArguments combine;
        private ConcatArguments concat;
        private bool disableAudio;
        private bool disableVideo;
        private string extra;
        private string format;
        private StreamArguments stream;
        private VideoCodeArguments video;

        public event PropertyChangedEventHandler PropertyChanged;

        public AudioCodeArguments Audio
        {
            get => audio;
            set => this.SetValueAndNotify(ref audio, value, nameof(Audio));
        }

        public CombineArguments Combine
        {
            get => combine;
            set => this.SetValueAndNotify(ref combine, value, nameof(Combine));
        }

        public ConcatArguments Concat
        {
            get => concat;
            set => this.SetValueAndNotify(ref concat, value, nameof(Concat));
        }

        public bool DisableAudio
        {
            get => disableAudio;
            set => this.SetValueAndNotify(ref disableAudio, value, nameof(DisableAudio));
        }

        public bool DisableVideo
        {
            get => disableVideo;
            set => this.SetValueAndNotify(ref disableVideo, value, nameof(DisableVideo));
        }

        public string Extra
        {
            get => extra;
            set => this.SetValueAndNotify(ref extra, value, nameof(Extra));
        }

        public string Format
        {
            get => format;
            set => this.SetValueAndNotify(ref format, value, nameof(Format));
        }

        public StreamArguments Stream
        {
            get => stream;
            set => this.SetValueAndNotify(ref stream, value, nameof(Stream));
        }

        public VideoCodeArguments Video
        {
            get => video;
            set => this.SetValueAndNotify(ref video, value, nameof(Video));
        }
    }
}