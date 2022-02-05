using Enterwell.Clients.Wpf.Notifications;
using FzLib;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAPICodePack.FzExtension;
using SimpleFFmpegGUI.Dto;
using SimpleFFmpegGUI.Manager;
using SimpleFFmpegGUI.Model;
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

namespace SimpleFFmpegGUI.WPF
{
    public class MediaInfoWindowViewModel : INotifyPropertyChanged
    {
        public MediaInfoWindowViewModel()
        {
        }

        private string filePath;

        public string FilePath
        {
            get => filePath;
            set
            {
                this.SetValueAndNotify(ref filePath, value, nameof(FilePath));
                if (!string.IsNullOrWhiteSpace(filePath) && System.IO.File.Exists(value))
                {
                    ShowInfoAsync();
                }
            }
        }

        private bool working;

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
            }
            finally
            {
                Working = false;
            }
        }

        private MediaInfoDto mediaInfo;

        public MediaInfoDto MediaInfo
        {
            get => mediaInfo;
            set => this.SetValueAndNotify(ref mediaInfo, value, nameof(MediaInfo));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    /// <summary>
    /// Interaction logic for MediaInfoWindow.xaml
    /// </summary>
    public partial class MediaInfoWindow : Window
    {
        public MediaInfoWindowViewModel ViewModel { get; set; }

        public MediaInfoWindow(MediaInfoWindowViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = ViewModel;
            InitializeComponent();
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
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

        private void TextBox_MouseLeave(object sender, MouseEventArgs e)
        {
            Keyboard.ClearFocus();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string path = new FileFilterCollection().AddAll().CreateOpenFileDialog().SetParent(this).GetFilePath();
            if (path != null)
            {
                ViewModel.FilePath = path;
            }
        }

        public void SetFile(string file)
        {
            ViewModel.FilePath = file;
        }
    }

    public class Bitrate2StringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            long num = System.Convert.ToInt64(value);
            string str = NumberConverter.ByteToFitString(num, 2, " bps", " kbps", " mbps", " gbps", " tbps");
            return str;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}