namespace SimpleFFmpegGUI.Dto
{
    public class VideoStreamDto : MediaStreamDto
    {
        public double AvgFrameRate { get; set; }
        public int BitsPerRawSample { get; set; }
        public (int Width, int Height) DisplayAspectRatio { get; set; }
        public string Profile { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public double FrameRate { get; set; }
        public string PixelFormat { get; set; }
        public int Rotation { get; set; }
    }
}