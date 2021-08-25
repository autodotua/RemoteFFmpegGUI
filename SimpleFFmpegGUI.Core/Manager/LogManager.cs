using SimpleFFmpegGUI.Dto;
using SimpleFFmpegGUI.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleFFmpegGUI.Manager
{
    public static class LogManager
    {
        public static PagedListDto<Log> GetLogs(char? type = null, 
            int taskId=0,
            DateTime? from = null, 
            DateTime? to = null,
            int skip = 0, 
            int take = 0)
        {
            using var db = FFmpegDbContext.GetNew();
            IQueryable<Log> logs = db.Logs;
            if (type.HasValue)
            {
                logs = logs.Where(p => p.Type == type.Value);
            }
            if (from.HasValue)
            {
                logs = logs.Where(p => p.Time > from.Value);
            }
            if (to.HasValue)
            {
                logs = logs.Where(p => p.Time < to.Value);
            }
            if(taskId!=0)
            {
                logs = logs.Where(p => p.TaskId == taskId);
            }
            logs = logs.OrderByDescending(p => p.Time);
            int count = logs.Count();
            if (skip > 0)
            {
                logs = logs.Skip(skip);
            }
            if (take > 0)
            {
                logs = logs.Take(take);
            }
            return new PagedListDto<Log>(logs, count);
        }
    }
}