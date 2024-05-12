using Enterwell.Clients.Wpf.Notifications;
using FzLib.WPF;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using SimpleFFmpegGUI.Dto;
using SimpleFFmpegGUI.Model;
using SimpleFFmpegGUI.WPF.ViewModels;
using SimpleFFmpegGUI.WPF.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SimpleFFmpegGUI.WPF.Pages
{
    public partial class MediaInfoPage : UserControl
    {
        public MediaInfoPage()
        {
            ViewModel = this.SetDataContext<MediaInfoPageViewModel>();
            InitializeComponent();
        }

        public MediaInfoPageViewModel ViewModel { get; set; }

        public void SetFile(string file)
        {
            ViewModel.FilePath = file;
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            base.OnDragOver(e);
            if (e.Data.GetDataPresent(DataFormats.FileDrop)
                && (e.Data.GetData(DataFormats.FileDrop) as string[]).Length == 1
                && System.IO.File.Exists((e.Data.GetData(DataFormats.FileDrop) as string[])[0]))
            {
                e.Effects = DragDropEffects.Link;
            }
        }

        protected override void OnDrop(DragEventArgs e)
        {
            base.OnDrop(e);
            if (e.Data.GetDataPresent(DataFormats.FileDrop)
            && (e.Data.GetData(DataFormats.FileDrop) as string[]).Length == 1
            && System.IO.File.Exists((e.Data.GetData(DataFormats.FileDrop) as string[])[0]))
            {
                ViewModel.FilePath = (e.Data.GetData(DataFormats.FileDrop) as string[])[0];
            }
        }

        private void TextBox_MouseLeave(object sender, MouseEventArgs e)
        {
            Keyboard.ClearFocus();
        }
    }
}