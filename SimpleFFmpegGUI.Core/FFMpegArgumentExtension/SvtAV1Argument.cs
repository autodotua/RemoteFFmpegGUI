using FFMpegCore.Arguments;
using System.Collections.Generic;
using System.Text;

namespace SimpleFFmpegGUI.FFMpegArgumentExtension
{
    public class SvtAV1Argument : IArgument
    {
        public SvtAV1Argument(double? mbitrate, double? fps, bool twoPass, int? width, int? height)
        {
            Mbitrate = mbitrate;
            Fps = fps;
            TwoPass = twoPass;
            Width = width;
            Height = height;
        }
        public double? Mbitrate { get; }
        public double? Fps { get; }
        public bool TwoPass { get; }
        public int? Width { get; }
        public int? Height { get; }

        public string Text
        {
            get
            {
                List<string> list = new List<string>();
                if (Mbitrate.HasValue)
                {
                    list.Add("rc=1");
                    list.Add($"tbr={(int)(Mbitrate.Value * 1000)}");
                }
                if (Fps.HasValue)
                {
                    list.Add($"fps={Fps.Value}");
                }
                if (TwoPass)
                {
                    list.Add("passes=2");
                }
                if (Width.HasValue)
                {
                    list.Add($"w={Width.Value}");
                }
                if (Height.HasValue)
                {
                    list.Add($"h={Height.Value}");
                }
                return list.Count == 0 ? "" : $"-svtav1-params {string.Join(":", list)}";
            }
        }
    }
}