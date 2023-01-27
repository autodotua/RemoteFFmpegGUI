namespace SimpleFFmpegGUI.FFmpegLib
{
    public class FFmpegArgumentItem
    {
        public FFmpegArgumentItem(string key)
        {
            Key = key;
        }

        public FFmpegArgumentItem(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public FFmpegArgumentItem(string key, string value, string parent,char seprator) : this(key, value)
        {
            Parent = parent;
            Seprator = seprator;
        }

        public string Key { get; set; }
        public string Value { get; set; }
        public string Parent { get; set; }
        public char Seprator { get; }
    }

}
