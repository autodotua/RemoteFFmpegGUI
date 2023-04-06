namespace SimpleFFmpegGUI.Model
{
    public class MediaInfoItem
    {
        public string Name { get; set; } // 编码设置项的名称
        public object Value { get; set; } // 编码设置项的值
        public MediaInfoItem()
        {

        }
        public MediaInfoItem(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}