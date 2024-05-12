﻿using CommunityToolkit.Mvvm.Messaging;
using FzLib.WPF;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using ModernWpf.FzExtension.CommonDialog;
using SimpleFFmpegGUI.Manager;
using SimpleFFmpegGUI.Model;
using SimpleFFmpegGUI.WPF.Converters;
using SimpleFFmpegGUI.WPF.Messages;
using SimpleFFmpegGUI.WPF.Model;
using SimpleFFmpegGUI.WPF.Pages;
using SimpleFFmpegGUI.WPF.Panels;
using SimpleFFmpegGUI.WPF.ViewModels;
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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CommonDialog = ModernWpf.FzExtension.CommonDialog.CommonDialog;
using Task = System.Threading.Tasks.Task;

namespace SimpleFFmpegGUI.WPF
{
    public partial class MainWindow : Window
    {
        private readonly QueueManager queue;
        private bool hasShownTrayMessage = false;
        private StatusPanel statusPanel;
        private TaskList taskPanel;
        private FzLib.Program.Runtime.TrayIcon tray;
        public MainWindow(QueueManager queue)
        {
            if (Config.Instance.WindowMaximum)
            {
                WindowState = WindowState.Maximized;
            }
            ViewModel = this.SetDataContext<MainWindowViewModel>();
            InitializeComponent();
            RegisterMessages();
            this.queue = queue;
        }

        public event EventHandler UiCompressModeChanged;

        public bool IsUiCompressMode { get; private set; }

        public MainWindowViewModel ViewModel { get; set; }

        /// <summary>
        /// 新增一个Tab项
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="title"></param>
        /// <param name="beforeLoad"></param>
        public T AddNewTab<T>(string title = null) where T : UserControl
        {
            return AddNewTab(typeof(T), title) as T;
        }

        /// <summary>
        /// 新增一个Tab项
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="title"></param>
        /// <param name="beforeLoad"></param>
        public object AddNewTab(Type type, string title = null)
        {
            title ??= PageHelper.GetTitle(type);
            object page = App.ServiceProvider.GetService(type);
            //beforeLoad?.Invoke(panel);
            var tabItem = new TabItem() { Header = title, Content = page };
            tab.Items.Add(tabItem);
            tab.SelectedIndex = tab.Items.Count - 1;
            return page;
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
            if(beforeLoad==null)
            {
                return await ShowTopTabAsync(typeof(T)) as T;
            }
            else
            {
                return await ShowTopTabAsync(typeof(T),o=>beforeLoad(o as T)) as T;
            }
        }  

        public async Task<object> ShowTopTabAsync(Type type, Func<object, Task> beforeLoad = null) 
        {
            grdLeft.IsEnabled = false;
            ICloseablePage panel = App.ServiceProvider.GetService(type) as ICloseablePage;
            if (beforeLoad != null)
            {
                await beforeLoad(panel);
            }
            topTab.Content = panel;
            ViewModel.SetTabVisiable(false);
            ResetUI();
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            panel.RequestToClose += (s, e) =>
            {
                topTab.Content = null;
                ResetUI();
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
                if (!hasShownTrayMessage)
                {
                    hasShownTrayMessage = true;
                    tray.ShowMessage("任务将在后台继续执行");
                }
            }
        }

        protected override async void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            await Task.Yield();
            string[] files =
            [
                "ffmpeg.exe",
                "ffprobe.exe",
                "ffplay.exe",
                "mediainfo.exe"
            ];

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
            if (e.GetPosition(this).X > (Content as Grid).ColumnDefinitions[0].ActualWidth && tab.SelectedIndex > 0)
            {
                return;
            }
            IEnumerable<string> files = e.Data.GetData(DataFormats.FileDrop) as string[];
            files = files.Where(File.Exists);
            if (files.Any())
            {
                List<SelectDialogItem> items = Enum
                    .GetValues<TaskType>()
                    .Select(p => new SelectDialogItem(AttributeHelper.GetAttributeValue<NameDescriptionAttribute, string>(p, p => p.Name),
                        AttributeHelper.GetAttributeValue<NameDescriptionAttribute, string>(p, p => p.Description)))
                    .ToList();
                items.Add(new SelectDialogItem("查询信息", "查看媒体的元数据信息"));
                int typeCount = Enum.GetValues(typeof(TaskType)).Length;
                Activate();
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

        private void RegisterMessages()
        {
            WeakReferenceMessenger.Default.Register<FileDialogMessage>(this, (_, m) =>
            {
                switch (m.Dialog)
                {
                    case OpenFileDialog ofd:
                        m.Result = ofd.ShowDialog(this);
                        break;
                    case SaveFileDialog sfd:
                        m.Result = sfd.ShowDialog(this);
                        break;
                    case OpenFolderDialog ofod:
                        m.Result = ofod.ShowDialog(this);
                        break;
                    default:
                        break;
                }
            });

            WeakReferenceMessenger.Default.Register<WindowHandleMessage>(this, (_, m) =>
            {
                m.Handle = new WindowInteropHelper(this).Handle;
            });

            WeakReferenceMessenger.Default.Register<WindowEnableMessage>(this, (_, m) =>
            {
                if (m.IsEnabled)
                {
                    ring.Hide();
                }
                else
                {
                    ring.Show();
                }
            });


            WeakReferenceMessenger.Default.Register<AddNewTabMessage>(this, (_, m) =>
            {
                if(m.Top)
                {
                    m.Page = ShowTopTabAsync(m.Type);
                }
                else
                {
                    m.Page = AddNewTab(m.Type);
                }
            });

            WeakReferenceMessenger.Default.Register<ShowCodeArgumentsMessage>(this, (_, m) =>
            {
                var task = m.Task;
                Debug.Assert(task != null);
                var panel = new CodeArgumentsPanel
                {
                    IsHitTestVisible = false
                };
                panel.ViewModel.Update(task.Type, task.Arguments);
                ScrollViewer scr = new ScrollViewer();
                scr.Content = panel;
                Window win = new Window()
                {
                    Owner = this.GetWindow(),
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Content = scr,
                    Width = 600,
                    Height = 800,
                    Title = "详细参数 - FFmpeg工具箱"
                };
                win.Show();
            });

            WeakReferenceMessenger.Default.Register<QueueMessagesMessage>(this, (_, m) =>
            {
                switch (m.Type)
                {
                    case 'S':
                        this.CreateMessage().QueueSuccess(m.Message);
                        break;
                    case 'E' when m.Exception == null:
                        this.CreateMessage().QueueError(m.Message);
                        break;
                    case 'E' when m.Exception != null:
                        this.CreateMessage().QueueError(m.Message, m.Exception);
                        break;
                    default:
                        break;
                }
            });
        }

        private void ResetUI(bool force = false)
        {
            if (tab.SelectedIndex == 0 && !topTab.HasContent
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
            statusPanel.Margin = new Thickness(12, IsUiCompressMode ? 12 : 44, 12, IsUiCompressMode ? 12 : 42);
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

        private void Tab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                ResetUI();
            }
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            taskPanel = new TaskList() { Margin = new Thickness(8, 0, 0, 0) };
            statusPanel = new StatusPanel() { Margin = new Thickness(12) };
            ResetUI(true);
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            Config.Instance.WindowMaximum = WindowState == WindowState.Maximized;
        }
    }
}