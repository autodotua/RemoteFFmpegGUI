using FFMpegCore;
using FFMpegCore.Arguments;
using FFMpegCore.Enums;
using System;

namespace SimpleFFmpegGUI.FFMpegArgumentExtension
{
    public static class FFMpegArgumentOptionsExtension
    {
        public static FFMpegArgumentOptions WithVideoMBitrate(this FFMpegArgumentOptions opt, double bitrate)
            => opt.WithArgument(new VideoMBitrateArgument(bitrate));

        public static FFMpegArgumentOptions WithVideoMMaxBitrate(this FFMpegArgumentOptions opt, double bitrate, double bufferMagnification = 2)
            => opt.WithArgument(new VideoMMaxBitrateArgument(bitrate, bufferMagnification));

        public static FFMpegArgumentOptions WithArguments(this FFMpegArgumentOptions opt, string arg)
            => opt.WithArgument(new CustomArgument(arg));

        public static FFMpegArgumentOptions To(this FFMpegArgumentOptions opt, TimeSpan to)
            => opt.WithArgument(new ToArgument(to));

        public static FFMpegArgumentOptions WithMapping(this FFMpegArgumentOptions opt, int inputIndex, Channel channel, int? streamIndex)
            => opt.WithArgument(new MapArgument(inputIndex, channel, streamIndex));

        public static FFMpegArgumentOptions WithVideoResolution(this FFMpegArgumentOptions opt, int width, int height)
            => opt.WithArgument(new VideoResolutionArgument(width, height));

        public static FFMpegArgumentOptions WithVideoPixelFormat(this FFMpegArgumentOptions opt, string format)
            => opt.WithArgument(new VideoPixelFormatArgument(format));

        public static FFMpegArgumentOptions WithVideoAspect(this FFMpegArgumentOptions opt, string aspect)
            => opt.WithArgument(new VideoAspectArgument(aspect));

        public static FFMpegArgumentOptions WithTwoPass(this FFMpegArgumentOptions opt, string codec, int pass)
            => opt.WithArgument(new VideoTwoPassArgument(codec, pass));

      public static FFMpegArgumentOptions WithCpuSpeed(this FFMpegArgumentOptions opt, string codec, int level)
            => opt.WithArgument(new VideoCpuSpeedArgument(codec, level));

        public static VideoFilterOptions Scale(this VideoFilterOptions opt, string size)
        {
            opt.Arguments.Add(new ScaleArgument(size));
            return opt;
        }
    }
}