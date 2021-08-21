namespace SimpleFFmpegGUI.Model
{
    public enum TaskType
    {
        /// <summary>
        /// 转码
        /// </summary>
        Code = 0,

        /// <summary>
        /// 合并视音频
        /// </summary>
        Combine = 1,

        /// <summary>
        /// 视频比较
        /// </summary>
        Compare = 2,

        /// <summary>
        /// 自定义参数
        /// </summary>
        Custom = 3
    }
}