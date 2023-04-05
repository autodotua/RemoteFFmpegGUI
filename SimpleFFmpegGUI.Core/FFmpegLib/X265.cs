using SimpleFFmpegGUI.FFmpegArgument;

namespace SimpleFFmpegGUI.FFmpegLib
{
    public class X265 : VideoCodec
    {
        public readonly static string[] Profiles = new string[]
        {
            "main",
            "main444-8",
            "main10",
            "main422-10",
            "main444-10"
        };

        public override string Name => "H265";
        public override string Lib => "libx265";
        public override int MaxSpeedLevel => FFmpegEnums.Presets.Length - 1;
        public override int DefaultSpeedLevel => 3;
        public override int DefaultCRF => 28;
        public override int MaxCRF => 51;

        public override double[] SpeedFPSRelationship => new[] { 1.5142, 2.6010, 11.5024, 33.7668, 34.5906, 42.4121, 42.5331, 61.8358, 80.5850 };


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
