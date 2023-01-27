using SimpleFFmpegGUI.FFmpegArgument;

namespace SimpleFFmpegGUI.FFmpegLib
{
    public class XVP9 : VideoCodec
    {
        public override string Name => "VP9";
        public override string Lib => "libvpx-vp9";
        public override int MaxSpeedLevel => 8;
        public override int DefaultSpeedLevel => 3;
        public override int DefaultCRF => 23;

        public override FFmpegArgumentItem Speed(int speed)
        {
            if (speed > MaxSpeedLevel)
            {
                throw new FFmpegArgumentException("速度值超出范围");
            }
            return new FFmpegArgumentItem("cpu-used", speed.ToString());
        }
    }

}
