using FzLib;
using System;
using System.ComponentModel;

namespace SimpleFFmpegGUI.Model
{
    public class InputArguments:INotifyPropertyChanged
    {
        private string filePath;
        public string FilePath
        {
            get => filePath;
            set => this.SetValueAndNotify(ref filePath, value, nameof(FilePath));
        }
        private TimeSpan? from;
        public TimeSpan? From
        {
            get => from;
            set => this.SetValueAndNotify(ref from, value, nameof(From));
        }
        private TimeSpan? to;
        public TimeSpan? To
        {
            get => to;
            set => this.SetValueAndNotify(ref to, value, nameof(To));
        }
        private TimeSpan? duration;

        public event PropertyChangedEventHandler PropertyChanged;

        public TimeSpan? Duration
        {
            get => duration;
            set => this.SetValueAndNotify(ref duration, value, nameof(Duration));
        }
        public string Format { get; set; }
        public string Extra { get; set; }
    }
}