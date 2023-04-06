using System.Collections.Generic;

namespace SimpleFFmpegGUI.Model
{
    public class MediaInfoStream : List<MediaInfoItem>
    {
        // 流类型的名称
        public string Name { get; set; }

        // 构造函数，接受流类型的名称作为参数
        public MediaInfoStream(string name)
        {
            Name = name;
        }
    }
}