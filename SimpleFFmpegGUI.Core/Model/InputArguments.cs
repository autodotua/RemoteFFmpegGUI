using FzLib;
using System;
using System.ComponentModel;

namespace SimpleFFmpegGUI.Model
{
    public class InputArguments : INotifyPropertyChanged
    {
        private TimeSpan? duration;

        private string filePath;

        private string format;

        private double? framerate;

        private TimeSpan? from;

        private bool image2;

        private string extra;

        private TimeSpan? to;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 持续时间
        /// </summary>
        public TimeSpan? Duration
        {
            get => duration;
            set => this.SetValueAndNotify(ref duration, value, nameof(Duration));
        }

        /// <summary>
        /// 其他参数
        /// </summary>
        public string Extra
        {
            get => extra;
            set => this.SetValueAndNotify(ref extra, value, nameof(Extra));
        }


        /// <summary>
        /// 输入文件的路径
        /// </summary>
        public string FilePath
        {
            get => filePath;
            set => this.SetValueAndNotify(ref filePath, value, nameof(FilePath));
        }
        /// <summary>
        /// 输入格式
        /// </summary>
        public string Format
        {
            get
            {
                if (!string.IsNullOrEmpty(format) && format != "image2" && image2)
                {
                    throw new Exception("不可以同时指定输入为帧序列又指定输入格式");
                }
                return image2 ? "image2" : format;
            }
            set => this.SetValueAndNotify(ref format, value, nameof(Format));
        }

        /// <summary>
        /// 输入帧率（主要针对图像序列）
        /// </summary>
        public double? Framerate
        {
            get => framerate;
            set => this.SetValueAndNotify(ref framerate, value, nameof(Framerate));
        }
        /// <summary>
        /// 开始时间
        /// </summary>
        public TimeSpan? From
        {
            get => from;
            set => this.SetValueAndNotify(ref from, value, nameof(From));
        }

        /// <summary>
        /// 输入是否为图像帧序列
        /// </summary>
        public bool Image2
        {
            get => image2;
            set
            {
                this.SetValueAndNotify(ref image2, value, nameof(Image2));
                if (value && !Framerate.HasValue)
                {
                    Framerate = 30;
                }
            }
        }

        /// <summary>
        /// 结束时间
        /// </summary>
        public TimeSpan? To
        {
            get => to;
            set => this.SetValueAndNotify(ref to, value, nameof(To));
        }
    }
}