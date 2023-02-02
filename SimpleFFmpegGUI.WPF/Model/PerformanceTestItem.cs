using FzLib;
using SimpleFFmpegGUI.FFmpegLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFFmpegGUI.WPF.Model
{
    public class PerformanceTestCodecParameter
    {
        public int CpuSpeed { get; set; }
        public string ExtraArguments { get; set; }
        public int MaxCpuSpeed { get; set; }
        public string Name { get; set; }
    }

    [DebuggerDisplay("{Codec.Name}")]
    public class PerformanceTestItem : INotifyPropertyChanged
    {
        private bool isChecked;
        private double psnr;
        private double score;
        private double ssim;
        private double vmaf;

        public PerformanceTestItem(VideoCodec codec)
        {
            Codec = codec;
        }

        public PerformanceTestItem()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public VideoCodec Codec { get; set; }
        public bool IsChecked
        {
            get => isChecked;
            set => this.SetValueAndNotify(ref isChecked, value, nameof(IsChecked));
        }
        public double PSNR
        {
            get => psnr;
            set => this.SetValueAndNotify(ref psnr, value, nameof(PSNR));
        }

        public double Score
        {
            get => score;
            set => this.SetValueAndNotify(ref score, value, nameof(Score));
        }

        public double SSIM
        {
            get => ssim;
            set => this.SetValueAndNotify(ref ssim, value, nameof(SSIM));
        }

        public double VMAF
        {
            get => vmaf;
            set => this.SetValueAndNotify(ref vmaf, value, nameof(VMAF));
        }
        public void Clear()
        {
            Score = 0;
            SSIM = 0;
            PSNR = 0;
            VMAF = 0;
        }
    }

    [DebuggerDisplay("{Header}")]
    public class PerformanceTestLine
    {
        public const int CodecsCount = 5;
        public PerformanceTestLine()
        {
            if (VideoCodec.VideoCodecs.Length != CodecsCount)
            {
                throw new Exception("编码数量不对应");
            }
        }
        public void FillItems()
        {
            Items = new PerformanceTestItem[CodecsCount];

            for (int i = 0; i < CodecsCount; i++)
            {
                Items[i] = new PerformanceTestItem(VideoCodec.VideoCodecs[i]);
            }
        }
        public string Header { get; set; }
        public PerformanceTestItem[] Items { get; set; }
        public double MBitrate { get; set; }
    }
}
