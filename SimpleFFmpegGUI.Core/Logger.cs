using SimpleFFmpegGUI.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SimpleFFmpegGUI
{
    public static class Logger
    {
        private static bool needSave = false;
        private static Timer timer = null;

        static Logger()
        {
            timer = new Timer(o =>
           {
               if (needSave)
               {
                   FFmpegDbContext.Get().SaveChanges();
               }
           }, null, 10000, 10000);
        }

        private static ConsoleColor DefaultColor = Console.ForegroundColor;

        private static string GetMessage(TaskInfo task, string message)
        {
            return $"{message}：{task.Type}  {string.Join('+', task.Inputs)} ===> {task.Output}";
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
            FFmpegDbContext.Get().Logs.Add(log);
            needSave = true;
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