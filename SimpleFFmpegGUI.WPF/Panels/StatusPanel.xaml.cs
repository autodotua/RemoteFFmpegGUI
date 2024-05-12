using SimpleFFmpegGUI.WPF.ViewModels;
using System.Windows.Controls;

namespace SimpleFFmpegGUI.WPF.Panels
{
    public partial class StatusPanel : UserControl
    {
        public StatusPanel()
        {
            ViewModel = this.SetDataContext<StatusPanelViewModel>();
            InitializeComponent();
        }

        public StatusPanelViewModel ViewModel { get; }
    }
}