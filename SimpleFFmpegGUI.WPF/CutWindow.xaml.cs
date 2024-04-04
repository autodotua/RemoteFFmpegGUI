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
    public class CutWindowViewModel : INotifyPropertyChanged
    {
        public CutWindowViewModel()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private double currentP;

        public double CurrentP
        {
            get => currentP;
            set
            {
                current = Length * value;
                this.SetValueAndNotify(ref currentP, value, nameof(CurrentP), nameof(Current));
            }
        }

        private TimeSpan current = TimeSpan.Zero;

        public TimeSpan Current
        {
            get => current;
            set
            {
                currentP = value / Length;
                this.SetValueAndNotify(ref current, value, nameof(Current), nameof(CurrentP));
            }
        }

        public double FromP => From / Length;
        public double ToP => To / Length;

        private TimeSpan from = TimeSpan.Zero;

        public TimeSpan From
        {
            get => from;
            set => this.SetValueAndNotify(ref from, value, nameof(From), nameof(FromP));
        }

        private TimeSpan to = TimeSpan.Zero;

        public TimeSpan To
        {
            get => to;
            set => this.SetValueAndNotify(ref to, value, nameof(To), nameof(ToP));
        }

        private string filePath;

        public string FilePath
        {
            get => filePath;
            set => this.SetValueAndNotify(ref filePath, value, nameof(FilePath));
        }

        private TimeSpan length;

        public TimeSpan Length
        {
            get => length;
            set
            {
                to = value;
                this.SetValueAndNotify(ref length, value, nameof(Length), nameof(To), nameof(FromP), nameof(ToP));
            }
        }

        private bool isBarEnabled = true;

        public bool IsBarEnabled
        {
            get => isBarEnabled;
            set => this.SetValueAndNotify(ref isBarEnabled, value, nameof(IsBarEnabled));
        }

        private long frame;

        public long Frame
        {
            get => frame;
            set => this.SetValueAndNotify(ref frame, value, nameof(Frame));
        }
    }

    /// <summary>
    /// Interaction logic for CutWindow.xaml
    /// </summary>
    public partial class CutWindow : Window
    {
        private readonly string[] args;

        public CutWindowViewModel ViewModel { get; set; }

        public CutWindow(CutWindowViewModel viewModel, string[] args)
        {
            InitializeComponent();
            ViewModel = viewModel;
            this.args = args;
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


        public async Task SetVideoAsync(string path, TimeSpan? from, TimeSpan? to)
        {
            MediaInfoGeneral mediaInfo;
            ViewModel.FilePath = path;
            try
            {
                mediaInfo = await MediaInfoManager.GetMediaInfoAsync(path);
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
            if (from.HasValue)
            {
                ViewModel.From = from.Value;
            }
            if (to.HasValue)
            {
                ViewModel.To = to.Value;
            }
            //fps = mediaInfo.VideoStreams[0].AvgFrameRate;

            var r = await media.Open(new Uri(path));
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.CurrentP):

                    break;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
        }

        private async void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            await media.Pause();
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            await media.Play();
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

        private void JumpToFrom_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Current = ViewModel.From;
        }

        private void JumpToTo_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Current = ViewModel.To;
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

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            Console.Write("{0},{1}", ViewModel.From, ViewModel.To);
            Application.Current.Shutdown();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
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

        private void Media_RenderingVideo(object sender, Unosquare.FFME.Common.RenderingVideoEventArgs e)
        {
            ViewModel.Frame = e.PictureNumber;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (args.Length != 3)
            {
                await CommonDialog.ShowErrorDialogAsync("参数不全");
                Application.Current.Shutdown();
                return;
            }
            if (!File.Exists(args[0]))
            {
                await CommonDialog.ShowErrorDialogAsync("文件不存在");
                Application.Current.Shutdown();
                return;
            }
            TimeSpan? from = null;
            TimeSpan? to = null;
            if (args[1] != "-")
            {
                if (TimeSpan.TryParse(args[1], out TimeSpan fromValue))
                {
                    from = fromValue;
                }
                else
                {
                    await CommonDialog.ShowErrorDialogAsync("无法解析开始时间");
                    Application.Current.Shutdown();
                    return;
                }
            }
            if (args[1] != "-")
            {
                if (TimeSpan.TryParse(args[2], out TimeSpan toValue))
                {
                    to = toValue;
                }
                else
                {
                    await CommonDialog.ShowErrorDialogAsync("无法解析结束时间");
                    Application.Current.Shutdown();
                    return;
                }
            }
            try
            {
                await SetVideoAsync(args[0], from, to);
            }
            catch (Exception ex)
            {
                await CommonDialog.ShowErrorDialogAsync(ex, "加载视频失败");
                Application.Current.Shutdown();
                return;
            }
        }
    }

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
}