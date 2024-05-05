namespace SimpleFFmpegGUI.WPF.Messages
{
    public class WindowEnableMessage(bool isEnabled)
    {
        public bool IsEnabled { get; } = isEnabled;
    }
}
