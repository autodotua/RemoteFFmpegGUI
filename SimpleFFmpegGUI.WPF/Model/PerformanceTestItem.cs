using FzLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFFmpegGUI.WPF.Model
{
    public class PerformanceTestLine
    {
        public const int CodecsCount = 4;
        public const int SizesCount = 4;
        public PerformanceTestLine()
        {
            Sizes = new PerformanceTestItem[CodecsCount]
            {
                new PerformanceTestItem(){IsChecked=true },
                new PerformanceTestItem(){IsChecked=true },
                new PerformanceTestItem(){IsChecked=true },
                new PerformanceTestItem(){IsChecked=true },
            };
        }
        public string Header { get; set; }
        public PerformanceTestItem[] Sizes { get; }
    }
    public class PerformanceTestItem:INotifyPropertyChanged
    {
        public bool IsChecked { get; set; }
        private double score;
        private double ssim;
        private double psnr;

        public event PropertyChangedEventHandler PropertyChanged;
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
        public double PSNR
        {
            get => psnr;
            set => this.SetValueAndNotify(ref psnr, value, nameof(PSNR));
        }

    }
}
