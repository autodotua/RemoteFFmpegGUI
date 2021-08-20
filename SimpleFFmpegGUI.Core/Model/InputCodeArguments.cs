using System;

namespace SimpleFFmpegGUI.Model
{
    public class InputCodeArguments
    {
        public TimeSpan? From { get; set; }
        public TimeSpan? To { get; set; }
        public TimeSpan? Duration { get; set; }
    }
}