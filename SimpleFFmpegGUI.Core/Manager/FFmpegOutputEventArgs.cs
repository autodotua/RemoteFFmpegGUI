namespace SimpleFFmpegGUI.Manager
{
    public class FFmpegOutputEventArgs
    {
        public FFmpegOutputEventArgs()
        {
        }

        public FFmpegOutputEventArgs(bool isError, string data)
        {
            IsError = isError;
            Data = data;
        }

        public bool IsError { get; set; }
        public string Data { get; set; }
    }
}