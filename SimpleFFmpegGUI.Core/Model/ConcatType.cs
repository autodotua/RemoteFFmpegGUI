using System.ComponentModel;

namespace SimpleFFmpegGUI.Model
{
    public enum ConcatType
    {
        [Description("经过TS格式中转")]
        ViaTs = 0,

        [Description("使用Concat格式")]
        ConcatFormat = 1
    }
}