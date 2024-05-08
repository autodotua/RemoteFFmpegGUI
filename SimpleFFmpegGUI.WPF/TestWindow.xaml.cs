using FzLib;
using FzLib.WPF;
using FzLib.WPF.Converters;
using Microsoft.Win32;
using ModernWpf.Controls;
using ModernWpf.FzExtension.CommonDialog;
using SimpleFFmpegGUI.FFmpegLib;
using SimpleFFmpegGUI.Manager;
using SimpleFFmpegGUI.Model;
using SimpleFFmpegGUI.WPF.Converters;
using SimpleFFmpegGUI.WPF.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using static SimpleFFmpegGUI.WPF.Model.PerformanceTestLine;
using static SimpleFFmpegGUI.WPF.TestWindowViewModel;
using static System.Net.Mime.MediaTypeNames;
using CommonDialog = ModernWpf.FzExtension.CommonDialog.CommonDialog;

namespace SimpleFFmpegGUI.WPF
{
    public partial class TestWindow : Window
    {
        private const string TestDir = "test";
        private readonly Logger logger;
        private FFmpegManager runningFFmpeg = null;
        private bool stopping = false;
        public TestWindow(TestWindowViewModel viewModel,Logger logger)
        {
            ViewModel = viewModel;
            this.logger = logger;
            DataContext = ViewModel;
            InitializeComponent();
            CreateDataGrids();
            CreateTestItemsDataGrid();
        }

        public TestWindowViewModel ViewModel { get; set; }

        private void BrowseTestVideoButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog().AddFilter("视频", "mp4", "mov", "mkv", "avi");
            string path = dialog.GetPath(this);
            if (!string.IsNullOrEmpty(path))
            {
                ViewModel.TestVideo = path;
            }
        }

        /// <summary>
        /// 动态创建表格
        /// </summary>
        private void CreateDataGrids()
        {
            grpSpeed.Content = GetDataGrid(nameof(PerformanceTestItem.FPS), "0.00");
            grpSSIM.Content = GetDataGrid(nameof(PerformanceTestItem.SSIM), "0.00%");
            grpPSNR.Content = GetDataGrid(nameof(PerformanceTestItem.PSNR), "0.00");
            grpVMAF.Content = GetDataGrid(nameof(PerformanceTestItem.VMAF), "0.00");
            grpCpuUsage.Content = GetDataGrid(nameof(PerformanceTestItem.CpuUsage), "0.00%");
            grpOutputSize.Content = GetDataGrid(nameof(PerformanceTestItem.OutputSize), "0.00");

            DataGrid GetDataGrid(string type, string format = null)
            {
                DataGrid dataGrid = new DataGrid()
                {
                    AutoGenerateColumns = false,
                    CanUserAddRows = false,
                    CanUserDeleteRows = false,
                    CanUserReorderColumns = false,
                    CanUserResizeColumns = false,
                    CanUserResizeRows = false,
                    CanUserSortColumns = false,
                    HeadersVisibility = DataGridHeadersVisibility.Column,
                    SelectionUnit = DataGridSelectionUnit.Cell,
                    IsReadOnly = true,
                    IsHitTestVisible = false,
                };
                dataGrid.SetBinding(DataGrid.IsEnabledProperty,
                    new Binding(nameof(TestWindowViewModel.IsTesting)) { Converter = new InverseBoolConverter() });
                dataGrid.SetBinding(DataGrid.ItemsSourceProperty,
                   new Binding(nameof(TestWindowViewModel.Tests)));
                DataGridTextColumn c = new DataGridTextColumn()
                {
                    Binding = new Binding(nameof(PerformanceTestLine.Header)),
                    IsReadOnly = true,
                };
                dataGrid.Columns.Add(c);
                for (int i = 0; i < CodecsCount; i++)
                {
                    c = new DataGridTextColumn()
                    {
                        Width = 108,
                        Header = ViewModel.Codecs[i].Name,
                        Binding = new Binding($"{nameof(PerformanceTestLine.Items)}[{i}].{type}")
                        {
                            Converter = new EmptyIfZeroConverter()
                        }
                    };
                    if (!string.IsNullOrEmpty(format))
                    {
                        c.Binding.StringFormat = format;
                    }
                    dataGrid.Columns.Add(c);
                }
                return dataGrid;
            }
        }

