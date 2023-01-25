using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFFmpegGUI.ConstantData
{
    public class VideoCodec
    {
        public static readonly VideoCodec H264 = new VideoCodec("H264", "libx264", 8, 3, 23);
        public static readonly VideoCodec H265 = new VideoCodec("H265", "libx265", 8, 3, 28);
        public static readonly VideoCodec VP9 = new VideoCodec("VP9", "libvpx-vp9", 5, 3, 30);
        public static readonly VideoCodec AV1 = new VideoCodec("AV1 (aom)", "libaom-av1", 8, 5, 28);
        public static readonly VideoCodec AV1SVT = new VideoCodec("AV1 (SVT)", "libsvtav1", 12, 6, 28);
        public static readonly VideoCodec[] VideoCodecs = new VideoCodec[]
        {
            H264,
            H265,
            VP9,
            AV1,
            AV1SVT
            };

        public VideoCodec()
        {
        }

        public VideoCodec(string name, string lib, int maxSpeedLevel, int defaultSpeedLevel, int defaultCRF)
        {
            Name = name;
            Lib = lib;
            MaxSpeedLevel = maxSpeedLevel;
            DefaultSpeedLevel = defaultSpeedLevel;
            DefaultCRF = defaultCRF;
        }

        public string Name { get; }
        public string Lib { get; }
        public int MaxSpeedLevel { get; }
        public int DefaultSpeedLevel { get; }
        public int DefaultCRF { get; }
    }
}
