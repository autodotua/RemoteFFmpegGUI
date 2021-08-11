using FFMpegCore.Arguments;

namespace SimpleFFmpegGUI.Host
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