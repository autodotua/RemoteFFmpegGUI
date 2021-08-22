using SimpleFFmpegGUI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleFFmpegGUI.WebAPI.Dto
{
    public class TaskDto
    {
        public List<InputArguments> Inputs { get; set; }
        public string Output { get; set; }
        public OutputArguments Argument { get; set; }
        public bool Start { get; set; }
    }
}