        /// <summary>
        /// 创建各分辨率的
        /// </summary>
        /// <param name="input"></param>
        /// <param name="sizes"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task CreateRefVideosAsync(string input, string[] sizes)
        {
            var task = new TaskInfo()
            {
                Inputs = new List<InputArguments>()
                {
                    new InputArguments()
                    {
                        FilePath = input,
                    }
                },
                Arguments = new OutputArguments()
                {
                    Video = new VideoCodeArguments()
                    {
                        Preset = 7,
                        Code = "H264",
                        Crf = 13,
                    },
                    Audio = null,
                    DisableAudio = true,
                    Format = "mp4",
                },
                Type = TaskType.Code,
            };
            for (int i = 0; i < sizes.Length; i++)
            {
                var size = sizes[i];

                if (!ViewModel.Tests[i].Items.Any(p => p.IsChecked))
                {
                    continue;
                }
                ViewModel.Message = $"正在准备{size.Split(':')[1]}P素材";
                task.Arguments.Video.Size = size;
                task.Output = Path.GetFullPath($"{TestDir}/{size.Split(':')[1]}P.mp4");

                if (File.Exists(task.Output))
                {
                    continue;
                }
                runningFFmpeg = new FFmpegManager(task);
                runningFFmpeg.StatusChanged += (s, e) =>
                {
                    var status = runningFFmpeg.GetStatus();
                    if (status.HasDetail && status.Progress != null && status.Progress.Percent is >= 0 and <= 1)
                    {
                        ViewModel.DetailProgress = status.Progress.Percent;
                    }
                };
                try
                {
                    await runningFFmpeg.RunAsync();
                }
                catch (TaskCanceledException ex)
                {
                    break;
                }
                catch (Exception ex)
                {
                    throw new Exception("编码失败", ex);
                }
                finally
                {
                    runningFFmpeg = null;
                }
            }
        }

        private void CreateTestItemsDataGrid()
        {
            DataGrid dataGrid = new DataGrid()
            {
                AutoGenerateColumns = false,
                CanUserAddRows = false,
                CanUserDeleteRows = false,
                CanUserReorderColumns = false,
                CanUserResizeColumns = false,
                CanUserResizeRows = false,
                CanUserSortColumns = false,
                HeadersVisibility = DataGridHeadersVisibility.Column,
                SelectionUnit = DataGridSelectionUnit.Cell,
            };
            dataGrid.SetBinding(DataGrid.IsEnabledProperty,
                new Binding(nameof(TestWindowViewModel.IsTesting)) { Converter = new InverseBoolConverter() });
            dataGrid.SetBinding(DataGrid.ItemsSourceProperty,
               new Binding(nameof(TestWindowViewModel.Tests)));
            var c = new DataGridTextColumn()
            {
                Binding = new Binding(nameof(PerformanceTestLine.Header)),
                IsReadOnly = true,
            };
            dataGrid.Columns.Add(c);
            for (int i = 0; i < CodecsCount; i++)
            {
                var codec = VideoCodec.VideoCodecs[i];
                var tc = new DataGridTemplateColumn
                {
                    Width = 108,
                    Header = codec.Name,
                    CellTemplate = new DataTemplate()
                };
                FrameworkElementFactory factory = new FrameworkElementFactory(typeof(CheckBox));
                factory.SetValue(MarginProperty, new Thickness(16, 0, 0, 0));
                factory.SetBinding(CheckBox.IsCheckedProperty, new Binding($"{nameof(PerformanceTestLine.Items)}[{i}].{nameof(PerformanceTestItem.IsChecked)}")
                {
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });
                tc.CellTemplate.VisualTree = factory;
                dataGrid.Columns.Add(tc);
            }
            c = new DataGridTextColumn()
            {
                Width = 108,
                Binding = new Binding(nameof(PerformanceTestLine.MBitrate)),
                Header = "码率 (Mbps)"
            };
            dataGrid.Columns.Add(c);
            grpTestItems.Content = dataGrid;
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog().AddFilter("CSV表格", "csv");
            dialog.FileName = "编码测试结果.csv";
            string path = dialog.GetPath(this);
            if (path == null)
            {
                return;
            }
            try
            {

                StringBuilder str = new StringBuilder();
                if (!File.Exists(path))
                {
                    str.AppendLine("Codec,Resolution,Mbitrate,BitrateFactor,CRF,Speed,Extra,FPS,VMAF,SSIM,PSNR,CPU,Duration,OutputSize,Video");
                }
                foreach (var test in ViewModel.Tests)
                {
                    foreach (var item in test.Items.Where(p => p.FPS > 0))
                    {
                        var codec = ViewModel.Codecs.First(p => p.Name == item.Codec);
                        str.Append(item.Codec ?? "")
                            .Append(',')
                            .Append(test.Header)
                            .Append(',')
                            .Append(ViewModel.QCMode == 0 ? test.MBitrate : "")
                            .Append(',')
                            .Append(ViewModel.QCMode == 0 ? codec.BitrateFactor : "")
                            .Append(',')
                            .Append(ViewModel.QCMode == 1 ? codec.CRF : "")
                            .Append(',')
                            .Append(codec.CpuSpeed)
                            .Append(',')
                            .Append(codec.ExtraArguments ?? "")
                            .Append(',')
                            .Append(item.FPS)
                            .Append(',')
                            .Append(item.VMAF)
                            .Append(',')
                            .Append(item.SSIM)
                            .Append(',')
                            .Append(item.PSNR)
                            .Append(',')
                            .Append(item.CpuUsage)
                            .Append(',')
                            .Append(item.ProcessDuration.TotalSeconds)
                            .Append(',')
                            .Append(item.OutputSize)
                            .Append(',')
                            .Append('"')
                            .Append(ViewModel.TestVideo)
                            .Append('"')
                            .AppendLine();
                    }
                }

                if (!File.Exists(path))
                {
                    File.WriteAllText(path, str.ToString(), new UTF8Encoding(true));
                }
                else
                {
                    File.AppendAllText(path, str.ToString(), new UTF8Encoding(true));
                }
            }
            catch (Exception ex)
            {
                CommonDialog.ShowErrorDialogAsync(ex, "导出失败");
            }
        }

