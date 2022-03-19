using FzLib.Program.Runtime;
using Microsoft.Extensions.DependencyInjection;
using SimpleFFmpegGUI.Manager;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SimpleFFmpegGUI.WPF.Cut
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
#if !DEBUG
                WPFUnhandledExceptionCatcher.RegistAll().UnhandledExceptionCatched += UnhandledException_UnhandledExceptionCatched;
#endif

            Unosquare.FFME.Library.FFmpegDirectory = FzLib.Program.App.ProgramDirectoryPath;

            MainWindow = new CutWindow(new CutWindowViewModel(),e.Args);
            MainWindow.Show();
            
        }



        private void UnhandledException_UnhandledExceptionCatched(object sender, FzLib.Program.Runtime.UnhandledExceptionEventArgs e)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    var result = MessageBox.Show("程序发生异常，可能出现数据丢失等问题。是否关闭？" + Environment.NewLine + Environment.NewLine + e.Exception.ToString(), FzLib.Program.App.ProgramName+" - 未捕获的异常", MessageBoxButton.YesNo, MessageBoxImage.Error);
                    if (result == MessageBoxResult.Yes)
                    {
                        Shutdown(-1);
                    }
                });
            }
            catch (Exception ex)
            {
            }
        }
    }
}