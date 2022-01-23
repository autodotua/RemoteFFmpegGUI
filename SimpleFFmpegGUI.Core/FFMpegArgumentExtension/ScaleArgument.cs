using FFMpegCore.Arguments;

namespace SimpleFFmpegGUI.FFMpegArgumentExtension
{
    public class ScaleArgument : IVideoFilterArgument
    {
        public string Key { get; } = "scale";

        public string Value { get; private set; }

        public ScaleArgument(string size)
        {
            Value = size;
        }
    }
}