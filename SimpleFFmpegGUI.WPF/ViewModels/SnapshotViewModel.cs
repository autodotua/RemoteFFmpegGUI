using CommunityToolkit.Mvvm.ComponentModel;
using SimpleFFmpegGUI.WPF.ViewModels;
using System;

namespace SimpleFFmpegGUI.WPF.ViewModels
{
    public partial class SnapshotViewModel : ViewModelBase
    {
        [ObservableProperty]
        private bool displayFrame;

        [ObservableProperty]
        private bool canUpdate;

        [ObservableProperty]
        private Uri source;
    }
}