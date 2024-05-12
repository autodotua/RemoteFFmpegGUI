using FzLib;
using SimpleFFmpegGUI.FFmpegLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFFmpegGUI.WPF.ViewModels
{
    public class PerformanceTestCodecParameter : INotifyPropertyChanged
    {
        private double bitrateFacor = 1;
        public event PropertyChangedEventHandler PropertyChanged;

        public double BitrateFactor
        {
            get => bitrateFacor;
            set
            {
                if (value < 0.5) value = 0.5;
                if (value > 2) value = 2;
                this.SetValueAndNotify(ref bitrateFacor, value, nameof(BitrateFactor));
            }
        }

        public int CpuSpeed { get; set; }
        public int CRF { get; set; }
        public string ExtraArguments { get; set; }
        public int MaxCpuSpeed { get; set; }
        public int MaxCRF { get; set; }
        public string Name { get; set; }
    }

    [DebuggerDisplay("{Codec}")]
    public class PerformanceTestItem : INotifyPropertyChanged
    {
        private double cpu;
        private bool isChecked;
        private double outputSize;
        private TimeSpan processDuration;
        private double psnr;
        private double score;
        private double ssim;
        private double vmaf;
        public PerformanceTestItem(string codec)
        {
            Codec = codec;
        }

        public PerformanceTestItem()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public string Codec { get; set; }
        public double CpuUsage
        {
            get => cpu;
            set => this.SetValueAndNotify(ref cpu, value, nameof(CpuUsage));
        }

        public double FPS
        {
            get => score;
            set => this.SetValueAndNotify(ref score, value, nameof(FPS));
        }

        public bool IsChecked
        {
            get => isChecked;
            set => this.SetValueAndNotify(ref isChecked, value, nameof(IsChecked));
        }
        public double OutputSize
        {
            get => outputSize;
            set => this.SetValueAndNotify(ref outputSize, value, nameof(OutputSize));
        }

        public TimeSpan ProcessDuration
        {
            get => processDuration;
            set => this.SetValueAndNotify(ref processDuration, value, nameof(ProcessDuration));
        }

        public double PSNR
        {
            get => psnr;
            set => this.SetValueAndNotify(ref psnr, value, nameof(PSNR));
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
            FPS = 0;
            SSIM = 0;
            PSNR = 0;
            VMAF = 0;
            CpuUsage = 0;
            OutputSize = 0;
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
        public string Header { get; set; }

        public PerformanceTestItem[] Items { get; set; }

        public double MBitrate { get; set; }

        public void FillItems()
        {
            Items = new PerformanceTestItem[CodecsCount];

            for (int i = 0; i < CodecsCount; i++)
            {
                Items[i] = new PerformanceTestItem(VideoCodec.VideoCodecs[i].Name);
            }
        }
    }
}
