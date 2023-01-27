using SimpleFFmpegGUI.FFmpegArgument;

namespace SimpleFFmpegGUI.FFmpegLib
{
    public class AomAV1 : VideoCodec
    {
        public override string Name => "AV1 (aom)";
        public override string Lib => "libaom-av1";
        public override int MaxSpeedLevel => 8;
        public override int DefaultSpeedLevel => 3;
        public override int DefaultCRF => 28;

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
