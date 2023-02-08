using System;

namespace SimpleFFmpegGUI.Dto
{
    public class ProgressDto
    {
        public string Name { get; set; }
        public TimeSpan VideoDuration { get; set; } = TimeSpan.Zero;
        public TimeSpan? VideoLength { get; set; }

        public bool IsIndeterminate { get; set; }

        public DateTime StartTime { get; set; }
        public TimeSpan PauseTime { get; set; } = TimeSpan.Zero;
        public TimeSpan Duration { get; set; }

        public DateTime FinishTime { get; set; }

        public TimeSpan LastTime { get; set; }
        public double Percent { get; set; }
        /// <summary>
        /// 基础百分比，用于二压时，第二遍在在百分比的基础上加上0.5
        /// </summary>
        public double BasePercent { get; set; } = 0;

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