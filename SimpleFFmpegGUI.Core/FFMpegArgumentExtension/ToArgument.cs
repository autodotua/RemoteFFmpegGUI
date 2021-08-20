using FFMpegCore.Arguments;
using System;

namespace SimpleFFmpegGUI.FFMpegArgumentExtension
{
    public class ToArgument : IArgument
    {
        public readonly TimeSpan to;

        public ToArgument(TimeSpan to)
        {
            this.to = to;
        }

        public string Text => $"-to {to}";
    }
}