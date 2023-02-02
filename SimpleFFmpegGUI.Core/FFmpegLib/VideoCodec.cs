using FFMpegCore.Enums;
using FzLib.DataAnalysis;
using SimpleFFmpegGUI.FFmpegArgument;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFFmpegGUI.FFmpegLib
{
    public abstract class VideoCodec : CodecBase
    {
        public static readonly VideoCodec[] VideoCodecs = new VideoCodec[]
        {
            new X264(),
            new X265(),
            new XVP9(),
            new AomAV1(),
            new SVTAV1()
        };



        public abstract int DefaultCRF { get; }
        public abstract int DefaultSpeedLevel { get; }
        public virtual int MaxCRF { get; } = 63;
        public abstract int MaxSpeedLevel { get; }
        public virtual IEnumerable<FFmpegArgumentItem> ExtraArguments()
        {
            return Enumerable.Empty<FFmpegArgumentItem>();
        }
        public virtual FFmpegArgumentItem AverageBitrate(double mb)
        {
            if (mb < 0)
            {
                throw new FFmpegArgumentException("平均码率超出范围");
            }
            return new FFmpegArgumentItem("b:v", $"{mb}M");
        }

        public virtual FFmpegArgumentItem BufferSize(double mb)
        {
            if (mb < 0)
            {
                throw new FFmpegArgumentException("缓冲大小超出范围");
            }
            return new FFmpegArgumentItem("bufsize", $"{mb}M");
        }

        public virtual FFmpegArgumentItem CRF(int level)
        {
            if (level < 0 || level > MaxCRF)
            {
                throw new FFmpegArgumentException("CRF的值超出范围");
            }
            return new FFmpegArgumentItem("crf", level.ToString());
        }

        public virtual FFmpegArgumentItem FrameRate(double fps)
        {
            if (fps < 0)
            {
                throw new FFmpegArgumentException("帧速率大小超出范围");
            }
            return new FFmpegArgumentItem("r", fps.ToString());
        }

        public virtual FFmpegArgumentItem MaxBitrate(double mb)
        {
            if (mb < 0)
            {
                throw new FFmpegArgumentException("最大码率超出范围");
            }
            return new FFmpegArgumentItem("maxrate", $"{mb}M");
        }

        public virtual FFmpegArgumentItem PixelFormat(string format)
        {
            if (string.IsNullOrWhiteSpace(format))
            {
                throw new FFmpegArgumentException("提供的像素格式为空");
            }
            return new FFmpegArgumentItem("pix_fmt", format);
        }

        public abstract FFmpegArgumentItem Speed(int speed);
    }

}
