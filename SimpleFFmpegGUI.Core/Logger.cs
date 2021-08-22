using SimpleFFmpegGUI.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SimpleFFmpegGUI
{
    public static class Logger
    {
        private static ConsoleColor DefaultColor = Console.ForegroundColor;

        private static string GetMessage(TaskInfo task, string message)
        {
            return $"{message} （{task.Type}  {(task.Inputs == null ? "？" : string.Join('+', task.Inputs))} ===> {task.Output ?? "？"}）";
        }

        public static void Info(TaskInfo task, string message)
        {
            AddLog('I', message, task);
        }

        public static void Info(string message)
        {
            AddLog('I', message);
        }

        public static void Output(TaskInfo task, string message)
        {
            AddLog('O', message, task);
        }

        public static void Error(string message)
        {
            AddLog('E', message);
        }

        public static void Error(TaskInfo task, string message)
        {
            AddLog('E', message, task);
        }

        public static void Warn(string message)
        {
            AddLog('W', message);
        }

        public static void Warn(TaskInfo task, string message)
        {
            AddLog('W', message, task);
        }

        private static void AddLog(char type, string message, TaskInfo task = null)
        {
            Log log = new Log()
            {
                Time = DateTime.Now,
                Type = type,
                Message = message,
                TaskId = task?.Id
            };
            Log?.Invoke(null, new LogEventArgs(log));
            using var db = FFmpegDbContext.GetNew();
            db.Logs.Add(log);
            db.SaveChanges();
        }

        public static event EventHandler<LogEventArgs> Log;
    }

    public class LogEventArgs : EventArgs
    {
        public LogEventArgs(Log log)
        {
            Log = log;
        }

        public Log Log { get; set; }
    }
}