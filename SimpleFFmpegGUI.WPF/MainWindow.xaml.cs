using FzLib;
using Microsoft.Extensions.DependencyInjection;
using ModernWpf.FzExtension.CommonDialog;
using SimpleFFmpegGUI.Manager;
using SimpleFFmpegGUI.WPF.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public QueueManager queue;

        public MainWindowViewModel(QueueManager queue)
        {
            this.queue = queue;
            queue.TaskManagersChanged += (s, e) => this.Notify(nameof(StartMainQueueButtonVisibility), nameof(StopMainQueueButtonVisibility));
        }

        public Visibility StartMainQueueButtonVisibility => queue.MainQueueTask == null ? Visibility.Visible : Visibility.Collapsed;
        public Visibility StopMainQueueButtonVisibility => queue.MainQueueTask == null ? Visibility.Collapsed : Visibility.Visible;

        public event PropertyChangedEventHandler PropertyChanged;
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly QueueManager queue;

        public MainWindowViewModel ViewModel { get; set; }

        public MainWindow(MainWindowViewModel viewModel, QueueManager queue)
        {
            ViewModel = viewModel;
            DataContext = ViewModel;
            InitializeComponent();
            this.queue = queue;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            App.ServiceProvider.GetService<AddTaskWindow>().Show();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (!TaskManager.HasQueueTasks())
            {
                this.CreateMessage().QueueError("没有排队中的任务");
                return;
            }
            queue.StartQueue();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            queue.Cancel();
        }

        private FzLib.Program.Runtime.TrayIcon tray;

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if (queue.Managers.Any())
            {
                e.Cancel = true;
                if (tray == null)
                {
                    var bmp = Bitmap.FromFile("icon.png");
                    var thumb = (Bitmap)bmp.GetThumbnailImage(64, 64, null, IntPtr.Zero);
                    thumb.MakeTransparent();
                    var icon = System.Drawing.Icon.FromHandle(thumb.GetHicon());
                    tray = new FzLib.Program.Runtime.TrayIcon(icon, FzLib.Program.App.ProgramName);

                    tray.MouseLeftClick += (s, e) =>
                    {
                        Show();
                        tray.Hide();
                    };
                    tray.ReShowWhenDisplayChanged = true;
                    Closed += (s, e) =>
                    {
                        tray.Dispose();
                    };
                }
                tray.Show();
                Hide();
            }
        }

        private void LogsButton_Click(object sender, RoutedEventArgs e)
        {
            var win = App.ServiceProvider.GetService<LogsWindow>();
            win.Owner = this;
            win.Show();
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            base.OnDragOver(e);
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Link;
            }
        }

        protected override void OnDrop(DragEventArgs e)
        {
            base.OnDrop(e);
            IEnumerable<string> files = e.Data.GetData(DataFormats.FileDrop) as string[];
            files = files.Where(p => File.Exists(p));
            if (files.Any())
            {
                var dialog = App.ServiceProvider.GetService<AddTaskWindow>();
                dialog.SetFiles(files);
                dialog.Show();
            }
        }

        protected async override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            await Task.Yield();
            string[] files = new[]
            {
                "ffmpeg.exe",
                "ffprobe.exe",
                "ffplay.exe",
                "mediainfo.dll"
            };

            foreach (var file in files)
            {
                if (!File.Exists(file))
                {
                    await CommonDialog.ShowErrorDialogAsync("找不到" + file);
                    Close();
                    return;
                }
            }
        }

        private void MediaInfoButton_Click(object sender, RoutedEventArgs e)
        {
            App.ServiceProvider.GetService<MediaInfoWindow>().Show();
        }

        private void SettingButton_Click(object sender, RoutedEventArgs e)
        {
            App.ServiceProvider.GetService<SettingWindow>().ShowDialog(this);
        }

        private void TasksButton_Click(object sender, RoutedEventArgs e)
        {
            App.ServiceProvider.GetService<TasksWindow>().ShowDialog(this);
        }

        private void PresetsButton_Click(object sender, RoutedEventArgs e)
        {
            App.ServiceProvider.GetService<PresetsWindow>().ShowDialog(this);
        }
    }
}