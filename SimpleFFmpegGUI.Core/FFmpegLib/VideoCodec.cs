using FFMpegCore.Arguments;
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
        public static readonly X264 X264 = new X264();
        public static readonly X265 X265 = new X265();
        public static readonly XVP9 XVP9 = new XVP9();
        public static readonly AomAV1 AomAV1 = new AomAV1();
        public static readonly SVTAV1 SVTAV1 = new SVTAV1();
        public static readonly GeneralVideoCodec General = new GeneralVideoCodec();
        public static readonly VideoCodec[] VideoCodecs = new VideoCodec[]
        {
           X264,
           X265,
           XVP9,
           AomAV1,
           SVTAV1
        };



        public abstract int DefaultCRF { get; }
        public abstract int DefaultSpeedLevel { get; }
        public abstract int MaxCRF { get; }
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

        public virtual FFmpegArgumentItem Pass(int pass)
        {
            if (pass is not (1 or 2 or 3))
            {
                throw new FFmpegArgumentException("参数pass超出范围");
            }
            return new FFmpegArgumentItem("pass", pass.ToString());
        }

        public abstract FFmpegArgumentItem Speed(int speed);
    }

}
