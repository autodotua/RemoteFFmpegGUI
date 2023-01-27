using SimpleFFmpegGUI.FFmpegLib;
using AudioCodec = SimpleFFmpegGUI.FFmpegLib.AudioCodec;

namespace SimpleFFmpegGUI.FFmpegArgument
{
    public class AudioArgumentsGenerator : ArgumentsGeneratorBase
    {
        AudioCodec audioCodec;
        public AudioArgumentsGenerator Bitrate(double? kb)
        {
            if (kb.HasValue)
            {
                arguments.Add(audioCodec.Bitrate(kb.Value));
            }
            return this;
        }

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

        public AudioArgumentsGenerator Copy()
        {
            arguments.Add(new FFmpegArgumentItem("c:a", "copy"));
            return this;
        }

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
