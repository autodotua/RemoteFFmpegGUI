using FFMpegCore.Arguments;

namespace SimpleFFmpegGUI.FFMpegArgumentExtension
{
    public class VideoMBitrateArgument : IArgument
    {
        public readonly double Bitrate;

        public VideoMBitrateArgument(double bitrate)
        {
            Bitrate = bitrate;
        }

        public string Text => $"-b:v {Bitrate}M";
    }
}