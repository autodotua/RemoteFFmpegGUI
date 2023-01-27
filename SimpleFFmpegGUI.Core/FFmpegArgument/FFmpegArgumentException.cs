using System;

namespace SimpleFFmpegGUI.FFmpegArgument
{
    public class FFmpegArgumentException : Exception
    {
        public FFmpegArgumentException()
        {
        }

        public FFmpegArgumentException(string message) : base(message)
        {
        }

        public FFmpegArgumentException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
