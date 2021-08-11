using FFMpegCore;

namespace SimpleFFmpegGUI.Host
{
    public static class FFMpegArgumentOptionsExtension
    {
        public static FFMpegArgumentOptions WithVideoMBitrate(this FFMpegArgumentOptions opt, double bitrate)
            => opt.WithArgument(new VideoMBitrateArgument(bitrate));

        public static FFMpegArgumentOptions WithVideoMMaxBitrate(this FFMpegArgumentOptions opt, double bitrate, double bufferMagnification = 2)
            => opt.WithArgument(new VideoMMaxBitrateArgument(bitrate, bufferMagnification));
    }
}