using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleFFmpegGUI.Model
{
    public class TaskInfo : ModelBase
    {
        public TaskInfo()
        {
            CreateTime = DateTime.Now;
            Status = TaskStatus.Queue;
        }

        public TaskType Type { get; set; }
        public TaskStatus Status { get; set; }
        public List<InputArguments> Inputs { get; set; }
        public string Output { get; set; }
        public OutputArguments Arguments { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? FinishTime { get; set; }
        public string Message { get; set; }
        public string FFmpegArguments { get; set; }
    }
}