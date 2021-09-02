using CefSharp;
using CefSharp.WinForms;
using FzLib.IO;
using SimpleFFmpegGUI.Manager;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimpleFFmpegGUI.WebApp
{
    public partial class MainForm : Form
    {
        private CefSharp.WinForms.ChromiumWebBrowser browser;

        public MainForm()
        {
            InitializeComponent();
        }

        public static int GetFreePort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }

        private void EnsureFiles()
        {
            if (!Directory.Exists("html"))
            {
                if (Directory.Exists("../../../../../SimpleFFmpegGUI.Web/dist"))
                {
                    FileSystem.CopyDirectory("../../../../../SimpleFFmpegGUI.Web/dist", "html");
                }
                else if (Directory.Exists("../../../../SimpleFFmpegGUI.Web/dist"))
                {
                    FileSystem.CopyDirectory("../../../../SimpleFFmpegGUI.Web/dist", "html");
                }
                else if (Directory.Exists("../../../../../../SimpleFFmpegGUI.Web/dist"))
                {
                    FileSystem.CopyDirectory("../../../../../../SimpleFFmpegGUI.Web/dist", "html");
                }
                else
                {
                    throw new Exception("未包含网页文件，且找不到生成的位置");
                }
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            int port = 0;
            try
            {
                EnsureFiles();
                port = GetFreePort();
                foreach (var file in Directory.EnumerateFiles("html/js", "app*.js"))
                {
                    string content = File.ReadAllText(file).Replace("/api/", "/");
                    File.WriteAllText(file, content);
                }
                string pipeName = Guid.NewGuid().ToString();
                new Thread(() => WebAPI.Program.Main(port, pipeName)).Start();
                new Thread(() => SimpleFFmpegGUI.Program.Main(new[] { "-p", pipeName })).Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "加载失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-1);
                return;
            }
            CefSettings settingsBrowser = new CefSettings();
            settingsBrowser.Locale = "zh-hans";

            Cef.Initialize(settingsBrowser);
            browser = new CefSharp.WinForms.ChromiumWebBrowser($"http://localhost:{port}/index.html")
            {
                Dock = DockStyle.Fill
            };

            void Loaded(object sender, LoadingStateChangedEventArgs e)
            {
                if (e.IsLoading == false)
                {
                    lblLoading.Invoke((Action)(() => { lblLoading.Visible = false; }));
                    browser.LoadingStateChanged -= Loaded;
                }
            };
            browser.LoadingStateChanged += Loaded;
            browser.KeyboardHandler = new KeyboardHandler();
            Controls.Add(browser);
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (PipeService.Manager.Tasks.Any())
            {
                MessageBox.Show("请先停止任务，然后才能关闭程序", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                e.Cancel = true;
            }
        }
    }

    public class KeyboardHandler : IKeyboardHandler
    {
        private DateTime last = DateTime.MinValue;

        public bool OnKeyEvent(IWebBrowser browserControl, IBrowser browser, KeyType type, int windowsKeyCode, int nativeKeyCode, CefEventFlags modifiers, bool isSystemKey)
        {
            bool result = false;
            if (windowsKeyCode == (int)Keys.F12 && (DateTime.Now - last).Seconds > 0.2)
            {
                last = DateTime.Now;
                browser.ShowDevTools();
            }
            return result;
        }

        public bool OnPreKeyEvent(IWebBrowser browserControl, IBrowser browser, KeyType type, int windowsKeyCode, int nativeKeyCode, CefEventFlags modifiers, bool isSystemKey, ref bool isKeyboardShortcut)
        {
            const int WM_SYSKEYDOWN = 0x104;
            const int WM_KEYDOWN = 0x100;
            const int WM_KEYUP = 0x101;
            const int WM_SYSKEYUP = 0x105;
            const int WM_CHAR = 0x102;
            const int WM_SYSCHAR = 0x106;
            const int VK_TAB = 0x9;

            bool result = false;

            isKeyboardShortcut = false;

            if (windowsKeyCode == VK_TAB)
            {
                return result;
            }

            Control control = browserControl as Control;
            int msgType = 0;
            switch (type)
            {
                case KeyType.RawKeyDown:
                    if (isSystemKey)
                    {
                        msgType = WM_SYSKEYDOWN;
                    }
                    else
                    {
                        msgType = WM_KEYDOWN;
                    }
                    break;

                case KeyType.KeyUp:
                    if (isSystemKey)
                    {
                        msgType = WM_SYSKEYUP;
                    }
                    else
                    {
                        msgType = WM_KEYUP;
                    }
                    break;

                case KeyType.Char:
                    if (isSystemKey)
                    {
                        msgType = WM_SYSCHAR;
                    }
                    else
                    {
                        msgType = WM_CHAR;
                    }
                    break;

                default:
                    Trace.Assert(false);
                    break;
            }
            PreProcessControlState state = PreProcessControlState.MessageNotNeeded;
            control.Invoke(new Action(() =>
            {
                Message msg = new Message() { HWnd = control.Handle, Msg = msgType, WParam = new IntPtr(windowsKeyCode), LParam = new IntPtr(nativeKeyCode) };

                bool processed = Application.FilterMessage(ref msg);
                if (processed)
                {
                    state = PreProcessControlState.MessageProcessed;
                }
                else
                {
                    state = control.PreProcessControlMessage(ref msg);
                }
            }));
            if (state == PreProcessControlState.MessageNeeded)
            {
                isKeyboardShortcut = true;
            }
            else if (state == PreProcessControlState.MessageProcessed)
            {
                // Most of the interesting cases get processed by PreProcessControlMessage.
                result = true;
            }
            return result;
        }
    }
}