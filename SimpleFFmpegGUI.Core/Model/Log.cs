using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleFFmpegGUI.Model
{
    public class Log : ModelBase
    {
        public Log()
        {
        }

        public DateTime Time { get; set; }
        public char Type { get; set; }
        public string Message { get; set; }
    }
}