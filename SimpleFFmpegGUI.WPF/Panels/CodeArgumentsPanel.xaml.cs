using FzLib.Collection;
using Mapster;
using Microsoft.DotNet.PlatformAbstractions;
using Microsoft.Extensions.DependencyInjection;
using SimpleFFmpegGUI.Manager;
using SimpleFFmpegGUI.Model;
using SimpleFFmpegGUI.WPF.ViewModels;
using SimpleFFmpegGUI.WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
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

namespace SimpleFFmpegGUI.WPF.Panels
{
    public enum ChannelOutputStrategy
    {
        [Description("重新编码")]
        Code,

        [Description("复制")]
        Copy,

        [Description("不导出")]
        Disable,
    }

    public partial class CodeArgumentsPanel : UserControl
    {

        public CodeArgumentsPanel()
        {
            DataContext = ViewModel;
            InitializeComponent();
        }
        public CodeArgumentsPanelViewModel ViewModel { get; } = App.ServiceProvider.GetService<CodeArgumentsPanelViewModel>();

        public OutputArguments GetOutputArguments()
        {
            return ViewModel.GetArguments();
        }

        public void SetAsClone()
        {
            ViewModel.CanApplyDefaultPreset = false;
        }

        public  Task UpdateTypeAsync(TaskType type)
        {
          return  ViewModel.UpdateTypeAsync(type);
        }

        public void Update(TaskType type, OutputArguments arguments)
        {
            ViewModel.Update(type, arguments);
        }
        protected override void OnDragOver(DragEventArgs e)
        {
            base.OnDragOver(e);
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];
                if (files.Length == 1 && File.Exists(files[0]))
                {
                    e.Effects = DragDropEffects.Link;
                }
            }
        }

        protected override async void OnDrop(DragEventArgs e)
        {
            base.OnDrop(e);
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];
                if (files.Length == 1 && File.Exists(files[0]))
                {
                    var file = files[0];
                    try
                    {
                        IsEnabled = false;
                        var info = await MediaInfoManager.GetMediaInfoAsync(file);
                        var videoArgs = MediaInfoManager.ConvertToVideoArguments(info);
                        ViewModel.Video = videoArgs.Adapt<VideoArgumentsViewModel>();
                        if (videoArgs.Crf.HasValue)
                        {
                            ViewModel.Video.EnableCrf = true;
                        }
                        if (videoArgs.AverageBitrate.HasValue)
                        {
                            ViewModel.Video.EnableAverageBitrate = true;
                        }
                        if (videoArgs.MaxBitrate.HasValue)
                        {
                            ViewModel.Video.EnableMaxBitrate = true;
                        }
                        this.CreateMessage().QueueSuccess("已加载指定视频参数");
                    }
                    catch (Exception ex)
                    {
                        this.CreateMessage().QueueError("解析视频编码参数失败", ex);
                    }
                    finally
                    {
                        IsEnabled = true;
                    }

                }
            }

        }
    }
    public class Int2StringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string s)
            {
                Dictionary<int, string> dic = null;
                try
                {
                    dic = s.Split(';').Select(p => p.Split(':')).ToDictionary(p => int.Parse(p[0]), p => p[1]);
                }
                catch (Exception ex)
                {
                    throw new Exception("无法解析参数", ex);
                }
                if (value is int i)
                {
                    if (!dic.ContainsKey(i))
                    {
                        throw new Exception("没有值" + i + "的转换目标");
                    }
                    return dic[i];
                }
                throw new ArgumentException("绑定值必须为整数");
            }
            throw new ArgumentException("参数必须为字符串");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}