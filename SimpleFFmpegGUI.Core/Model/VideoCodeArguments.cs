using FzLib;
using System.ComponentModel;

namespace SimpleFFmpegGUI.Model
{
    public class VideoCodeArguments : INotifyPropertyChanged
    {
        private string code;

        public string Code
        {
            get => code;
            set => this.SetValueAndNotify(ref code, value, nameof(Code));
        }

        private int preset;

        public int Preset
        {
            get => preset;
            set => this.SetValueAndNotify(ref preset, value, nameof(Preset));
        }

        private int? crf;

        public int? Crf
        {
            get => crf;
            set => this.SetValueAndNotify(ref crf, value, nameof(Crf));
        }

        private int? width;

        public int? Width
        {
            get => width;
            set => this.SetValueAndNotify(ref width, value, nameof(Width));
        }

        private int? height;

        public int? Height
        {
            get => height;
            set => this.SetValueAndNotify(ref height, value, nameof(Height));
        }

        private double? fps;

        public double? Fps
        {
            get => fps;
            set => this.SetValueAndNotify(ref fps, value, nameof(Fps));
        }

        private double? averageBitrate;

        public double? AverageBitrate
        {
            get => averageBitrate;
            set => this.SetValueAndNotify(ref averageBitrate, value, nameof(AverageBitrate));
        }

        private double? maxBitrate;

        public double? MaxBitrate
        {
            get => maxBitrate;
            set => this.SetValueAndNotify(ref maxBitrate, value, nameof(MaxBitrate));
        }

        private double? maxBitrateBuffer;

        public event PropertyChangedEventHandler PropertyChanged;

        public double? MaxBitrateBuffer
        {
            get => maxBitrateBuffer;
            set => this.SetValueAndNotify(ref maxBitrateBuffer, value, nameof(MaxBitrateBuffer));
        }

        private string pixelFormat;

        public string PixelFormat
        {
            get => pixelFormat;
            set => this.SetValueAndNotify(ref pixelFormat, value, nameof(PixelFormat));
        }
    }
}