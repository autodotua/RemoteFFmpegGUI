namespace SimpleFFmpegGUI.Model
{
    public class CodeArguments
    {
        public InputCodeArguments Input { get; set; }
        public VideoCodeArguments Video { get; set; }
        public AudioCodeArguments Audio { get; set; }
        public string Extra { get; set; }
        public bool DisableVideo { get; set; }
        public bool DisableAudio { get; set; }
        public string Format { get; set; }
        public CombineArguments Combine { get; set; }
    }

    public class CombineArguments
    {
        public bool Shortest  { get; set; }
    }
}