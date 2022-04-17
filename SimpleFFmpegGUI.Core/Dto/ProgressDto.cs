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

        public void Update(TimeSpan VideoDuration)
        {
            if (VideoLength.HasValue)
            {
                Percent = VideoDuration.Ticks * 1.0 / VideoLength.Value.Ticks;
                if (Percent >= 1)
                {
                    Percent = 1;
                }
                var totalTime = (DateTime.Now - (StartTime + PauseTime)) / Percent;
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