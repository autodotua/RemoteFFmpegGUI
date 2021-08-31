using Microsoft.Extensions.DependencyInjection;
using SimpleFFmpegGUI.Manager;
using System;
using System.Collections.Generic;
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

namespace SimpleFFmpegGUI.WPF
{
    public class MainWindowViewModel
    {
        public QueueManager queue;

        public MainWindowViewModel(QueueManager queue)
        {
            this.queue = queue;
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindowViewModel ViewModel { get; set; }

        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = App.ServiceProvider.GetService<AddTaskWindow>();
            dialog.Owner = this;
            dialog.ShowDialog();
        }
    }
}