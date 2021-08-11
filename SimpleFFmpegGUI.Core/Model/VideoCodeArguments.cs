namespace SimpleFFmpegGUI.Model
{
    public class VideoCodeArguments
    {
        public string Code { get; set; }
        public int Preset { get; set; }
        public int? Crf { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public double? Fps { get; set; }
        public double? AverageBitrate { get; set; }
        public double? MaxBitrate { get; set; }
        public double? MaxBitrateBuffer { get; set; }
    }
}