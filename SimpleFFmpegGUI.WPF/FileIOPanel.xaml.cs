using FzLib;
using Mapster;
using Microsoft.DotNet.PlatformAbstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAPICodePack.FzExtension;
using SimpleFFmpegGUI.Manager;
using SimpleFFmpegGUI.Model;
using SimpleFFmpegGUI.WPF;
using System;
using System.Collections;
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

namespace SimpleFFmpegGUI.WPF
{
    public class InputArgumentsDetail : InputArguments, INotifyPropertyChanged, ITempArguments
    {
        public void SetFile(string path)
        {
            FilePath = path;
            this.Notify(nameof(path));
        }

        public void Update()
        {
            throw new NotImplementedException();
        }

        public void Apply()
        {
            throw new NotImplementedException();
        }

        private int index;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Index
        {
            get => index;
            set => this.SetValueAndNotify(ref index, value, nameof(Index));
        }

        private bool canDelete;

        public bool CanDelete
        {
            get => canDelete;
            set => this.SetValueAndNotify(ref canDelete, value, nameof(CanDelete));
        }

        public bool EnableFrom { get; set; }
        public bool EnableTo { get; set; }
        public bool EnableDuration { get; set; }
    }

    public class FileIOPanelViewModel : INotifyPropertyChanged
    {
        public FileIOPanelViewModel()
        {
            for (int i = 0; i < MinInputsCount; i++)
            {
                Inputs.Add(new InputArgumentsDetail() { Index = i + 1, });
            }
            Inputs.CollectionChanged += Inputs_CollectionChanged;
        }

        public int MinInputsCount { get; set; } = 1;

        private void Inputs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            for (int i = 0; i < Inputs.Count; i++)
            {
                Inputs[i].Index = i + 1;
                Inputs[i].CanDelete = i >= MinInputsCount;
            }
        }

        public ObservableCollection<InputArgumentsDetail> Inputs { get; } = new ObservableCollection<InputArgumentsDetail>();

        public event PropertyChangedEventHandler PropertyChanged;

        private bool canChangeInputsCount;

        public bool CanChangeInputsCount
        {
            get => canChangeInputsCount;
            set => this.SetValueAndNotify(ref canChangeInputsCount, value, nameof(CanChangeInputsCount));
        }
    }

    public partial class FileIOPanel : UserControl
    {
        public FileIOPanel()
        {
            DataContext = ViewModel;
            InitializeComponent();
        }

        public FileIOPanelViewModel ViewModel { get; } = App.ServiceProvider.GetService<FileIOPanelViewModel>();

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Inputs.Add(new InputArgumentsDetail());
        }

        private void BrowseFileButton_Click(object sender, RoutedEventArgs e)
        {
            var input = (sender as FrameworkElement).Tag as InputArgumentsDetail;
            string path = new FileFilterCollection().AddAll().CreateOpenFileDialog().GetFilePath();
            if (path != null)
            {
                input.SetFile(path);
            }
        }

        private void DeleteFileButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Inputs.Remove((sender as FrameworkElement).Tag as InputArgumentsDetail);
        }

        public void Update(TaskType type)
        {
            ViewModel.CanChangeInputsCount = type is TaskType.Code or TaskType.Combine or TaskType.Concat;
        }
    }
}