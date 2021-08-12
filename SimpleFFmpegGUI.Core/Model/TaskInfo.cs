using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SimpleFFmpegGUI.Model
{
    public class TaskInfo
    {
        public TaskInfo()
        {
            CreateTime = DateTime.Now;
            Status = TaskStatus.Queue;
        }

        [Key]
        public int ID { get; set; }

        public TaskType Type { get; set; }
        public TaskStatus Status { get; set; }
        public List<string> Inputs { get; set; }
        public string Output { get; set; }
        public CodeArguments Arguments { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? FinishTime { get; set; }
        public string Message { get; set; }
        public bool IsDeleted { get; set; }
    }
}