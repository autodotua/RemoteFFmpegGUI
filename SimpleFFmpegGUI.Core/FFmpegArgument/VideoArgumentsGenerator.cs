using FFMpegCore.Enums;
using MediaInfo.Model;
using SimpleFFmpegGUI.FFmpegLib;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using VideoCodec = SimpleFFmpegGUI.FFmpegLib.VideoCodec;

namespace SimpleFFmpegGUI.FFmpegArgument
{
    public class VideoArgumentsGenerator : ArgumentsGeneratorBase
    {
        double? maxBitrate;
        VideoCodec videoCodec;
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

        public VideoArgumentsGenerator AverageBitrate(double? mb)
        {
            if (mb.HasValue)
            {
                arguments.Add(videoCodec.AverageBitrate(mb.Value));
            }
            return this;
        }

        public VideoArgumentsGenerator BufferRatio(double? ratio)
        {
            if (ratio.HasValue && videoCodec.Name != VideoCodec.SVTAV1.Name)
            {
                if (maxBitrate == null)
                {
                    throw new FFmpegArgumentException("应先设置最大码率，然后设置缓冲比例");
                }
                arguments.Add(videoCodec.BufferSize(ratio.Value * maxBitrate.Value));
            }
            return this;
        }

        public VideoArgumentsGenerator Codec(string codec)
        {
            codec = codec.ToLower();
            foreach (var c in VideoCodec.VideoCodecs)
            {
                if (c.Name.ToLower() == codec || c.Lib.ToLower() == codec)
                {
                    videoCodec = c;
                    arguments.Add(new FFmpegArgumentItem("c:v", c.Lib));
                    return this;
                }
            }
            videoCodec = new GeneralVideoCodec();
            if (codec is not ("自动" or "auto") && !string.IsNullOrEmpty(codec))
            {
                arguments.Add(new FFmpegArgumentItem("c:v", codec));
            }
            return this;
        }

        public VideoArgumentsGenerator Copy()
        {
            arguments.Add(new FFmpegArgumentItem("c:v", "copy"));
            return this;
        }

        public VideoArgumentsGenerator CRF(int? crf)
        {
            if (crf.HasValue)
            {
                arguments.Add(videoCodec.CRF(crf.Value));
            }
            return this;
        }

        public VideoArgumentsGenerator Disable()
        {
            arguments.Add(new FFmpegArgumentItem("vn"));
            return this;
        }

        public VideoArgumentsGenerator MaxBitrate(double? mb)
        {
            if (mb.HasValue)
            {
                maxBitrate = mb;
                arguments.Add(videoCodec.MaxBitrate(mb.Value));
            }
            return this;
        }

        public VideoArgumentsGenerator Pass(int? pass)
        {
            if (!pass.HasValue || pass.Equals(0))
            {
                return this;
            }
            arguments.Add(videoCodec.Pass(pass.Value));
            return this;
        }

        public VideoArgumentsGenerator PixelFormat(string format)
        {
            if (!string.IsNullOrEmpty(format))
            {
                arguments.Add(videoCodec.PixelFormat(format));
                return this;
            }
            return this;
        }

        public VideoArgumentsGenerator Scale(string scale)
        {
            if (!string.IsNullOrEmpty(scale))
            {
                arguments.Add(new FFmpegArgumentItem("scale", scale, "vf", ','));
            }
            return this;
        }

        public VideoArgumentsGenerator Speed(int? speed)
        {
            if (speed.HasValue)
            {
                arguments.Add(videoCodec.Speed(speed.Value));
            }
            return this;
        }

        public VideoArgumentsGenerator FrameRate(double? fps)
        {
            if (fps.HasValue)
            {
                arguments.Add(videoCodec.FrameRate(fps.Value));
            }
            return this;
        }

        public override IEnumerable<FFmpegArgumentItem> ExtraArguments()
        {
            return videoCodec == null ? Enumerable.Empty<FFmpegArgumentItem>() : videoCodec.ExtraArguments();
        }
    }
}
