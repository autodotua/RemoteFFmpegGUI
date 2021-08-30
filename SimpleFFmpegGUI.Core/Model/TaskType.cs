using System.ComponentModel;

namespace SimpleFFmpegGUI.Model
{
    public enum TaskType
    {
        [Description("转码")]
        /// <summary>
        /// 转码
        /// </summary>
        Code = 0,

        [Description("合并视音频")]
        /// <summary>
        /// 合并视音频
        /// </summary>
        Combine = 1,

        [Description("视频比较")]
        /// <summary>
        /// 视频比较
        /// </summary>
        Compare = 2,

        [Description("自定义参数")]
        /// <summary>
        /// 自定义参数
        /// </summary>
        Custom = 3,
        [Description("视频拼接")]

        /// <summary>
        /// 视频拼接
        /// </summary>
        Concat = 4
    }
}