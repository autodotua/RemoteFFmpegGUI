using SimpleFFmpegGUI.FFmpegArgument;

namespace SimpleFFmpegGUI.FFmpegLib
{
    public abstract class AudioCodec : CodecBase
    {
        public static readonly AudioCodec[] AudioCodecs = new AudioCodec[]
        {
            new AAC(),
            new OPUS()
        };

        /// <summary>
        /// 码率
        /// </summary>
        /// <param name="kb"></param>
        /// <returns></returns>
        /// <exception cref="FFmpegArgumentException"></exception>
        public virtual FFmpegArgumentItem Bitrate(double kb)
        {
            if (kb < 0)
            {
                throw new FFmpegArgumentException("码率超出范围");
            }
            return new FFmpegArgumentItem("b:a", $"{kb}K");
        }

        /// <summary>
        /// 采样率
        /// </summary>
        /// <param name="hz"></param>
        /// <returns></returns>
        /// <exception cref="FFmpegArgumentException"></exception>
        public virtual FFmpegArgumentItem SamplingRate(int hz)
        {
            if (hz < 9600)
            {
                throw new FFmpegArgumentException("采样率超出范围");
            }
            return new FFmpegArgumentItem("ar", hz.ToString());
        }
    }

}
