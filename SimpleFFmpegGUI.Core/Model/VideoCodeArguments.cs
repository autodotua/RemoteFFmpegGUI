using FzLib;
using System.ComponentModel;

namespace SimpleFFmpegGUI.Model
{
    public class VideoCodeArguments : INotifyPropertyChanged
    {
        private string aspect;
        private double? averageBitrate;
        private string code;

        private int? crf;

        private double? fps;

        private double? maxBitrate;

        private double? maxBitrateBuffer;

        private string pixelFormat;

        private int preset;

        private string size;

        private bool twoPass;

        public event PropertyChangedEventHandler PropertyChanged;

        public string AspectRatio
        {
            get => aspect;
            set => this.SetValueAndNotify(ref aspect, value, nameof(AspectRatio));
        }

        public double? AverageBitrate
        {
            get => averageBitrate;
            set => this.SetValueAndNotify(ref averageBitrate, value, nameof(AverageBitrate));
        }

        public string Code
        {
            get => code;
            set => this.SetValueAndNotify(ref code, value, nameof(Code));
        }
        public int? Crf
        {
            get => crf;
            set => this.SetValueAndNotify(ref crf, value, nameof(Crf));
        }

        public double? Fps
        {
            get => fps;
            set => this.SetValueAndNotify(ref fps, value, nameof(Fps));
        }

        public double? MaxBitrate
        {
            get => maxBitrate;
            set => this.SetValueAndNotify(ref maxBitrate, value, nameof(MaxBitrate));
        }

        public double? MaxBitrateBuffer
        {
            get => maxBitrateBuffer;
            set => this.SetValueAndNotify(ref maxBitrateBuffer, value, nameof(MaxBitrateBuffer));
        }

        public string PixelFormat
        {
            get => pixelFormat;
            set => this.SetValueAndNotify(ref pixelFormat, value, nameof(PixelFormat));
        }

        public int Preset
        {
            get => preset;
            set => this.SetValueAndNotify(ref preset, value, nameof(Preset));
        }
        public string Size
        {
            get => size;
            set => this.SetValueAndNotify(ref size, value, nameof(Size));
        }
        public bool TwoPass
        {
            get => twoPass;
            set => this.SetValueAndNotify(ref twoPass, value, nameof(TwoPass));
        }

    }
}