using FFMpegCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleFFmpegGUI.Dto
{
    public class MediaInfoDto
    {
        public TimeSpan Duration { get; set; }
        public MediaFormat Format { get; set; }
        public List<VideoStreamDto> VideoStreams { get; set; }
        public List<AudioStreamDto> AudioStreams { get; set; }
        public string Detail { get; set; }
    }
}