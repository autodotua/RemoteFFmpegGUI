using FFMpegCore.Arguments;
using System;

namespace SimpleFFmpegGUI.FFMpegArgumentExtension
{
    public class VideoTwoPassArgument : IArgument
    {
        public readonly string Code;
        public readonly int Pass;

        public VideoTwoPassArgument(string code,int pass)
        {
            Code= code;
            Pass= pass;
        }

        public string Text => Code switch
        {
            "libx265" => $"-x265-params pass={Pass}",
            "libx264" => $"-pass {Pass}",
            _ => throw new ArgumentException("不支持2Pass的编码", nameof(Code)),
        };
    }
}