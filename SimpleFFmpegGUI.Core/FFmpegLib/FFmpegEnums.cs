namespace SimpleFFmpegGUI.FFmpegLib
{
    public class FFmpegEnums
    {
        public readonly static string[] Presets = new[] {
            "veryslow",
            "slower",
            "slow",
            "medium",
            "fast",
            "faster",
            "veryfast",
            "superfast",
            "ultrafast",
        };

        public readonly static string[] PixelFormats = new[] {
            "yuv420p",
            "yuvj420p",
            "yuv422p",
            "yuvj422p",
            "rgb24",
            "gray",
            "yuv420p10le"
        };
    }

}
