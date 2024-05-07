using CommunityToolkit.Mvvm.Messaging;
using FzLib.WPF;
using Microsoft.DotNet.PlatformAbstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using ModernWpf.FzExtension.CommonDialog;
using SimpleFFmpegGUI.Manager;
using SimpleFFmpegGUI.Model;
using SimpleFFmpegGUI.WPF.Messages;
using SimpleFFmpegGUI.WPF.Model;
using SimpleFFmpegGUI.WPF.Pages;
using SimpleFFmpegGUI.WPF.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CommonDialog = ModernWpf.FzExtension.CommonDialog.CommonDialog;
using Path = System.IO.Path;

namespace SimpleFFmpegGUI.WPF.Panels
{
    public partial class FileIOPanel : UserControl
    {
        public FileIOPanel()
        {
            ViewModel = this.SetDataContext<FileIOPanelViewModel>();
            InitializeComponent();
        }

        public FileIOPanelViewModel ViewModel { get; }

        public void AddInput()
        {
            if (ViewModel.Inputs.Count >= ViewModel.MaxInputsCount)
            {
                throw new NotSupportedException("无法继续增加输入文件");
            }
            ViewModel.Inputs.Add(new InputArgumentsDetail());
        }

        public void BrowseAndAddInput()
        {
            var dialog = new OpenFileDialog().AddAllFilesFilter();
            string path = dialog.GetPath(this.GetWindow());
            if (path != null)
            {
                var input = new InputArgumentsDetail();
                input.FilePath = path;
                ViewModel.Inputs.Add(input);
            }
        }




        /// <summary>
        /// 用于添加到远程主机，获取输出文件名
        /// </summary>
        /// <returns></returns>
        public string GetOutputFileName()
        {
            if (ViewModel.CanSetOutputFileName)//需要可以设置输出文件名
            {
                if (!string.IsNullOrWhiteSpace(ViewModel.OutputFileName))//如果手动指定
                {
                    return ViewModel.OutputFileName;
                }
                if (ViewModel.Inputs.Where(p => !string.IsNullOrEmpty(p.FilePath)).Any())//如果未手动指定并且存在输入文件
                {
                    return Path.GetFileName(ViewModel.Inputs.Where(p => !string.IsNullOrEmpty(p.FilePath)).First().FilePath);
                }
            }
            return null;
        }

        public void Reset()
        {
            ViewModel.Reset(false);
        }

        public void Update(TaskType type, List<InputArguments> inputs, string output)
        {
            ViewModel.Update(type, inputs, output);
        }

        public void UpdateType(TaskType type)
        {
            ViewModel.UpdateType(type);
            ViewModel.Reset(true);
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            base.OnDragOver(e);
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Link;
            }
        }

        protected override void OnDrop(DragEventArgs e)
        {
            base.OnDrop(e);
            var files = e.Data.GetData(DataFormats.FileDrop) as string[];

            foreach (var file in ViewModel.Inputs.Where(p => string.IsNullOrEmpty(p.FilePath)).ToList())
            {
                ViewModel.Inputs.Remove(file);
            }
            foreach (string file in files)
            {
                if (ViewModel.Inputs.Count >= ViewModel.MinInputsCount && !ViewModel.CanChangeInputsCount)
                {
                    break;
                }
                if (File.Exists(file))
                {
                    ViewModel.Inputs.Add(new InputArgumentsDetail() { FilePath = file });
                }
            }
            while (ViewModel.Inputs.Count < ViewModel.MinInputsCount)
            {
                ViewModel.Inputs.Add(new InputArgumentsDetail());
            }
        }
    }
}