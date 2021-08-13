using FFMpegCore.Arguments;

namespace SimpleFFmpegGUI.FFMpegArgumentExtension
{
    public class CustomArgument : IArgument
    {
        public readonly string arg;

        public CustomArgument(string arg)
        {
            this.arg = arg;
        }

        public string Text => arg;
    }
}