using SimpleFFmpegGUI.Model;

namespace SimpleFFmpegGUI.WebAPI.Dto
{
    public class CodePresetDto
    {
        public string Name { get; set; }
        public CodeArguments Arguments { get; set; }
        public TaskType Type { get; set; }
    }
}