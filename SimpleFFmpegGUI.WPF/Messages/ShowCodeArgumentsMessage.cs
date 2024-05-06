using SimpleFFmpegGUI.Model;
using SimpleFFmpegGUI.WPF.Model;

namespace SimpleFFmpegGUI.WPF.Messages
{
    public class ShowCodeArgumentsMessage(UITaskInfo task)
    {
        public UITaskInfo Task { get; } = task;
    }
}
