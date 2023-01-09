using SimpleFFmpegGUI.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleFFmpegGUI
{
    public class LogEventArgs : EventArgs
    {
        public LogEventArgs(Log log)
        {
            Log = log;
        }

        public Log Log { get; set; }
    }

    public class Logger : IDisposable
    {
        private static HashSet<Logger> allLoggers = new HashSet<Logger>();
        private FFmpegDbContext db = FFmpegDbContext.GetNew();
        private bool disposed = false;
        private Timer timer;
        public Logger()
        {
            StartTimer();
            allLoggers.Add(this);
        }

        ~Logger()
        {
            Dispose();
        }

        public static event EventHandler<LogEventArgs> Log;

        public static void SaveAll()
        {
            foreach (var logger in allLoggers)
            {
                logger.Save();
            }
        }

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                if (allLoggers.Contains(this))
                {
                    allLoggers.Remove(this);
                }
                timer.Dispose();
                Save();
                db.Dispose();
            }
        }

        public void Error(string message)
        {
            AddLog('E', message);
        }

        public void Error(TaskInfo task, string message)
        {
            AddLog('E', message, task);
        }

        public void Info(TaskInfo task, string message)
        {
            AddLog('I', message, task);
        }

        public void Info(string message)
        {
            AddLog('I', message);
        }

        public void Output(TaskInfo task, string message)
        {
            AddLog('O', message, task);
        }

        public void Warn(string message)
        {
            AddLog('W', message);
        }

        public void Warn(TaskInfo task, string message)
        {
            AddLog('W', message, task);
        }

        private void AddLog(char type, string message, TaskInfo task = null)
        {
            Log log = new Log()
            {
                Time = DateTime.Now,
                Type = type,
                Message = message,
                TaskId = task?.Id
            };
            Log?.Invoke(null, new LogEventArgs(log));
            db.Logs.Add(log);
        }

        private void Save()
        {
            if (db.ChangeTracker.HasChanges())
            {
                db.SaveChanges();
            }
        }
        private void StartTimer()
        {
            timer = new Timer(new TimerCallback(o =>
            {
                Save();
            }), null, 10000, 10000);
        }
    }
}