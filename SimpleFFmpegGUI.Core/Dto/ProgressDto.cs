using System;

namespace SimpleFFmpegGUI.Dto
{
    /// <summary>
    /// 进度信息
    /// </summary>
    public class ProgressDto
    {
        /// <summary>
        /// 基础百分比，用于二压时，第二遍在在百分比的基础上加上一个值
        /// </summary>
        public double BasePercent { get; set; } = 0;

        /// <summary>
        /// 已经花费的时间
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// 已经花费的时间，不包含暂停的时间
        /// </summary>
        public TimeSpan RealDuration { get; set; }

        /// <summary>
        /// 预计结束时间
        /// </summary>
        public DateTime FinishTime { get; set; }

        /// <summary>
        /// 是否无法确定进度
        /// </summary>
        public bool IsIndeterminate { get; set; }

        /// <summary>
        /// 剩余时间
        /// </summary>
        public TimeSpan LastTime { get; set; }

        /// <summary>
        /// 任务名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 暂停的持续时间
        /// </summary>
        public TimeSpan PauseTime { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// 百分比进度
        /// </summary>
        public double Percent { get; set; }

        /// <summary>
        /// 任务开始时间
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 视频持续时间
        /// </summary>
        public TimeSpan VideoDuration { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// 视频长度
        /// </summary>
        public TimeSpan? VideoLength { get; set; }

        /// <summary>
        /// 百分比压缩因子。用于二压时，Pass=1和Pass=2时采用的速度预设不同，因此编码速度有较大差异，
        /// 需要为前后两次Pass分别乘以不同的因子以保证进度条正确
        /// </summary>
        /// <remarks>
        /// 可恶的NewBing，告诉我2Pass时前后两次Preset可以不同，被坑了。
        /// </remarks>
        public double PercentCompressionFactor { get; set; } = 1;

        /// <summary>
        /// 更新进度
        /// </summary>
        /// <param name="VideoDuration"></param>
        public void Update(TimeSpan VideoDuration)
        {
            if (VideoLength.HasValue)
            {
                //百分比=计算百分比*压缩系数+基准百分比，是一个线性的变化
                Percent = VideoDuration.Ticks * 1.0 / VideoLength.Value.Ticks * PercentCompressionFactor + BasePercent;
                if (Percent >= 1)
                {
                    Percent = 1;
                }
                Duration = DateTime.Now - StartTime;
                RealDuration = Duration - PauseTime;
                var totalTime = Percent == 0 ? TimeSpan.Zero : RealDuration / Percent;
                FinishTime = StartTime + PauseTime + totalTime;
                LastTime = totalTime - RealDuration;
                LastTime = LastTime > TimeSpan.Zero ? LastTime : TimeSpan.Zero;
            }
            else
            {
                IsIndeterminate = true;
            }
        }
    }
}