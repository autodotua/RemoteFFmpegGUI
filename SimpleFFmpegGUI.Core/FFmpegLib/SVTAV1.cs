using SimpleFFmpegGUI.FFmpegArgument;
using System;

namespace SimpleFFmpegGUI.FFmpegLib
{
    public class SVTAV1 : VideoCodec
    {
        public override string Name => "AV1 (SVT)";
        public override string Lib => "libsvtav1";
        public override int MaxSpeedLevel => 12;
        public override int DefaultSpeedLevel => 6;
        public override int DefaultCRF => 28;

        public override FFmpegArgumentItem Speed(int speed)
        {
            if (speed < -1 || speed > MaxSpeedLevel)
            {
                throw new FFmpegArgumentException("编码速度值超出范围");
            }
            return new FFmpegArgumentItem("preset", speed.ToString());
        }

        public override FFmpegArgumentItem AverageBitrate(double mb)
        {
            if (mb < 0)
            {
                throw new FFmpegArgumentException("平均码率超出范围");
            }
            return new FFmpegArgumentItem("rc", "1", "svtav1-params", ':')
            {
                Other = new FFmpegArgumentItem("tbr", Convert.ToInt32(mb * 1000).ToString(), "svtav1-params", ':')
            };
        }

        public override FFmpegArgumentItem MaxBitrate(double mb)
        {
            if (mb < 0)
            {
                throw new FFmpegArgumentException("最大码率超出范围");
            }
            return new FFmpegArgumentItem("mbr", Convert.ToInt32(mb * 1000).ToString(), "svtav1-params", ':');
        }
        public override FFmpegArgumentItem BufferSize(double mb)
        {
            throw new FFmpegArgumentException("SVTAV1编译器不支持该参数：" + nameof(BufferSize));
        }
        public override FFmpegArgumentItem FrameRate(double fps)
        {
            if (fps < 0)
            {
                throw new FFmpegArgumentException("帧速率大小超出范围");
            }
            return new FFmpegArgumentItem("fps", fps.ToString(), "svtav1-params", ':');
        }
    }

}
