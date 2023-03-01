using System;

namespace SimpleFFmpegGUI.Dto
{
    /// <summary>
    /// 进度信息
    /// </summary>
    public class ProgressDto
    {
        /// <summary>
        /// 基础百分比，用于二压时，第二遍在在百分比的基础上加上0.5
        /// </summary>
        public double BasePercent { get; set; } = 0;

        /// <summary>
        /// 已经花费的时间
        /// </summary>
        public TimeSpan Duration { get; set; }

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
        /// 更新进度
        /// </summary>
        /// <param name="VideoDuration"></param>
        public void Update(TimeSpan VideoDuration)
        {
            if (VideoLength.HasValue)
            {
                Percent = VideoDuration.Ticks * 1.0 / VideoLength.Value.Ticks+ BasePercent;
                if (Percent >= 1)
                {
                    Percent = 1;
                }
                var totalTime = Percent==0?TimeSpan.Zero:(DateTime.Now - (StartTime + PauseTime)) / Percent;
                FinishTime = StartTime + PauseTime + totalTime;
                LastTime = totalTime - (DateTime.Now - (StartTime + PauseTime));
                Duration = DateTime.Now - StartTime;
            }
            else
            {
                IsIndeterminate = true;
            }
        }
    }
}