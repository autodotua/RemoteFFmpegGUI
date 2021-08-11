namespace SimpleFFmpegGUI.Model
{
    public class CodeArguments
    {
        public InputCodeArguments Input { get; set; }
        public VideoCodeArguments Video { get; set; }
        public AudioCodeArguments Audio { get; set; }
        public string Extra { get; set; }
    }
}