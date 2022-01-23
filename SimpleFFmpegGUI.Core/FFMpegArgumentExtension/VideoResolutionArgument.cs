using FFMpegCore.Arguments;

namespace SimpleFFmpegGUI.FFMpegArgumentExtension
{
    public class VideoResolutionArgument : IArgument
    {
        public readonly double Width;
        public readonly double Height;

        public VideoResolutionArgument(double width, double height)
        {
            Width = width;
            Height = height;
        }

        public string Text => $"-s {Width}x{Height}";
    }
}