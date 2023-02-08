using SimpleFFmpegGUI.FFmpegArgument;

namespace SimpleFFmpegGUI.FFmpegLib
{
    public class GeneralVideoCodec : VideoCodec
    {
        public override int DefaultCRF => 5;
        public override int DefaultSpeedLevel => 3;
        public override string Lib => null;
        public override int MaxSpeedLevel => 10;
        public override string Name => null;
        public override int MaxCRF => 63;

        public override FFmpegArgumentItem Speed(int speed)
        {
            if (speed > MaxSpeedLevel)
            {
                throw new FFmpegArgumentException("速度值超出范围");
            }
            return new FFmpegArgumentItem("preset", FFmpegEnums.Presets[speed]); throw new System.NotImplementedException();
        }
    }

}
