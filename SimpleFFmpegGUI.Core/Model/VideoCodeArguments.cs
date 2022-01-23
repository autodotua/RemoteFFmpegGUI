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

        private string aspect;

        public string AspectRatio
        {
            get => aspect;
            set => this.SetValueAndNotify(ref aspect, value, nameof(AspectRatio));
        }

        private string size;

        public string Size
        {
            get => size;
            set => this.SetValueAndNotify(ref size, value, nameof(Size));
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