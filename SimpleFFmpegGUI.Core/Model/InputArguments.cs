using System;

namespace SimpleFFmpegGUI.Model
{
    public class InputArguments
    {
        public string FilePath { get; set; }
        public TimeSpan? From { get; set; }
        public TimeSpan? To { get; set; }
        public TimeSpan? Duration { get; set; }
    }
}