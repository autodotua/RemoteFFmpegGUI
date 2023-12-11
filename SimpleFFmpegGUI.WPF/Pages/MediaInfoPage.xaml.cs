using Enterwell.Clients.Wpf.Notifications;
using FzLib;
using FzLib.WPF;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using SimpleFFmpegGUI.Dto;
using SimpleFFmpegGUI.Manager;
using SimpleFFmpegGUI.Model;
using SimpleFFmpegGUI.Model.MediaInfo;
using SimpleFFmpegGUI.WPF.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
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

namespace SimpleFFmpegGUI.WPF.Pages
{
    public class Bitrate2StringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            long num = System.Convert.ToInt64(value);
            string str = NumberConverter.ByteToFitString(num, 2, " bps", " Kbps", " Mbps", " Gbps", " Tbps");
            return str;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Interaction logic for MediaInfoPage.xaml
    /// </summary>
    public partial class MediaInfoPage : UserControl
    {
        public MediaInfoPage(MediaInfoPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = ViewModel;
            InitializeComponent();
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            ViewModel.ReadMediaInfoError += (s, e) => this.CreateMessage().QueueError("读取媒体信息失败", e.ExceptionObject as Exception);
        }

        public MediaInfoPageViewModel ViewModel { get; set; }
        public void SetFile(string file)
        {
            ViewModel.FilePath = file;
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            base.OnDragOver(e);
            if (e.Data.GetDataPresent(DataFormats.FileDrop)
                && (e.Data.GetData(DataFormats.FileDrop) as string[]).Length == 1
                && System.IO.File.Exists((e.Data.GetData(DataFormats.FileDrop) as string[])[0]))
            {
                e.Effects = DragDropEffects.Link;
            }
        }

        protected override void OnDrop(DragEventArgs e)
        {
            base.OnDrop(e);
            if (e.Data.GetDataPresent(DataFormats.FileDrop)
            && (e.Data.GetData(DataFormats.FileDrop) as string[]).Length == 1
            && System.IO.File.Exists((e.Data.GetData(DataFormats.FileDrop) as string[])[0]))
            {
                ViewModel.FilePath = (e.Data.GetData(DataFormats.FileDrop) as string[])[0];
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog().AddAllFilesFilter();

            string path = dialog.GetPath(this.GetWindow());
            if (path != null)
            {
                ViewModel.FilePath = path;
            }
        }

        private void TextBox_MouseLeave(object sender, MouseEventArgs e)
        {
            Keyboard.ClearFocus();
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.Working))
            {
                if (ViewModel.Working)
                {
                    ring.Show();
                }
                else
                {
                    ring.Hide();
                }
            }
        }
    }

    public class MediaInfoPageViewModel : INotifyPropertyChanged
    {
        private string filePath;

        private MediaInfoGeneral mediaInfo;

        private bool working;

        public MediaInfoPageViewModel()
        {
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler<UnhandledExceptionEventArgs> ReadMediaInfoError;

        public string FilePath
        {
            get => filePath;
            set
            {
                this.SetValueAndNotify(ref filePath, value, nameof(FilePath));
                if (!string.IsNullOrWhiteSpace(filePath) && System.IO.File.Exists(value))
                {
                    ShowInfoAsync().ConfigureAwait(false);
                }
            }
        }
        public MediaInfoGeneral MediaInfo
        {
            get => mediaInfo;
            set => this.SetValueAndNotify(ref mediaInfo, value, nameof(MediaInfo));
        }

        public bool Working
        {
            get => working;
            set => this.SetValueAndNotify(ref working, value, nameof(Working));
        }

        private async Task ShowInfoAsync()
        {
            Working = true;
            try
            {
                MediaInfo = await MediaInfoManager.GetMediaInfoAsync(FilePath);
            }
            catch (Exception ex)
            {
                ReadMediaInfoError?.Invoke(this, new UnhandledExceptionEventArgs(ex, false));
            }
            finally
            {
                Working = false;
            }
        }
    }
}