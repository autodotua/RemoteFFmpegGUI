using Enterwell.Clients.Wpf.Notifications;
using FzLib;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAPICodePack.FzExtension;
using SimpleFFmpegGUI.Dto;
using SimpleFFmpegGUI.Manager;
using SimpleFFmpegGUI.Model;
using SimpleFFmpegGUI.WPF.Model;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
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

namespace SimpleFFmpegGUI.WPF.Pages
{
    public class SettingPageViewModel : Config
    {
        public SettingPageViewModel()
        {
            Config.Instance.Adapt(this);
            ObservableRemoteHosts = new ObservableCollection<RemoteHost>(RemoteHosts);
            this.Notify(nameof(ObservableRemoteHosts));
        }

        public ObservableCollection<RemoteHost> ObservableRemoteHosts { get; set; }
    }

    /// <summary>
    /// Interaction logic for SettingPage.xaml
    /// </summary>
    public partial class SettingPage : UserControl
    {
        public SettingPage(SettingPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = ViewModel;
            InitializeComponent();
        }

        public SettingPageViewModel ViewModel { get; set; }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.RemoteHosts = ViewModel.ObservableRemoteHosts.ToList();
            ViewModel.Adapt(Config.Instance);
            Config.Instance.Save();
            this.GetWindow<MainWindow>().RemoveTab(this);
        }

        private void AddRemoteHost_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ObservableRemoteHosts.Add(new RemoteHost());
        }

        private void CommandBar_MouseEnter(object sender, MouseEventArgs e)
        {
            Keyboard.ClearFocus();
        }
    }
}