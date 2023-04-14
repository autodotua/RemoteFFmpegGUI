using FzLib;
using FzLib.WPF;
using Microsoft.Extensions.DependencyInjection;
using ModernWpf.FzExtension.CommonDialog;
using SimpleFFmpegGUI.Manager;
using SimpleFFmpegGUI.Model;
using SimpleFFmpegGUI.WPF.Converters;
using SimpleFFmpegGUI.WPF.Model;
using SimpleFFmpegGUI.WPF.Pages;
using SimpleFFmpegGUI.WPF.Panels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
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
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;
using Task = System.Threading.Tasks.Task;

namespace SimpleFFmpegGUI.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly QueueManager queue;
        private TaskList taskPanel;
        private StatusPanel statusPanel;
        private FzLib.Program.Runtime.TrayIcon tray;
        public MainWindow(MainWindowViewModel viewModel, QueueManager queue)
        {
            if (Config.Instance.WindowMaximum)
            {
                WindowState = WindowState.Maximized;
            }
            ViewModel = viewModel;
            DataContext = ViewModel;
            InitializeComponent();
            this.queue = queue;
        }

        public MainWindowViewModel ViewModel { get; set; }

        /// <summary>
        /// 新增一个Tab项
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="title"></param>
        /// <param name="beforeLoad"></param>
        public T AddNewTab<T>(string title = null/*, Action<T> beforeLoad = null*/) where T : UserControl
        {
            title ??= PageHelper.GetTitle<T>();
            T panel = App.ServiceProvider.GetService<T>();
            //beforeLoad?.Invoke(panel);
            var tabItem = new TabItem() { Header = title, Content = panel };
            tab.Items.Add(tabItem);
            tab.SelectedIndex = tab.Items.Count - 1;
            return panel;
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

        /// <summary>
        /// 显示顶级对话框级别的页面，并等待其关闭
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="beforeLoad"></param>
        /// <returns></returns>
        public async Task<T> ShowTopTabAsync<T>(Func<T, Task> beforeLoad = null) where T : UserControl, ICloseablePage
        {
            grdLeft.IsEnabled = false;
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
                grdLeft.IsEnabled = true;
            };
            return await tcs.Task;
        }

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

        protected override async void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            await Task.Yield();
            string[] files = new[]
            {
                "ffmpeg.exe",
                "ffprobe.exe",
                "ffplay.exe",
                "mediainfo.exe"
            };

            foreach (var file in files)
            {
                if (!File.Exists(file))
                {
                    await CommonDialog.ShowErrorDialogAsync("程序目录中缺少文件：" + file);
                    Close();
                    return;
                }
            }
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
                    .Select(p => new SelectDialogItem(AttributeHelper.GetAttributeValue<NameDescriptionAttribute, string>(p, p => p.Name),
                        AttributeHelper.GetAttributeValue<NameDescriptionAttribute, string>(p, p => p.Description)))
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
                    var panel = AddNewTab<AddTaskPage>();
                    panel.SetFiles(files, (TaskType)index);
                }
                else if (index == typeCount)
                {
                    AddNewTab<MediaInfoPage>().SetFile(files.First());
                }
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddNewTab<AddTaskPage>();
        }

        private async void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (!await CommonDialog.ShowYesNoDialogAsync("终止队列", "是否终止队列？"))
            {
                return;
            }
            try
            {
                IsEnabled = false;
                await queue.CancelAsync();
            }
            catch (Exception ex)
            {
                this.CreateMessage().QueueError("终止队列失败", ex);
            }
            finally
            {
                IsEnabled = true;
            }
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

        private void FFmpegOutputButton_Click(object sender, RoutedEventArgs e)
        {
            AddNewTab<FFmpegOutputPage>();
        }

        private void LogsButton_Click(object sender, RoutedEventArgs e)
        {
            AddNewTab<LogsPage>();
        }

        private void MediaInfoButton_Click(object sender, RoutedEventArgs e)
        {
            AddNewTab<MediaInfoPage>();
        }

        private void PresetsButton_Click(object sender, RoutedEventArgs e)
        {
            AddNewTab<PresetsPage>();
        }

        private async void SettingButton_Click(object sender, RoutedEventArgs e)
        {
            await ShowTopTabAsync<SettingPage>();
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

        private void TasksButton_Click(object sender, RoutedEventArgs e)
        {
            AddNewTab<TasksPage>();
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            var window = App.ServiceProvider.GetService<TestWindow>();
            window.Owner = this;
            window.Show();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Config.Instance.Save();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            Config.Instance.WindowMaximum = WindowState == WindowState.Maximized;
        }

        public bool IsUiCompressMode { get; private set; }

        private void ResetUI(bool force = false)
        {
            if (tab.Items.Count == 0 && topTab.Content == null
                && (IsUiCompressMode || force)) //左侧和右侧
            {
                RemoveFromGrid();
                grdLeft.Children.Add(taskPanel);
                grdMain.Children.Add(statusPanel);
                Grid.SetColumn(statusPanel, 2);
                IsUiCompressMode = false;
            }
            else if (!IsUiCompressMode || force)//全部在左侧
            {
                RemoveFromGrid();
                grdLeft.Children.Add(taskPanel);
                grdLeft.Children.Add(statusPanel);
                Grid.SetRow(statusPanel, 2);
                IsUiCompressMode = true;
            }

            grdLeft.RowDefinitions[2].Height = new GridLength(IsUiCompressMode ? 384 : 0);
            grdLeft.RowDefinitions[2].MinHeight = IsUiCompressMode ? 384 : 0;
            leftSplitter.Visibility = IsUiCompressMode ? Visibility.Visible : Visibility.Collapsed;
            UiCompressModeChanged?.Invoke(this, new EventArgs());
            void RemoveFromGrid()
            {
                if (taskPanel.Parent != null)
                {
                    (taskPanel.Parent as Grid).Children.Remove(taskPanel);
                }
                if (statusPanel.Parent != null)
                {
                    (statusPanel.Parent as Grid).Children.Remove(statusPanel);
                }
            }
        }

        public event EventHandler UiCompressModeChanged;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            taskPanel = new TaskList() { Margin = new Thickness(8, 0, 0, 0) };
            statusPanel = new StatusPanel() { Margin = new Thickness(12) };
            ResetUI(true);
        }

        private void Tab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ResetUI();
        }
    }

    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public QueueManager queue;

        private bool isTabControlVisiable = true;

        public MainWindowViewModel(QueueManager queue)
        {
            this.queue = queue;
            queue.TaskManagersChanged += (s, e) => this.Notify(nameof(StartMainQueueButtonVisibility), nameof(StopMainQueueButtonVisibility));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Visibility StartMainQueueButtonVisibility => queue.MainQueueTask == null ? Visibility.Visible : Visibility.Collapsed;
        public Visibility StopMainQueueButtonVisibility => queue.MainQueueTask == null ? Visibility.Collapsed : Visibility.Visible;
        public Visibility TabControlVisibility => isTabControlVisiable ? Visibility.Visible : Visibility.Collapsed;
        public Visibility TopTabVisibility => isTabControlVisiable ? Visibility.Collapsed : Visibility.Visible;

        public void SetTabVisiable(bool isTabControlVisiable)
        {
            this.isTabControlVisiable = isTabControlVisiable;
            this.Notify(nameof(TabControlVisibility), nameof(TopTabVisibility));
        }
    }
}