using FFMpegCore.Arguments;

namespace SimpleFFmpegGUI.FFMpegArgumentExtension
{
    public class VideoPixelFormatArgument : IArgument
    {
        public readonly string Format;

        public VideoPixelFormatArgument(string format)
        {
            Format = format;
        }

        public string Text => $"-pix_fmt {Format}";
    }
}