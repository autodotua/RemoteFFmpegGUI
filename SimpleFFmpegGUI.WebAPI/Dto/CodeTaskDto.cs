using SimpleFFmpegGUI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleFFmpegGUI.WebAPI.Dto
{
    public class CodeTaskDto
    {
        public IEnumerable<string> Input { get; set; }
        public string Output { get; set; }
        public CodeArguments Argument { get; set; }
        public bool Start { get; set; }
    }
}