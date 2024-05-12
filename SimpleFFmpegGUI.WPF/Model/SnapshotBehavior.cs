using CommunityToolkit.Mvvm.ComponentModel;
using SimpleFFmpegGUI.WPF.ViewModels;
using System;

namespace SimpleFFmpegGUI.WPF.Model
{
    public partial class Snapshot:ViewModelBase
    {
        [ObservableProperty]
        private bool displayFrame;

        [ObservableProperty]
        private bool canUpdate;

        [ObservableProperty]
        private Uri source;
    }
}