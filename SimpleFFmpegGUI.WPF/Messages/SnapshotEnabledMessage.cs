using SimpleFFmpegGUI.WPF.ViewModels;

namespace SimpleFFmpegGUI.WPF.Messages
{
    public class SnapshotEnabledMessage(SnapshotViewModel options)
    {
        public SnapshotViewModel Options { get; private set; } = options;
    }
}