        private void SaveConfigs()
        {
            Config.Instance.TestVideo = ViewModel.TestVideo;
            Config.Instance.TestCodecs = ViewModel.Codecs;
            Config.Instance.TestItems = ViewModel.Tests;
            Config.Instance.TestQCMode = ViewModel.QCMode;
            Config.Instance.Save();
        }

        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Tests.ForEach(p => p.Items.ForEach(q => q.IsChecked = true));
        }

        private void SelectNoneButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Tests.ForEach(p => p.Items.ForEach(q => q.IsChecked = false));
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            SaveConfigs();
            if (!ViewModel.Tests.Any(p => p.Items.Any(q => q.IsChecked)))
            {
                await CommonDialog.ShowErrorDialogAsync("没有选择任何测试项目");
                return;
            }
            try
            {
                stopping = false;
                ViewModel.Progress = 0;
                ViewModel.IsTesting = true;
                ViewModel.Tests.ForEach(q => q.Items.ForEach(p => p.Clear()));
                if (!Directory.Exists(TestDir))
                {
                    Directory.CreateDirectory(TestDir);
                }
                string input = Path.GetFullPath(ViewModel.TestVideo);
                if (!File.Exists(input))
                {
                    throw new FileNotFoundException("不存在测试文件");
                }
                await TestAsync(input);
                SaveConfigs();
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
                ViewModel.DetailProgress = 1;
                IsEnabled = true;
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            IsEnabled = false;
            stopping = true;
            runningFFmpeg?.Cancel();
        }

        /// <summary>
        /// 测试核心代码
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task TestAsync(string input)
        {
            var media = await MediaInfoManager.GetMediaInfoAsync(input);
            var frameCount = media.Videos[0].FrameRate * media.Videos[0].DurationSeconds;
            await CreateRefVideosAsync(input, sizes);
            ViewModel.MaxProgress = ViewModel.Tests.Sum(p => p.Items.Count(q => q.IsChecked));
            if (stopping)
            {
                return;
            }


            var task = new TaskInfo()
            {
                Inputs = new List<InputArguments>(),
                Arguments = new OutputArguments()
                {
                    Video = new VideoCodeArguments(),
                    Audio = null,
                    DisableAudio = true,
                    Format = "mp4"
                },
                Type = TaskType.Code,
            };
            for (int i = 0; i < CodecsCount; i++)
            {
                for (int j = 0; j < sizes.Length; j++)
                {
                    var test = ViewModel.Tests[j];
                    var item = test.Items[i];
                    if (!item.IsChecked)
                    {
                        continue;
                    }


                    //编码速度测试
                    var codec = ViewModel.Codecs[i];
                    ViewModel.Message = $"正在编码：{codec.Name}，{sizeTexts[j]}";
                    ViewModel.DetailProgress = 0;
                    task.Inputs.Clear();
                    task.Inputs.Add(new InputArguments() { FilePath = Path.GetFullPath($"test/{sizeTexts[j]}.mp4") });
                    task.Arguments.Video.Code = codec.Name;
                    task.Arguments.Video.Preset = codec.CpuSpeed;
                    task.Arguments.Video.TwoPass = false;
                    switch (ViewModel.QCMode)//平均码率
                    {
                        case 0:
                            task.Arguments.Video.AverageBitrate = test.MBitrate * codec.BitrateFactor;
                            break;
                        default:
                            task.Arguments.Video.Crf = codec.CRF;
                            break;
                    }
                    task.Arguments.Extra = codec.ExtraArguments;
                    string qctext = ViewModel.QCMode == 0 ? $"bitrate={test.MBitrate}M&factor={codec.BitrateFactor}" : $"crf={codec.CRF}";
                    task.Output = Path.GetFullPath($"{TestDir}/codec={codec.Name}&size={sizeTexts[j]}&speed={codec.CpuSpeed}&{qctext}.mp4");

                    runningFFmpeg = new FFmpegManager(task);
                    runningFFmpeg.StatusChanged += (s, e) =>
                    {
                        var status = runningFFmpeg.GetStatus();
                        if (status.HasDetail && status.Progress != null && status.Progress.Percent is >= 0 and <= 1)
                        {
                            ViewModel.DetailProgress = status.Progress.Percent;
                        }
                    };
                    try
                    {
                        await runningFFmpeg.RunAsync();
                        item.CpuUsage = runningFFmpeg.LastProcess.CpuUsage;
                        item.ProcessDuration = runningFFmpeg.LastProcess.RunningTime;
                        item.FPS = frameCount / item.ProcessDuration.TotalSeconds;
                        item.OutputSize = 1.0 * new FileInfo(task.RealOutput).Length / 1024 / 1024;
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


                    if (stopping)
                    {
                        return;
                    }
                    ViewModel.Progress += 0.5;


                    //编码质量测试
                    var qualityTask = new TaskInfo()
                    {
                        Inputs = new List<InputArguments>()
                        {
                            task.Inputs[0],
                            new InputArguments()
                            {
                                FilePath =task.RealOutput
                            }
                        },
                        Arguments = new OutputArguments(),
                        Type = TaskType.Compare,
                    };
                    ViewModel.Message = $"正在质量测试：{codec.Name}，{sizeTexts[j]}";
                    ViewModel.DetailProgress = 0;


                    runningFFmpeg = new FFmpegManager(qualityTask);
                    runningFFmpeg.StatusChanged += (s, e) =>
                    {
                        var status = runningFFmpeg.GetStatus();
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
                        throw new Exception("质量检测失败", ex);
                    }
                    finally
                    {
                        runningFFmpeg = null;
                    }
                    ViewModel.Progress += 0.5;
                    if (stopping)
                    {
                        return;
                    }

                    string message = qualityTask.Message;
                    item.SSIM = double.Parse(Regex.Match(message, @"All:[0-9\.]+").Value[4..]);
                    item.PSNR = double.Parse(Regex.Match(message, @"average:[0-9\.]+").Value[8..]);
                    try
                    {
                        item.VMAF = double.Parse(Regex.Match(message, @"VMAF score: [0-9\.]+").Value[12..]);
                    }
                    catch
                    {
                        throw new Exception("未放置VMAF模型文件");
                    }
                }
            }
        end:
            ViewModel.Message = "";
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            SaveConfigs();

            if (ViewModel.IsTesting)
            {
                e.Cancel = true;
            }
        }

        private void OpenDirButton_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(TestDir))
            {
                Process.Start("explorer.exe", Path.GetFullPath(TestDir));
            }
            else
            {
                this.CreateMessage().QueueError("测试目录不存在");
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(TestDir))
            {
                FzLib.IO.WindowsFileSystem.DeleteFileOrFolder(TestDir, false, true);
                this.CreateMessage().QueueSuccess("已删除测试目录");
            }
            else
            {
                this.CreateMessage().QueueSuccess("测试目录不存在，无需重置");
            }
        }
    }

    public class TestWindowViewModel : INotifyPropertyChanged
    {
        public const int SizesCount = 4;
        public static readonly string[] sizes = new string[SizesCount] { "-1:720", "-1:1080", "-1:1440", "-1:2160" };
        public static readonly string[] sizeTexts = new string[SizesCount] { "720P", "1080P", "1440P", "2160P" };
        private static readonly int[] bitrates = new int[SizesCount] { 2, 4, 8, 16 };
        private double detailProgress = 0;

        private bool isTesting = false;

        private double maxProgress = 0;

        private string message = "";

        private double progress = 0;

        private int qCMode;
        private string testVideo = "test.mp4";

        public TestWindowViewModel()
        {
            if (Config.Instance.TestItems == null || Config.Instance.TestItems.Any(p => p == null) || Config.Instance.TestItems.Length != Tests.Length)
            {
                for (int i = 0; i < SizesCount; i++)
                {
                    Tests[i] = new PerformanceTestLine() { Header = sizeTexts[i], MBitrate = bitrates[i] };
                    Tests[i].FillItems();
                }
            }
            else
            {
                Tests = Config.Instance.TestItems;
                if (Tests[0].Items[0].Codec == null)
                {
                    foreach (var test in Tests)
                    {
                        for (int i = 0; i < CodecsCount; i++)
                        {
                            test.Items[i].Codec = VideoCodec.VideoCodecs[i].Name;
                        }
                    }
                }
            }
            if (Config.Instance.TestCodecs == null || Config.Instance.TestCodecs.Any(p => p == null) || Config.Instance.TestCodecs.Length != Codecs.Length)
            {
                //Dictionary<string, double> codec2defaultBitrateFactor = new Dictionary<string, double>()
                //{
                //    [VideoCodec.X264.Name] = 1.04,
                //    [VideoCodec.X265.Name] = 1.05,
                //    [VideoCodec.XVP9.Name] = 1.03,
                //    [VideoCodec.AomAV1.Name] = 0.80,
                //    [VideoCodec.SVTAV1.Name] = 1.15,
                //};
                for (int i = 0; i < CodecsCount; i++)
                {
                    Codecs[i] = new PerformanceTestCodecParameter()
                    {
                        CpuSpeed = VideoCodec.VideoCodecs[i].DefaultSpeedLevel,
                        CRF = VideoCodec.VideoCodecs[i].DefaultCRF,
                        //BitrateFactor = codec2defaultBitrateFactor[VideoCodec.VideoCodecs[i].Name],
                        ExtraArguments = ""
                    };
                }
            }
            else
            {
                Codecs = Config.Instance.TestCodecs;
            }
            for (int i = 0; i < CodecsCount; i++)
            {
                Codecs[i].Name = VideoCodec.VideoCodecs[i].Name;
                Codecs[i].MaxCpuSpeed = VideoCodec.VideoCodecs[i].MaxSpeedLevel;
                Codecs[i].MaxCRF = VideoCodec.VideoCodecs[i].MaxCRF;
            }

            if (Config.Instance.TestVideo != null)
            {
                TestVideo = Config.Instance.TestVideo;
            }
            QCMode = Config.Instance.TestQCMode;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public PerformanceTestCodecParameter[] Codecs { get; } = new PerformanceTestCodecParameter[CodecsCount];

        public double DetailProgress
        {
            get => detailProgress;
            set => this.SetValueAndNotify(ref detailProgress, value, nameof(DetailProgress));
        }

        public bool IsTesting
        {
            get => isTesting;
            set => this.SetValueAndNotify(ref isTesting, value, nameof(IsTesting));
        }

        public double MaxProgress
        {
            get => maxProgress;
            set => this.SetValueAndNotify(ref maxProgress, value, nameof(MaxProgress));
        }

        public string Message
        {
            get => message;
            set => this.SetValueAndNotify(ref message, value, nameof(Message));
        }

        public double Progress
        {
            get => progress;
            set => this.SetValueAndNotify(ref progress, value, nameof(Progress));
        }
        public int QCMode
        {
            get => qCMode;
            set => this.SetValueAndNotify(ref qCMode, value, nameof(QCMode));
        }

        public PerformanceTestLine[] Tests { get; } = new PerformanceTestLine[SizesCount];

        public string TestVideo
        {
            get => testVideo;
            set => this.SetValueAndNotify(ref testVideo, value, nameof(TestVideo));
        }
    }
}