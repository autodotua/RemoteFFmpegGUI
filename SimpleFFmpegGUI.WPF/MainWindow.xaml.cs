using FzLib;
using Microsoft.Extensions.DependencyInjection;
using ModernWpf.FzExtension.CommonDialog;
using SimpleFFmpegGUI.Manager;
using SimpleFFmpegGUI.Model;
using SimpleFFmpegGUI.WPF.Model;
using SimpleFFmpegGUI.WPF.Pages;
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
        private bool isTabControlVisiable = true;
        public Visibility TabControlVisibility => isTabControlVisiable ? Visibility.Visible : Visibility.Collapsed;
        public Visibility TopTabVisibility => isTabControlVisiable ? Visibility.Collapsed : Visibility.Visible;

        public void SetTabVisiable(bool isTabControlVisiable)
        {
            this.isTabControlVisiable = isTabControlVisiable;
            this.Notify(nameof(TabControlVisibility), nameof(TopTabVisibility));
        }

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

        /// <summary>
        /// 新增一个Tab项
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="title"></param>
        /// <param name="beforeLoad"></param>
        public void AddNewTab<T>(string title = null, Action<T> beforeLoad = null) where T : UserControl
        {
            title = title ?? PageHelper.GetTitle<T>();
            T panel = App.ServiceProvider.GetService<T>();
            beforeLoad?.Invoke(panel);
            var tabItem = new TabItem() { Header = title, Content = panel };
            tab.Items.Add(tabItem);
            tab.SelectedIndex = tab.Items.Count - 1;
        }

        /// <summary>
        /// 显示顶级对话框级别的页面，并等待其关闭
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="beforeLoad"></param>
        /// <returns></returns>
        public async Task<T> ShowTopTabAsync<T>(Func<T, Task> beforeLoad = null) where T : UserControl, ICloseablePage
        {
            T panel = App.ServiceProvider.GetService<T>();
            if (beforeLoad != null)
            {
                await beforeLoad(panel);
            }
            topTab.Content = panel;
            ViewModel.SetTabVisiable(false);
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            panel.RequestToClose += (s, e) =>
            {
                topTab.Content = null;
                ViewModel.SetTabVisiable(true);
                tcs.SetResult(panel);
            };
            return await tcs.Task;
        }

        /// <summary>
        /// 移除一项Tab
        /// </summary>
        /// <param name="content">TabItem的内容，页面</param>
        /// <exception cref="ArgumentException"></exception>
        public void RemoveTab(object content)
        {
            var tabItem = tab.Items.Cast<TabItem>().Where(p => p.Content == content).FirstOrDefault();
            if (tabItem != null)
            {
                tab.Items.Remove(tabItem);
                return;
            }
            throw new ArgumentException();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddNewTab<AddTaskPage>();
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
            AddNewTab<LogsPage>();
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            base.OnDragOver(e);
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Link;
            }
        }

        protected override async void OnDrop(DragEventArgs e)
        {
            base.OnDrop(e);
            if (e.GetPosition(this).X > (Content as Grid).ColumnDefinitions[0].ActualWidth && tab.SelectedIndex >= 0)
            {
                return;
            }
            IEnumerable<string> files = e.Data.GetData(DataFormats.FileDrop) as string[];
            files = files.Where(p => File.Exists(p));
            if (files.Any())
            {
                List<SelectDialogItem> items = Enum
                    .GetValues(typeof(TaskType))
                    .Cast<TaskType>()
                    .ToList()
                    .Select(p => new SelectDialogItem(FzLib.WPF.Converters.DescriptionConverter.GetDescription(p)))
                    .ToList();
                items.Add(new SelectDialogItem("查询信息", "查看媒体的元数据信息"));
                int typeCount = Enum.GetValues(typeof(TaskType)).Length;
                this.Activate();
                var index = await CommonDialog.ShowSelectItemDialogAsync("选择操作", items);
                if (index == -1)
                {
                    return;
                }
                if (index < typeCount)
                {
                    AddNewTab<AddTaskPage>(beforeLoad: p => p.SetFiles(files, (TaskType)index));
                }
                else if (index == typeCount)
                {
                    AddNewTab<MediaInfoPage>(beforeLoad: p => p.SetFile(files.First()));
                }
            }
        }

        protected override async void OnContentRendered(EventArgs e)
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
            AddNewTab<MediaInfoPage>();
        }

        private async void SettingButton_Click(object sender, RoutedEventArgs e)
        {
            await ShowTopTabAsync<SettingPage>();
        }

        private void TasksButton_Click(object sender, RoutedEventArgs e)
        {
            AddNewTab<TasksPage>();
        }

        private void PresetsButton_Click(object sender, RoutedEventArgs e)
        {
            AddNewTab<PresetsPage>();
        }

        private void CloseTabButton_Click(object sender, RoutedEventArgs e)
        {
            DependencyObject obj = e.Source as DependencyObject;
            while (obj != null)
            {
                obj = VisualTreeHelper.GetParent(obj);
                if (obj is TabItem item)
                {
                    tab.Items.Remove(item);
                }
            }
        }
    }
}