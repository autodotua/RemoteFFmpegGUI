using SimpleFFmpegGUI.FFmpegLib;
using System.Collections.Generic;
using System.Linq;
using VideoCodec = SimpleFFmpegGUI.FFmpegLib.VideoCodec;

namespace SimpleFFmpegGUI.FFmpegArgument
{
    public class VideoArgumentsGenerator : ArgumentsGeneratorBase
    {
        /// <summary>
        /// 最大码率
        /// </summary>
        private double? maxBitrate;
        
        /// <summary>
        /// 视频编码
        /// </summary>
        public VideoCodec VideoCodec { get; private set; }

        /// <summary>
        /// 画面比例
        /// </summary>
        /// <param name="aspect"></param>
        /// <returns></returns>
        /// <exception cref="FFmpegArgumentException"></exception>
        public VideoArgumentsGenerator Aspect(string aspect)
        {
            if (string.IsNullOrEmpty(aspect))
            {
                return this;
            }
            if (double.TryParse(aspect, out _))
            {
                arguments.Add(new FFmpegArgumentItem("aspect", aspect));
                return this;
            }
            string[] parts = aspect.Split(':');
            if (parts.Length == 2 || double.TryParse(parts[0], out _) && double.TryParse(parts[1], out _))
            {
                arguments.Add(new FFmpegArgumentItem("aspect", aspect));
                return this;
            }
            throw new FFmpegArgumentException("无法解析的画面比例格式");
        }

        /// <summary>
        /// 平均码率
        /// </summary>
        /// <param name="mb"></param>
        /// <returns></returns>
        public VideoArgumentsGenerator AverageBitrate(double? mb)
        {
            if (mb.HasValue)
            {
                arguments.Add(VideoCodec.AverageBitrate(mb.Value));
            }
            return this;
        }

        /// <summary>
        /// 设置最大码率时，缓冲区与最大码率的比值
        /// </summary>
        /// <param name="ratio"></param>
        /// <returns></returns>
        /// <exception cref="FFmpegArgumentException"></exception>
        public VideoArgumentsGenerator BufferRatio(double? ratio)
        {
            if (ratio.HasValue && VideoCodec.Name != VideoCodec.SVTAV1.Name)
            {
                if (maxBitrate == null)
                {
                    throw new FFmpegArgumentException("应先设置最大码率，然后设置缓冲比例");
                }
                arguments.Add(VideoCodec.BufferSize(ratio.Value * maxBitrate.Value));
            }
            return this;
        }

        /// <summary>
        /// 视频编码
        /// </summary>
        /// <param name="codec"></param>
        /// <returns></returns>
        public VideoArgumentsGenerator Codec(string codec)
        {
            codec = codec.ToLower();
            foreach (var c in VideoCodec.VideoCodecs)
            {
                if (c.Name.ToLower() == codec || c.Lib.ToLower() == codec)
                {
                    VideoCodec = c;
                    arguments.Add(new FFmpegArgumentItem("c:v", c.Lib));
                    return this;
                }
            }
            VideoCodec = new GeneralVideoCodec();
            if (codec is not ("自动" or "auto") && !string.IsNullOrEmpty(codec))
            {
                arguments.Add(new FFmpegArgumentItem("c:v", codec));
            }
            return this;
        }

        /// <summary>
        /// 复制视频流
        /// </summary>
        /// <returns></returns>
        public VideoArgumentsGenerator Copy()
        {
            arguments.Add(new FFmpegArgumentItem("c:v", "copy"));
            return this;
        }

        /// <summary>
        /// CRF（恒定画面质量）系数
        /// </summary>
        /// <param name="crf"></param>
        /// <returns></returns>
        public VideoArgumentsGenerator CRF(int? crf)
        {
            if (crf.HasValue)
            {
                arguments.Add(VideoCodec.CRF(crf.Value));
            }
            return this;
        }

        /// <summary>
        /// 禁用视频流
        /// </summary>
        /// <returns></returns>
        public VideoArgumentsGenerator Disable()
        {
            arguments.Add(new FFmpegArgumentItem("vn"));
            return this;
        }

        /// <summary>
        /// 最大码率
        /// </summary>
        /// <param name="mb"></param>
        /// <returns></returns>
        public VideoArgumentsGenerator MaxBitrate(double? mb)
        {
            if (mb.HasValue)
            {
                maxBitrate = mb;
                arguments.Add(VideoCodec.MaxBitrate(mb.Value));
            }
            return this;
        }

        /// <summary>
        /// 编码次数（二次编码）
        /// </summary>
        /// <param name="pass"></param>
        /// <returns></returns>
        public VideoArgumentsGenerator Pass(int? pass)
        {
            if (!pass.HasValue || pass.Equals(0))
            {
                return this;
            }
            arguments.Add(VideoCodec.Pass(pass.Value));
            return this;
        }

        /// <summary>
        /// 像素格式
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public VideoArgumentsGenerator PixelFormat(string format)
        {
            if (!string.IsNullOrEmpty(format))
            {
                arguments.Add(VideoCodec.PixelFormat(format));
                return this;
            }
            return this;
        }

        /// <summary>
        /// 画面尺寸，分辨率
        /// </summary>
        /// <param name="scale"></param>
        /// <returns></returns>
        public VideoArgumentsGenerator Scale(string scale)
        {
            if (!string.IsNullOrEmpty(scale))
            {
                arguments.Add(new FFmpegArgumentItem("scale", scale, "vf", ','));
            }
            return this;
        }

        /// <summary>
        /// 编码速度/质量比参数
        /// </summary>
        /// <param name="speed"></param>
        /// <returns></returns>
        public VideoArgumentsGenerator Speed(int? speed)
        {
            if (speed.HasValue)
            {
                arguments.Add(VideoCodec.Speed(speed.Value));
            }
            return this;
        }

        /// <summary>
        /// 帧率
        /// </summary>
        /// <param name="fps"></param>
        /// <returns></returns>
        public VideoArgumentsGenerator FrameRate(double? fps)
        {
            if (fps.HasValue)
            {
                arguments.Add(VideoCodec.FrameRate(fps.Value));
            }
            return this;
        }

        /// <summary>
        /// 针对部分视频编码格式的额外参数
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<FFmpegArgumentItem> ExtraArguments()
        {
            return VideoCodec == null ? Enumerable.Empty<FFmpegArgumentItem>() : VideoCodec.ExtraArguments();
        }
    }
}