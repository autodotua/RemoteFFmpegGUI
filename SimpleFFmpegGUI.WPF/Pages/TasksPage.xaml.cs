using Enterwell.Clients.Wpf.Notifications;
using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;
using FzLib;
using FzLib.WPF;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using SimpleFFmpegGUI.Dto;
using SimpleFFmpegGUI.Manager;
using SimpleFFmpegGUI.Model;
using SimpleFFmpegGUI.WPF.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
using Size = System.Drawing.Size;

namespace SimpleFFmpegGUI.WPF.Pages
{
    public class TasksPageViewModel : INotifyPropertyChanged
    {
        public TasksPageViewModel()
        {
            AllTasks.PropertyChanged += AllTasks_PropertyChanged;
            RefreshPages();
        }

        private void AllTasks_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RefreshPages();
        }

        private void RefreshPages()
        {
            var pages = Enumerable.Range(0, AllTasks.PageCount)
                 .Select(p => new
                 {
                     Label = "第" + (p + 1) + "页",
                     Value = p,
                     Value1 = p + 1
                 });
            if (Pages == null || pages.Count() != Pages.Cast<object>().Count())
            {
                Pages = pages;
            }
        }

        private IEnumerable pages;

        public IEnumerable Pages
        {
            get => pages;
            set => this.SetValueAndNotify(ref pages, value, nameof(Pages));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public AllTasks AllTasks => App.ServiceProvider.GetService<AllTasks>();
    }

    /// <summary>
    /// Interaction logic for TasksPage.xaml
    /// </summary>
    public partial class TasksPage : UserControl
    {
        public TasksPageViewModel ViewModel { get; set; }

        //private TimeSpan length;
        //private double fps;

        public TasksPage(TasksPageViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            DataContext = ViewModel;
        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            App.ServiceProvider.GetService<AllTasks>().Refresh();
        }
    }
}