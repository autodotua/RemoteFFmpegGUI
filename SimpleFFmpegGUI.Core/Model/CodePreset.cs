namespace SimpleFFmpegGUI.Model
{
    public class CodePreset : ModelBase
    {
        public string Name { get; set; }
        public TaskType Type { get; set; }
        public OutputArguments Arguments { get; set; }
        public bool Default { get; set; }
    }
}