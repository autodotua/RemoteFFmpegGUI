namespace SimpleFFmpegGUI.FFmpegLib
{
    public abstract class CodecBase
    {
        /// <summary>
        /// 编码名称
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// 编码在FFmpeg中的库名称
        /// </summary>
        public abstract string Lib { get; }
    }

}
