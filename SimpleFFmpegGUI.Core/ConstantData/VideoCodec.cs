using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFFmpegGUI.ConstantData
{
    public class VideoCodec
    {
        public static readonly VideoCodec H264 = new VideoCodec("H264", "libx264", 8);
        public static readonly VideoCodec H265 = new VideoCodec("H265", "libx265", 8);
        public static readonly VideoCodec VP9 = new VideoCodec("VP9", "libvpx-vp9",5);
        public static readonly VideoCodec AV1 = new VideoCodec("AV1", "libaom-av1",8);
        public static readonly VideoCodec[] VideoCodecs = new VideoCodec[]
        {
            H264,
            H265,
            VP9,
            AV1,
            };

        public VideoCodec()
        {
        }

        public VideoCodec(string name, string lib,int maxSpeedLevel)
        {
            Name = name;
            Lib = lib;
            MaxSpeedLevel = maxSpeedLevel;
        }

        public string Name { get; set; }
        public string Lib { get; set; }
        public int MaxSpeedLevel { get; }
    }
}
