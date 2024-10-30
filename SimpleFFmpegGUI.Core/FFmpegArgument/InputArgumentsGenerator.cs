using SimpleFFmpegGUI.FFmpegLib;
using System;

namespace SimpleFFmpegGUI.FFmpegArgument
{
    public class InputArgumentsGenerator : ArgumentsGeneratorBase
    {
        /// <summary>
        /// 持续时间
        /// </summary>
        /// <param name="length"></param>
        public void Duration(TimeSpan? length)
        {
            if (!length.HasValue)
            {
                return;
            }
            arguments.Add(new FFmpegArgumentItem("t", length.Value.TotalSeconds.ToString("0.000")));
        }

        /// <summary>
        /// 强制输入格式
        /// </summary>
        /// <param name="format"></param>
        public void Format(string format)
        {
            if (format == null)
            {
                return;
            }
            arguments.Add(new FFmpegArgumentItem("f", format));
        }

        /// <summary>
        /// 输入帧率
        /// </summary>
        /// <param name="fps"></param>
        public void Framerate(double? fps)
        {
            if (!fps.HasValue || double.IsNaN(fps.Value))
            {
                return;
            }
            arguments.Add(new FFmpegArgumentItem("framerate", fps.ToString()));
        }

        /// <summary>
        /// 输入文件
        /// </summary>
        /// <param name="file"></param>
        public void Input(string file)
        {
            if (file == null)
            {
                return;
            }
            arguments.Add(new FFmpegArgumentItem("i", $"\"{file}\""));
        }

        /// <summary>
        /// 开始时间
        /// </summary>
        /// <param name="seek"></param>
        public void Seek(TimeSpan? seek)
        {
            if (!seek.HasValue)
            {
                return;
            }
            arguments.Add(new FFmpegArgumentItem("ss", seek.Value.TotalSeconds.ToString("0.000")));
        }

        /// <summary>
        /// 结束时间
        /// </summary>
        /// <param name="to"></param>
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