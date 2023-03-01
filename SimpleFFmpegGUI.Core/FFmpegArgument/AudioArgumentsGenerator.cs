using SimpleFFmpegGUI.FFmpegLib;
using AudioCodec = SimpleFFmpegGUI.FFmpegLib.AudioCodec;

namespace SimpleFFmpegGUI.FFmpegArgument
{
    public class AudioArgumentsGenerator : ArgumentsGeneratorBase
    {
        /// <summary>
        /// 编码
        /// </summary>
        private AudioCodec audioCodec;

        /// <summary>
        /// 码率
        /// </summary>
        /// <param name="kb"></param>
        /// <returns></returns>
        public AudioArgumentsGenerator Bitrate(double? kb)
        {
            if (kb.HasValue)
            {
                arguments.Add(audioCodec.Bitrate(kb.Value));
            }
            return this;
        }

        /// <summary>
        /// 编码
        /// </summary>
        /// <param name="codec"></param>
        /// <returns></returns>
        public AudioArgumentsGenerator Codec(string codec)
        {
            codec = codec.ToLower();
            foreach (var c in AudioCodec.AudioCodecs)
            {
                if (c.Name.ToLower() == codec || c.Lib.ToLower() == codec)
                {
                    audioCodec = c;
                    arguments.Add(new FFmpegArgumentItem("c:a", c.Lib));
                    return this;
                }
            }
            audioCodec = new GeneralAudioCodec();
            if (codec is not ("自动" or "auto") && !string.IsNullOrEmpty(codec))
            {
                arguments.Add(new FFmpegArgumentItem("c:a", codec));
            }
            return this;
        }

        /// <summary>
        /// 复制音频流
        /// </summary>
        /// <returns></returns>
        public AudioArgumentsGenerator Copy()
        {
            arguments.Add(new FFmpegArgumentItem("c:a", "copy"));
            return this;
        }

        /// <summary>
        /// 禁用音频流
        /// </summary>
        /// <returns></returns>
        public AudioArgumentsGenerator Disable()
        {
            arguments.Add(new FFmpegArgumentItem("an"));
            return this;
        }

        public AudioArgumentsGenerator SamplingRate(int? hz)
        {
            if (hz.HasValue)
            {
                arguments.Add(audioCodec.SamplingRate(hz.Value));
            }
            return this;
        }
    }
}