using Enterwell.Clients.Wpf.Notifications;
using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;
using FzLib;
using FzLib.WPF;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using ModernWpf.FzExtension.CommonDialog;
using SimpleFFmpegGUI.Dto;
using SimpleFFmpegGUI.Manager;
using SimpleFFmpegGUI.Model;
using SimpleFFmpegGUI.Model.MediaInfo;
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
    public static class AsyncEventExtension
    {
        public static Task WaitForEventAsync(this object obj, string eventName)
        {
            var e = obj.GetType().GetEvent(eventName);
            if (e == null)
            {
                throw new ArgumentException($"找不到事件{eventName}");
            }
            TaskCompletionSource tcs = new TaskCompletionSource();
            EventHandler handler = null;
            handler = new EventHandler(Finish);
            e.AddEventHandler(obj, handler);
            return tcs.Task;

            void Finish(object sender, EventArgs a)
            {
                e.RemoveEventHandler(obj, handler);
                tcs.SetResult();
            }
        }
    }

    /// <summary>
    /// Interaction logic for CutWindow.xaml
    /// </summary>
    public partial class CutWindow : Window
    {

        public CutWindow(CutWindowViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;

            DataContext = ViewModel;
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            Unloaded += (s, e) => media.Close();
            media.MessageLogged += async (s, e) =>
            {
                if (e.MessageType == Unosquare.FFME.Common.MediaLogMessageType.Error)
                {
                    await Dispatcher.Invoke(async () =>
                       {
                           await CommonDialog.ShowErrorDialogAsync("加载视频失败", e.Message);
                           Close();
                       });
                }
                Debug.WriteLine(e.Message);
            };

        }

        public string FileName { get; private set; }

        public CutWindowViewModel ViewModel { get; set; }

        public void Initialize(string fileName, TimeSpan? from, TimeSpan? to)
        {
            FileName = fileName;
            if (from.HasValue)
            {
                ViewModel.From = from.Value;
            }
            if (to.HasValue)
            {
                ViewModel.To = to.Value;
            }
        }


        public async Task SetVideoAsync()
        {
            MediaInfoGeneral mediaInfo;
            ViewModel.FilePath = FileName;
            try
            {
                mediaInfo = await MediaInfoManager.GetMediaInfoAsync(FileName);
            }
            catch (Exception ex)
            {
                throw new Exception("查询视频信息失败", ex);
            }
            if (mediaInfo.Videos.Count == 0)
            {
                throw new Exception("文件没有视频流");
            }
            ViewModel.Length = mediaInfo.Duration;
            await media.Open(new Uri(FileName));
        }

        protected override async void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            switch (e.Key)
            {
                case Key.Space:
                    if (media.IsPlaying)
                    {
                        await media.Pause();
                    }
                    else
                    {
                        await media.Play();
                    }
                    break;

                case Key.Left:
                    await media.StepBackward();
                    break;

                case Key.Right:
                    await media.StepForward();
                    break;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private async void JumpButton_Click(object sender, RoutedEventArgs e)
        {
            bool shift = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);
            bool ctrl = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
            int s = shift ? 30 : (ctrl ? 1 : 5);
            int f = shift ? 10 : (ctrl ? 5 : 1);
            switch ((sender as FrameworkElement).Tag as string)
            {
                case "-2":
                    ViewModel.Current = new TimeSpan(Math.Max(0, ViewModel.Current.Ticks - TimeSpan.FromSeconds(s).Ticks));
                    break;

                case "-1":
                case "1":
                    await media.Pause();
                    for (int i = 0; i < f; i++)
                    {
                        if ((sender as FrameworkElement).Tag.Equals("-1"))
                        {
                            await media.StepBackward();
                        }
                        else
                        {
                            await media.StepForward();
                        }
                    }
                    break;

                case "2":
                    ViewModel.Current = new TimeSpan(Math.Min(ViewModel.Length.Ticks, ViewModel.Current.Ticks + TimeSpan.FromSeconds(s).Ticks));
                    break;
            }
        }

        private void JumpToFrom_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Current = ViewModel.From;
        }

        private void JumpToTo_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Current = ViewModel.To;
        }

        private void Media_RenderingVideo(object sender, Unosquare.FFME.Common.RenderingVideoEventArgs e)
        {
            ViewModel.Frame = e.PictureNumber;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private async void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            await media.Pause();
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            await media.Play();
        }

        private void SetFrom_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.Current >= ViewModel.To)
            {
                this.CreateMessage().QueueError("开始时间不可晚于结束时间");
                return;
            }
            ViewModel.From = ViewModel.Current;
            this.CreateMessage().QueueSuccess("已将开始时间设置为" + ViewModel.From.ToString(FindResource("TimeSpanFormat") as string));
        }

        private void SetTo_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.Current <= ViewModel.From)
            {
                this.CreateMessage().QueueError("结束时间不可早于开始时间");
                return;
            }
            ViewModel.To = ViewModel.Current;
            this.CreateMessage().QueueSuccess("已将结束时间设置为" + ViewModel.To.ToString(FindResource("TimeSpanFormat") as string));
        }

        private void Slider_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.CurrentP):

                    break;
            }
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(FileName))
            {
                await CommonDialog.ShowErrorDialogAsync("文件不存在");
                Close();
                return;
            }
            try
            {
                await SetVideoAsync();
            }
            catch (Exception ex)
            {
                await CommonDialog.ShowErrorDialogAsync(ex, "加载视频失败");
                Close();
                return;
            }
        }
    }

    public class CutWindowViewModel : INotifyPropertyChanged
    {
        private TimeSpan current = TimeSpan.Zero;

        private double currentP;

        private string filePath;

        private long frame;

        private TimeSpan from = TimeSpan.Zero;

        private bool isBarEnabled = true;

        private TimeSpan length;

        private TimeSpan to = TimeSpan.Zero;

        public CutWindowViewModel()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public TimeSpan Current
        {
            get => current;
            set
            {
                currentP = value / Length;
                this.SetValueAndNotify(ref current, value, nameof(Current), nameof(CurrentP));
            }
        }

        public double CurrentP
        {
            get => currentP;
            set
            {
                current = Length * value;
                this.SetValueAndNotify(ref currentP, value, nameof(CurrentP), nameof(Current));
            }
        }
        public string FilePath
        {
            get => filePath;
            set => this.SetValueAndNotify(ref filePath, value, nameof(FilePath));
        }

        public long Frame
        {
            get => frame;
            set => this.SetValueAndNotify(ref frame, value, nameof(Frame));
        }

        public TimeSpan From
        {
            get => from;
            set => this.SetValueAndNotify(ref from, value, nameof(From), nameof(FromP));
        }

        public double FromP => From / Length;
        public bool IsBarEnabled
        {
            get => isBarEnabled;
            set => this.SetValueAndNotify(ref isBarEnabled, value, nameof(IsBarEnabled));
        }

        public TimeSpan Length
        {
            get => length;
            set
            {
                to = value;
                this.SetValueAndNotify(ref length, value, nameof(Length), nameof(To), nameof(FromP), nameof(ToP));
            }
        }

        public TimeSpan To
        {
            get => to;
            set => this.SetValueAndNotify(ref to, value, nameof(To), nameof(ToP));
        }

        public double ToP => To / Length;
    }
}