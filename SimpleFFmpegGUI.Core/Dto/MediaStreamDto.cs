using System;
using System.Collections.Generic;

namespace SimpleFFmpegGUI.Dto
{
    public class MediaStreamDto
    {
        public int Index { get; set; }
        public string CodecName { get; set; }
        public string CodecLongName { get; set; }
        public int BitRate { get; set; }
        public TimeSpan Duration { get; set; }
        public string Language { get; set; }
        public Dictionary<string, string> Tags { get; set; }
    }

}