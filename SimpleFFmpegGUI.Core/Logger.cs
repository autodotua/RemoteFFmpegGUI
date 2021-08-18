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
            return $"{message} （{task.Type}  {string.Join('+', task.Inputs)} ===> {task.Output}）";
        }

        public static void Info(TaskInfo task, string message)
        {
            Info(GetMessage(task, message));
        }

        public static void Info(string message)
        {
            Console.ForegroundColor = DefaultColor;
            AddLog('I', message);
        }

        public static void Output(string message)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            AddLog('O', message);
        }

        public static void Error(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            AddLog('E', message);
        }

        public static void Error(TaskInfo task, string message)
        {
            Error(GetMessage(task, message));
        }

        public static void Warn(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            AddLog('W', message);
        }

        public static void Warn(TaskInfo task, string message)
        {
            Warn(GetMessage(task, message));
        }

        private static void AddLog(char type, string message)
        {
            Log log = new Log()
            {
                Time = DateTime.Now,
                Type = type,
                Message = message
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