using SimpleFFmpegGUI.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleFFmpegGUI
{
    public class ExceptionEventArgs : EventArgs
    {
        public ExceptionEventArgs(Exception exception)
        {
            Exception = exception;
        }

        public ExceptionEventArgs(Exception exception, string message)
        {
            Exception = exception;
            Message = message;
        }
        public Exception Exception { get; }
        public string Message { get; }
    }

    public class LogEventArgs : EventArgs
    {
        public LogEventArgs(Log log)
        {
            Log = log;
        }

        public Log Log { get; set; }
    }
    public static class Logger
    {
        static Logger()
        {
            StartTimer();
        }

        public static event EventHandler<LogEventArgs> Log;

        public static event EventHandler<ExceptionEventArgs> LogSaveFailed;

        public static void Error(string message)
        {
            AddLog('E', message);
        }

        public static void Error(TaskInfo task, string message)
        {
            AddLog('E', message, task);
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
            queueLogs.Add(log);
            Debug.WriteLine($"[{type}] {message}");
        }

        private static ConcurrentBag<Log> queueLogs = new ConcurrentBag<Log>();

        public static async Task SaveAllAsync()
        {
            if (queueLogs.IsEmpty)
            {
                return;
            }
            try
            {
                await using var db = new FFmpegDbContext();
                var logs = new Log[queueLogs.Count];
                queueLogs.CopyTo(logs, 0);
                queueLogs.Clear();
                db.Logs.AddRange(logs);
                await db.SaveChangesAsync();
                Debug.WriteLine($"保存了{logs.Length}个日志");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("保存日志失败");
                Debug.WriteLine(ex);
                LogSaveFailed?.Invoke(null, new ExceptionEventArgs(ex, "保存日志失败"));
            }
        }
        public static void SaveAll()
        {
            if (queueLogs.IsEmpty)
            {
                return;
            }
            try
            {
                using var db = new FFmpegDbContext();
                var logs = new Log[queueLogs.Count];
                queueLogs.CopyTo(logs, 0);
                queueLogs.Clear();
                db.Logs.AddRange(logs);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("保存日志失败");
                Debug.WriteLine(ex);
                LogSaveFailed?.Invoke(null, new ExceptionEventArgs(ex, "保存日志失败"));
            }
        }

        private static async void StartTimer()
        {
            PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
            while (await timer.WaitForNextTickAsync())
            {
                await SaveAllAsync();
            }
        }
    }
}