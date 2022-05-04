using FzLib;
using ModernWpf.FzExtension.CommonDialog;
using SimpleFFmpegGUI.Manager;
using SimpleFFmpegGUI.Model;
using SimpleFFmpegGUI.WPF.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using static SimpleFFmpegGUI.WPF.Model.PerformanceTestLine;

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
            new PerformanceTestLine(){Header="总得分"},
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
        private double detailProgress = 0;
        public double DetailProgress
        {
            get => detailProgress;
            set => this.SetValueAndNotify(ref detailProgress, value, nameof(DetailProgress));
        }
        private int preset = 3;
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

        public TestWindow(TestWindowViewModel viewModel)
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
                ViewModel.Tests[^1].Sizes.ForEach(p => p.Score = 0);
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
                Directory.Delete("test", true);
                ViewModel.IsTesting = false;
                ViewModel.DetailProgress = 1;
                IsEnabled = true;
            }
        }

        /// <summary>
        /// 测试核心代码
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task TestAsync(string input)
        {
            double[] sums = new double[CodecsCount];
            int[] counts = new int[CodecsCount];
            string[] codecs = new string[CodecsCount] { "H264", "H265", "VP9" };
            string[] sizes = new string[SizesCount] { "1280x720", "1920x1080", "2560x1440", "3840x2160" };
            //不同分辨率不同编码的权重矩阵，即一般情况下不同分辨率的编码帧速率与720P的帧速率之比
            double[,] weights = new[,] {
                { 1, 0.64, 0.4, 0.2 },
                { 1, 0.6, 0.35, 0.16 },
                { 1, 0.6, 0.4, 0.25 } };

            var task = new TaskInfo()
            {
                Inputs = new List<InputArguments>()
                {
                    new InputArguments()
                    {
                        FilePath = input,
                    }
                },
                Output = Path.GetFullPath("test/temp"),
                Arguments = new OutputArguments()
                {
                    Video = new VideoCodeArguments() { Preset = ViewModel.Preset },
                    Audio = null,
                    DisableAudio = true,
                    Format = "mp4"
                },
                Type = TaskType.Code,
            };
            for (int i = 0; i < codecs.Length; i++)
            {
                for (int j = 0; j < sizes.Length; j++)
                {
                    var item = ViewModel.Tests[j].Sizes[i];

                    ViewModel.Message = $"正在测试{codecs[i]}，{sizes[j].Split('x')[1]}P";
                    ViewModel.Progress += 0.5;
                    ViewModel.DetailProgress = 0;
                    task.Arguments.Video.Code = codecs[i];
                    task.Arguments.Video.Size = sizes[j];

                    runningFFmpeg = new FFmpegManager(task);
                    double fps = 0;
                    runningFFmpeg.StatusChanged += (s, e) =>
                    {
                        var status = runningFFmpeg.GetStatus();
                        if (status.HasDetail && status.Fps > 0)
                        {
                            fps = status.Fps;
                        }
                        if (status.HasDetail && status.Progress != null && status.Progress.Percent is >= 0 and <= 1)
                        {
                            ViewModel.DetailProgress = status.Progress.Percent;
                        }
                    };
                    try
                    {
                        await runningFFmpeg.RunAsync();
                    }
                    catch (TaskCanceledException)
                    {
                        goto end;
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
                    sums[i] += fps / weights[i, j];
                    counts[i]++;
                    item.Score = fps;
                    if (stopping)
                    {
                        return;
                    }
                }
            }
        end:
            for (int i = 0; i < CodecsCount; i++)
            {
                ViewModel.Tests[^1].Sizes[i].Score = counts[i] == 0 ? 0 : Math.Round(sums[i] / counts[i], 2);
            }
            ViewModel.Message = "";
        }
    }
}