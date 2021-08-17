using System;

namespace SimpleFFmpegGUI.Dto
{
    public class ProgressDto
    {
        public string Name { get; set; }
        public TimeSpan VideoDuration { get; set; } = TimeSpan.Zero;
        public TimeSpan VideoLength { get; set; }

        public DateTime StartTime { get; set; }
        public TimeSpan PauseTime { get; set; } = TimeSpan.Zero;
        public TimeSpan Duration { get; set; }

        public DateTime FinishTime { get; set; }

        public TimeSpan LastTime { get; set; }
        public double Percent { get; set; }

        public void Update(TimeSpan VideoDuration)
        {
            Percent = VideoDuration.Ticks * 1.0 / VideoLength.Ticks;
            if (Percent > 1)
            {
                Percent = 0.9999;
            }
            var needTime = (DateTime.Now - (StartTime + PauseTime)) / Percent;
            FinishTime = StartTime + PauseTime + needTime;
            LastTime = FinishTime - DateTime.Now;
            Duration = DateTime.Now - StartTime;
        }
    }
}