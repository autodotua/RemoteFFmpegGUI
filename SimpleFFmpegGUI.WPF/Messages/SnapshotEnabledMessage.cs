using SimpleFFmpegGUI.WPF.Model;

namespace SimpleFFmpegGUI.WPF.Messages
{
    public class SnapshotEnabledMessage(Snapshot options)
    {
        public Snapshot Options { get; private set; } = options;
    }
}
