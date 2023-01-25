using FzLib;
using FzLib.WPF.Converters;
using ModernWpf.FzExtension.CommonDialog;
using SimpleFFmpegGUI.ConstantData;
using SimpleFFmpegGUI.Manager;
using SimpleFFmpegGUI.Model;
using SimpleFFmpegGUI.WPF.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using static SimpleFFmpegGUI.WPF.Model.PerformanceTestLine;
using static SimpleFFmpegGUI.WPF.TestWindowViewModel;

namespace SimpleFFmpegGUI.WPF
{
    public partial class TestWindow : Window
    {
        private const string TestDir = "test";
        private FFmpegManager runningFFmpeg = null;
        private bool stopping = false;
        public TestWindow(TestWindowViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = ViewModel;
            InitializeComponent();
            CreateDataGrids();
            CreateTestItemsDataGrid();
        }

        public TestWindowViewModel ViewModel { get; set; }

        private void CreateDataGrids()
        {
            grpSpeed.Content = GetDataGrid(nameof(PerformanceTestItem.Score));
            grpSSIM.Content = GetDataGrid(nameof(PerformanceTestItem.SSIM), "0.00");
            grpPSNR.Content = GetDataGrid(nameof(PerformanceTestItem.PSNR), "0.00");

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

        private async Task<int> CreateRefVideosAsync(string input, string[] sizes)
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
            int count = 0;
            for (int i = 0; i < sizes.Length; i++)
            {
                var size = sizes[i];

                if (!ViewModel.Tests[i].Items.Any(p => p.IsChecked))
                {
                    continue;
                }
                count++;
                ViewModel.Message = $"正在准备{size.Split(':')[1]}P素材";
                task.Arguments.Video.Size = size;
                task.Output = Path.GetFullPath($"test/{size.Split(':')[1]}P");

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
                ViewModel.Progress += 1;
            }
            return count;
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
                IsReadOnly = true,
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
                var tc = new DataGridTemplateColumn()
                {
                    Width = 108,
                    Header = codec.Name
                };
                tc.CellTemplate = new DataTemplate();
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
        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Tests.ForEach(p => p.Items.ForEach(q => q.IsChecked = true));
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
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
            if (Directory.Exists(TestDir))
            {
                Directory.Delete(TestDir, true);
            }
            int count = await CreateRefVideosAsync(input, sizes);
            ViewModel.MaxProgress = ViewModel.Tests.Sum(p => p.Items.Count(q => q.IsChecked)) + count;
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
                    var item = ViewModel.Tests[j].Items[i];
                    if (!item.IsChecked)
                    {
                        continue;
                    }


                    //编码速度测试
                    ViewModel.Message = $"正在编码测试{ViewModel.Codecs[i].Name}，{sizeTexts[j]}";
                    ViewModel.DetailProgress = 0;
                    task.Inputs.Clear();
                    task.Inputs.Add(new InputArguments() { FilePath = Path.GetFullPath($"test/{sizeTexts[j]}.mp4") });
                    task.Arguments.Video.Code = ViewModel.Codecs[i].Name;
                    task.Arguments.Video.Preset = ViewModel.Codecs[i].CpuSpeed;
                    task.Arguments.Video.AverageBitrate = ViewModel.Tests[j].MBitrate;
                    task.Arguments.Extra = ViewModel.Codecs[i].ExtraArguments;
                    task.Output = Path.GetFullPath($"test/{ViewModel.Codecs[i].Name}-{sizeTexts[j]}.mp4");

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

                    if (stopping)
                    {
                        return;
                    }


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
                    ViewModel.Message = $"正在质量测试{ViewModel.Codecs[i].Name}，{sizeTexts[j]}";
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

                    //统计整理
                    item.Score = fps;
                    string message = qualityTask.Message;
                    item.SSIM = double.Parse(Regex.Match(message, @"All:[0-9\.]+").Value[4..]) * 100;
                    item.PSNR = double.Parse(Regex.Match(message, @"average:[0-9\.]+").Value[8..]);
                }
            }
        end:
            ViewModel.Message = "";
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (ViewModel.IsTesting)
            {
                e.Cancel = true;
            }
        }
    }

    public class TestWindowViewModel : INotifyPropertyChanged
    {
        public const int SizesCount = 4;
        public static readonly string[] sizes = new string[SizesCount] { "-1:720", "-1:1080", "-1:1440", "-1:2160" };
        public static readonly string[] sizeTexts = new string[SizesCount] { "720P", "1080P", "1440P", "2160P" };
        private static readonly int[] bitrates = new int[SizesCount] { 2, 4, 8, 16 };
        private static readonly string[] extraArgs = new string[CodecsCount] { "", "", "-row-mt 1", "-row-mt 1", "" };
        private double detailProgress = 0;

        private bool isTesting = false;

        private double maxProgress = 0;

        private string message = "";

        private double progress = 0;

        public TestWindowViewModel()
        {
            for (int i = 0; i < SizesCount; i++)
            {
                Tests[i] = new PerformanceTestLine() { Header = sizeTexts[i], MBitrate = bitrates[i] };
            }

            for (int i = 0; i < CodecsCount; i++)
            {
                Codecs[i] = new PerformanceTestCodecParameter()
                {
                    Name = VideoCodec.VideoCodecs[i].Name,
                    CpuSpeed = VideoCodec.VideoCodecs[i].DefaultSpeedLevel,
                    MaxCpuSpeed = VideoCodec.VideoCodecs[i].MaxSpeedLevel,
                    ExtraArguments = extraArgs[i]
                };
            }
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

        public PerformanceTestLine[] Tests { get; } = new PerformanceTestLine[SizesCount];
    }
}