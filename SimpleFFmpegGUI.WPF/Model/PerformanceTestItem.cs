using FzLib;
using SimpleFFmpegGUI.ConstantData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFFmpegGUI.WPF.Model
{
    public class PerformanceTestItem : INotifyPropertyChanged
    {
        private bool isChecked;
        private double psnr;

        private double score;

        private double ssim;

        public event PropertyChangedEventHandler PropertyChanged;

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

        public void Clear()
        {
            Score = 0;
            SSIM = 0;
            PSNR = 0;
        }
    }

    public class PerformanceTestLine
    {
        public const int CodecsCount = 5;
        public PerformanceTestLine()
        {
            Items = new PerformanceTestItem[CodecsCount];

            for (int i = 0; i < CodecsCount; i++)
            {
                Items[i] = new PerformanceTestItem();
            }
        }
        public string Header { get; set; }
        public PerformanceTestItem[] Items { get; }
        public double MBitrate { get; set; }
    }

    public class PerformanceTestCodecParameter
    {
        public string Name { get; set; }
        public string ExtraArguments { get; set; }
        public int CpuSpeed { get; set; }
        public int MaxCpuSpeed { get; set; }
    }
}
