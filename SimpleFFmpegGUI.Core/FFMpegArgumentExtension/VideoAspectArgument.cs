using FFMpegCore.Arguments;

namespace SimpleFFmpegGUI.FFMpegArgumentExtension
{
    public class VideoAspectArgument : IArgument
    {
        public readonly string Aspect;

        public VideoAspectArgument(string aspect)
        {
            Aspect = aspect;
        }

        public string Text => $"-aspect {Aspect}";
    }
}