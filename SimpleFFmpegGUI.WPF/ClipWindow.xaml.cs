using Enterwell.Clients.Wpf.Notifications;
using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;
using FzLib;
using FzLib.WPF;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using SimpleFFmpegGUI.Dto;
using SimpleFFmpegGUI.Manager;
using SimpleFFmpegGUI.Model;
using SimpleFFmpegGUI.WPF.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;
using Size = System.Drawing.Size;

namespace SimpleFFmpegGUI.WPF
{
    public class ClipWindowViewModel : INotifyPropertyChanged
    {
        public ClipWindowViewModel()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private double percent;

        public double Percent
        {
            get => percent;
            set => this.SetValueAndNotify(ref percent, value, nameof(Percent));
        }

        private TimeSpan current = TimeSpan.Zero;

        public TimeSpan Current
        {
            get => current;
            set => this.SetValueAndNotify(ref current, value, nameof(Current));
        }

        private TimeSpan from;

        public TimeSpan From
        {
            get => from;
            set => this.SetValueAndNotify(ref from, value, nameof(From));
        }

        private TimeSpan to;

        public TimeSpan To
        {
            get => to;
            set => this.SetValueAndNotify(ref to, value, nameof(To));
        }

        private ImageSource image;

        public ImageSource Image
        {
            get => image;
            set => this.SetValueAndNotify(ref image, value, nameof(Image));
        }

        private string filePath;

        public string FilePath
        {
            get => filePath;
            set => this.SetValueAndNotify(ref filePath, value, nameof(FilePath));
        }
    }

    /// <summary>
    /// Interaction logic for ClipWindow.xaml
    /// </summary>
    public partial class ClipWindow : Window
    {
        public ClipWindowViewModel ViewModel { get; set; }

        private TimeSpan length;
        private double fps;
        private Timer timer;
        private TimeSpan? requestedTime;
        private bool isUpdating = false;

        public ClipWindow(ClipWindowViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = ViewModel;
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            InitializeComponent();
            timer = new Timer(new TimerCallback(o => CheckAndUpdateImage()), null, 0, 500);
        }

        private async Task CheckAndUpdateImage()
        {
            if (isUpdating)
            {
                return;
            }
            isUpdating = true;
            while (requestedTime.HasValue)
            {
                ViewModel.Current = requestedTime.Value;
                requestedTime = null;
                await Dispatcher.InvokeAsync(async () =>
                 ViewModel.Image = new BitmapImage(new Uri(await MediaInfoManager.GetSnapshotAsync(ViewModel.FilePath, ViewModel.Current))));
            }
            isUpdating = false;
        }

        public async Task SetVideo(string path)
        {
            MediaInfoDto mediaInfo;
            ViewModel.FilePath = path;
            try
            {
                mediaInfo = await MediaInfoManager.GetMediaInfoAsync(path, false);
            }
            catch (Exception ex)
            {
                throw new Exception("查询视频信息失败", ex);
            }
            if (mediaInfo.VideoStreams.Count == 0)
            {
                throw new Exception("文件没有视频流");
            }
            length = mediaInfo.Duration;
            fps = mediaInfo.VideoStreams[0].AvgFrameRate;
            requestedTime = TimeSpan.FromSeconds(0);
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.Percent):
                    TimeSpan time = length * ViewModel.Percent;
                    requestedTime = time;
                    break;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //vlc.SetPause(true);
        }

        private void NextFrame_Click(object sender, RoutedEventArgs e)
        {
            requestedTime = ViewModel.Current + (TimeSpan.FromSeconds(1) / fps-TimeSpan.FromMilliseconds(1));
            Debug.WriteLine(requestedTime);
            Debug.WriteLine(requestedTime.Value.TotalMilliseconds * fps);
            CheckAndUpdateImage();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            //vlc.Dispose();
        }

        private void LastFrame_Click(object sender, RoutedEventArgs e)
        {
            requestedTime = ViewModel.Current - (TimeSpan.FromSeconds(1) / fps - TimeSpan.FromMilliseconds(1));
            Debug.WriteLine(requestedTime);
            Debug.WriteLine(requestedTime.Value.TotalMilliseconds * fps);
            CheckAndUpdateImage();
        }
    }
}