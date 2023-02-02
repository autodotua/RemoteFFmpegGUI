using SimpleFFmpegGUI.FFmpegArgument;

namespace SimpleFFmpegGUI.FFmpegLib
{
    public class X264 : VideoCodec
    {
        public readonly static string[] Profiles = new string[]
        {
            "baseline",
            "main",
            "high",
            "high10",
            "high422",
            "high444"
        };

        public override string Name => "H264";
        public override string Lib => "libx264";
        public override int MaxSpeedLevel => FFmpegEnums.Presets.Length-1;
        public override int DefaultSpeedLevel => 3;
        public override int DefaultCRF => 23;

        public override FFmpegArgumentItem Speed(int speed)
        {
            if (speed > MaxSpeedLevel)
            {
                throw new FFmpegArgumentException("速度值超出范围");
            }
            return new FFmpegArgumentItem("preset", FFmpegEnums.Presets[speed]);
        }
    }

}
