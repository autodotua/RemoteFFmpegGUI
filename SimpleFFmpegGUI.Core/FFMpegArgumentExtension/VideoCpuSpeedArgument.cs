using FFMpegCore.Arguments;
using FFMpegCore.Enums;
using SimpleFFmpegGUI.ConstantData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoCodec = SimpleFFmpegGUI.ConstantData.VideoCodec;

namespace SimpleFFmpegGUI.FFMpegArgumentExtension
{
    public class VideoCpuSpeedArgument : IArgument
    {
        public readonly string Codec;
        public readonly int Level;

        public VideoCpuSpeedArgument(string codec, int level)
        {
            Codec = codec;
            Level = level;
        }

        public string Text
        {
            get
            {
                if (Codec == VideoCodec.AV1.Lib || Codec == VideoCodec.VP9.Lib)
                {
                    return $"-cpu-used {Level}";
                }
                if (Codec == VideoCodec.AV1SVT.Lib)
                {
                    return $"-preset {Level}";
                }
                return new SpeedPresetArgument((Speed)Level).Text;
            }
        }
    }
}
