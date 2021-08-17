using System;

namespace SimpleFFmpegGUI.Model
{
    public class VideoFormat
    {
        public static readonly VideoFormat[] Formats = new[]
        {
            new VideoFormat("matroska","mkv",main:true),
            new VideoFormat("mp4","mp4",main:true),
            new VideoFormat("mov","mov",main:true),
            new VideoFormat("mpegts","ts"),
            new VideoFormat("avi","avi",main:true),
            new VideoFormat("webm","webm",main:true),
            new VideoFormat("ogv","ogv"),
            new VideoFormat("mp3","mp3",true,true),
            new VideoFormat("aac","aac",true,true),
            new VideoFormat("ac3","ac3"),
            new VideoFormat("av1","av1",main:true),
            new VideoFormat("wav","wav"),
            new VideoFormat("mp2","mp2"),
            new VideoFormat("vob","vob"),
        };

        public VideoFormat()
        {
        }

        public VideoFormat(string name, string extension, bool audioOnly = false, bool main = false)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Extension = extension ?? throw new ArgumentNullException(nameof(extension));
            AudioOnly = audioOnly;
            Main = main;
        }

        public string Name { get; set; }
        public string Extension { get; set; }
        public bool AudioOnly { get; }
        public bool Main { get; set; }
    }
}