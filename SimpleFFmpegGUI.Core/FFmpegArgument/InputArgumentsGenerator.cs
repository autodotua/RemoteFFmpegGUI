using SimpleFFmpegGUI.FFmpegLib;
using System;

namespace SimpleFFmpegGUI.FFmpegArgument
{
    public class InputArgumentsGenerator : ArgumentsGeneratorBase
    {
        public void Duration(TimeSpan? length)
        {
            if (!length.HasValue)
            {
                return;
            }
            arguments.Add(new FFmpegArgumentItem("t", length.Value.TotalSeconds.ToString("0.000")));
        }

        public void Format(string format)
        {
            if (format == null)
            {
                return;
            }
            arguments.Add(new FFmpegArgumentItem("f", format));
        }

        public void Framerate(double? fps)
        {
            if (!fps.HasValue)
            {
                return;
            }
            arguments.Add(new FFmpegArgumentItem("framerate", fps.ToString()));
        }
     
        public void Input(string file)
        {
            if (file == null)
            {
                return;
            }
            arguments.Add(new FFmpegArgumentItem("i", $"\"{file}\""));
        }

        public void Seek(TimeSpan? seek)
        {
            if (!seek.HasValue)
            {
                return;
            }
            arguments.Add(new FFmpegArgumentItem("ss", seek.Value.TotalSeconds.ToString("0.000")));
        }

        public void To(TimeSpan? to)
        {
            if (!to.HasValue)
            {
                return;
            }
            arguments.Add(new FFmpegArgumentItem("to", to.Value.TotalSeconds.ToString("0.000")));
        }
    }
}
