using FFMpegCore.Arguments;
using FFMpegCore.Enums;
using SimpleFFmpegGUI.ConstantData;
using System;
using VideoCodec = SimpleFFmpegGUI.ConstantData.VideoCodec;

namespace SimpleFFmpegGUI.FFMpegArgumentExtension
{
    public class VideoTwoPassArgument : IArgument
    {
        public readonly string Codec;
        public readonly int Pass;

        public VideoTwoPassArgument(string codec, int pass)
        {
            Codec = codec;
            Pass = pass;
        }

        public string Text
        {
            get
            {
                if (Codec == VideoCodec.H265.Lib)
                {
                    return $"-x265-params pass={Pass}";
                }
                else
                {
                    return $"-pass {Pass}";
                }
            }
        }

    }
}