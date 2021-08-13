using FFMpegCore.Arguments;

namespace SimpleFFmpegGUI.FFMpegArgumentExtension
{
    public class VideoMMaxBitrateArgument : IArgument
    {
        public readonly double MaxBitrate;
        public readonly double BufferMagnification;

        public VideoMMaxBitrateArgument(double maxBitrate, double bufferMagnification)
        {
            MaxBitrate = maxBitrate;
            BufferMagnification = bufferMagnification;
        }

        public string Text => $"-maxrate {MaxBitrate}M -bufsize {MaxBitrate * BufferMagnification}M";
    }
}