using CefSharp;
using FzLib.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
            browser = new CefSharp.WinForms.ChromiumWebBrowser
            {
                Dock = DockStyle.Fill
            };

            Controls.Add(browser);
            lblLoading.BringToFront();
            menu.BringToFront();
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
                    MessageBox.Show("未包含网页文件，且找不到生成的位置");
                    Environment.Exit(-1);
                    return;
                }
            }
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            EnsureFiles();
            int port = GetFreePort();
            foreach (var file in Directory.EnumerateFiles("html/js", "app*.js"))
            {
                string content = File.ReadAllText(file).Replace("/api/", "/");
                File.WriteAllText(file, content);
            }
            string pipeName = Guid.NewGuid().ToString();
            new Thread(() => WebAPI.Program.Main(port, pipeName)).Start();
            new Thread(() => SimpleFFmpegGUI.Program.Main(new[] { "-p", pipeName })).Start();

            await browser.LoadUrlAsync($"http://localhost:{port}/index.html");
            lblLoading.Visible = false;
        }

        private void 打开开发人员面板ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            browser.ShowDevTools();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}