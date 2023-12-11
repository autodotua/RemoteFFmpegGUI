using Enterwell.Clients.Wpf.Notifications;
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
using System.Collections.ObjectModel;
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
using System.Windows.Threading;

namespace SimpleFFmpegGUI.WPF.Pages
{
    public class FFmpegOutputPageViewModel
    {
        public FFmpegOutputPageViewModel()
        {
            Logger.Log += Logger_Log;
        }
        private async void Logger_Log(object sender, LogEventArgs e)
        {
            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                if (e.Log.Message.StartsWith("frame=")
                && Outputs.Count > 0 && Outputs[^1].Message.StartsWith("frame="))
                {
                    Outputs[^1] = e.Log;
                }
                else
                {
                    Outputs.Add(e.Log);
                }
            });
        }

        public ObservableCollection<Log> Outputs { get; } = new ObservableCollection<Log>();
    }

    /// <summary>
    /// Interaction logic for FFmpegOutputPage.xaml
    /// </summary>
    public partial class FFmpegOutputPage : UserControl
    {
        public FFmpegOutputPage(FFmpegOutputPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = ViewModel;
            InitializeComponent();
            ViewModel.Outputs.CollectionChanged += Outputs_CollectionChanged;
        }

        private void Outputs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                scr.ScrollToEnd();
            }
        }

        private async void Logger_Log(object sender, LogEventArgs e)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                if (e.Log.Message.StartsWith("frame=")
                && ViewModel.Outputs.Count > 0 && ViewModel.Outputs[^1].Message.StartsWith("frame="))
                {
                    ViewModel.Outputs[^1] = e.Log;
                }
                else
                {
                    bool end = scr.VerticalOffset == scr.ScrollableHeight;
                    ViewModel.Outputs.Add(e.Log);
                    if (end)
                    {
                        scr.ScrollToEnd();
                    }
                }
            });
        }

        public FFmpegOutputPageViewModel ViewModel { get; set; }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.GetWindow().Closing += (s, e) => Logger.Log -= Logger_Log;
        }

        private void ClearAllButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Outputs.Clear();
        }

        private void CopyAllButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(string.Join(Environment.NewLine,ViewModel.Outputs.Select(p=>p.Message)));
                this.CreateMessage().QueueSuccess("已复制内容到剪贴板");
            }
            catch (Exception ex)
            {
                this.CreateMessage().QueueError("复制内容失败", ex);
            }
        }

        private void TextBlock_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Clipboard.SetText((sender as TextBlock).Text);
                this.CreateMessage().QueueSuccess("已复制内容到剪贴板");
            }
            catch(Exception ex)
            {
                this.CreateMessage().QueueError("复制内容失败", ex);
            }
        }
    }
}