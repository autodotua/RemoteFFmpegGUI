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

        /// <summary>
        /// 画面比例
        /// </summary>
        public string AspectRatio
        {
            get => aspect;
            set => this.SetValueAndNotify(ref aspect, value, nameof(AspectRatio));
        }

        /// <summary>
        /// 平均码率
        /// </summary>
        public double? AverageBitrate
        {
            get => averageBitrate;
            set => this.SetValueAndNotify(ref averageBitrate, value, nameof(AverageBitrate));
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
        /// CRF（视频目标质量）
        /// </summary>
        public int? Crf
        {
            get => crf;
            set => this.SetValueAndNotify(ref crf, value, nameof(Crf));
        }

        /// <summary>
        /// 帧率
        /// </summary>
        public double? Fps
        {
            get => fps;
            set => this.SetValueAndNotify(ref fps, value, nameof(Fps));
        }

        /// <summary>
        /// 最大码率
        /// </summary>
        public double? MaxBitrate
        {
            get => maxBitrate;
            set => this.SetValueAndNotify(ref maxBitrate, value, nameof(MaxBitrate));
        }

        /// <summary>
        /// 最大码率缓冲倍率
        /// </summary>
        public double? MaxBitrateBuffer
        {
            get => maxBitrateBuffer;
            set => this.SetValueAndNotify(ref maxBitrateBuffer, value, nameof(MaxBitrateBuffer));
        }

        /// <summary>
        /// 像素格式
        /// </summary>
        public string PixelFormat
        {
            get => pixelFormat;
            set => this.SetValueAndNotify(ref pixelFormat, value, nameof(PixelFormat));
        }

        /// <summary>
        /// 编码速度或速度预设
        /// </summary>
        public int Preset
        {
            get => preset;
            set => this.SetValueAndNotify(ref preset, value, nameof(Preset));
        } 

        /// <summary>
        /// 视频尺寸（分辨率）
        /// </summary>
        public string Size
        {
            get => size;
            set => this.SetValueAndNotify(ref size, value, nameof(Size));
        }

        /// <summary>
        /// 是否二次编码
        /// </summary>
        public bool TwoPass
        {
            get => twoPass;
            set => this.SetValueAndNotify(ref twoPass, value, nameof(TwoPass));
        }
    }
}