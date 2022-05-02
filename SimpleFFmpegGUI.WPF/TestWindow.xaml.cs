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
using Path = System.IO.Path;

namespace SimpleFFmpegGUI.WPF
{
    public class TestWindowViewModel : INotifyPropertyChanged
    {
        public TestWindowViewModel()
        {
        }

        public PerformanceTestLine[] Tests { get; } = new PerformanceTestLine[]
        {
            new PerformanceTestLine(){Header="720P"},
            new PerformanceTestLine(){Header="1080P"},
            new PerformanceTestLine(){Header="1440P"},
            new PerformanceTestLine(){Header="2160P"},
        };
        private string message = "";
        public string Message
        {
            get => message;
            set => this.SetValueAndNotify(ref message, value, nameof(Message));
        }
        private bool isTesting = false;
        public bool IsTesting
        {
            get => isTesting;
            set => this.SetValueAndNotify(ref isTesting, value, nameof(IsTesting));
        }

        private double progress = 0;
        public double Progress
        {
            get => progress;
            set => this.SetValueAndNotify(ref progress, value, nameof(Progress));
        }
        private int preset = 4;
        public int Preset
        {
            get => preset;
            set => this.SetValueAndNotify(ref preset, value, nameof(Preset));
        }



        public event PropertyChangedEventHandler PropertyChanged;
    }

    public partial class TestWindow : Window
    {
        private bool stopping = false;
        private FFmpegManager runningFFmpeg = null;

        public TestWindowViewModel ViewModel { get; set; }

        public TestWindow(TestWindowViewModel viewModel, QueueManager queue)
        {
            ViewModel = viewModel;
            DataContext = ViewModel;
            InitializeComponent();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            IsEnabled = false;
            stopping = true;
            runningFFmpeg?.Cancel();
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                stopping = false;
                ViewModel.Progress = 0;
                ViewModel.IsTesting = true;
                if (!Directory.Exists("test"))
                {
                    Directory.CreateDirectory("test");
                }
                string input = Path.GetFullPath(Config.Instance.PerformanceTestFileName);
                if (!File.Exists(input))
                {
                    throw new FileNotFoundException("不存在测试文件");
                }
                await TestAsync(input);
            }
            catch (Exception ex)
            {
                if (stopping)
                {
                    ViewModel.Message = "测试终止";
                }
                else
                {
                    await CommonDialog.ShowErrorDialogAsync(ex);
                    ViewModel.Message = ex.Message;
                }
            }
            finally
            {
                ViewModel.IsTesting = false;
                IsEnabled = true;
            }
        }

        private async Task TestAsync(string input)
        {
            double sum = 0;
            string[] codes = new string[] { "H264", "H265", "VP9" };
            string[] sizes = new string[] { "1280x720", "1920x1080", "2560x1440", "3840x2160" };
            string[] formats = new string[] { "mp4", "mp4", "webm" };
            TaskInfo task = new TaskInfo()
            {
                Inputs = new List<InputArguments>() { new InputArguments() { FilePath = input } },
                Output = Path.GetFullPath("test/temp"),
                Arguments = new OutputArguments()
                {
                    Video = new VideoCodeArguments() { Preset = ViewModel.Preset },
                    Audio = new AudioCodeArguments()
                },
                Type = TaskType.Code
            };
            for (int i = 0; i < codes.Length; i++)
            {
                for (int j = 0; j < sizes.Length; j++)
                {
                    ViewModel.Message = $"正在测试{codes[i]}，{sizes[j].Split('x')[1]}P";
                    ViewModel.Progress += 0.5;
                    task.Arguments.Video.Code = codes[i];
                    task.Arguments.Video.Size = sizes[j];
                    task.Arguments.Format = formats[i];

                    runningFFmpeg = new FFmpegManager(task);
                    double fps = 0;
                    runningFFmpeg.StatusChanged += (s, e) =>
                    {
                        var status = runningFFmpeg.GetStatus();
                        if (status.HasDetail && status.Fps > 0)
                        {
                            fps = status.Fps;
                        }
                    };
                    try
                    {
                        await runningFFmpeg.RunAsync();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("编码失败", ex);
                    }
                    finally
                    {
                        runningFFmpeg = null;
                    }
                    ViewModel.Progress += 0.5;
                    if (fps <= 0)
                    {
                        throw new Exception("无法获取编码帧速度");
                    }
                    sum += fps;
                    ViewModel.Tests[j].Items[i].Score = fps;
                    if (stopping)
                    {
                        return;
                    }
                }
            }
            Directory.Delete("test", true);
            ViewModel.Message = $"总得分：{sum:0.00}";
        }
    }
}