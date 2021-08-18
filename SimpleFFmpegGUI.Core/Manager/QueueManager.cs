using FFMpegCore;
using FFMpegCore.Enums;
using FzLib.IO;
using Instances;
using SimpleFFmpegGUI.Dto;
using SimpleFFmpegGUI.FFMpegArgumentExtension;
using SimpleFFmpegGUI.Manager;
using SimpleFFmpegGUI.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Task = System.Threading.Tasks.Task;
using Tasks = System.Threading.Tasks;

namespace SimpleFFmpegGUI.Manager
{
    public class QueueManager
    {
        private DateTime pauseStartTime;
        private bool paused = false;
        public bool IsRunning => ProcessingTask != null;
        public bool IsPaused => ProcessingTask != null && paused;
        public TaskInfo ProcessingTask { get; private set; }
        public ProgressDto Progress => ffmpeg.Progress;
        private bool cancelQueue = false;
        private CancellationTokenSource cancel;
        private FFmpegManager ffmpeg = new FFmpegManager();

        public QueueManager()
        {
        }

        private IQueryable<TaskInfo> GetQueueTasks(FFmpegDbContext db)
        {
            return db.Tasks
                .Where(p => p.Status == TaskStatus.Queue)
                .Where(p => !p.IsDeleted);
        }

        public async void StartQueue()
        {
            if (ProcessingTask != null)
            {
                Logger.Warn("队列正在运行，开始队列失败");
                return;
            }
            Logger.Info("开始队列");
            FFmpegDbContext db = FFmpegDbContext.Get();
            List<TaskInfo> tasks;
            while (!cancelQueue && GetQueueTasks(db).Any())
            {
                tasks = GetQueueTasks(db).OrderBy(p => p.CreateTime).ToList();

                var task = tasks[0];

                ProcessingTask = task;
                paused = false;
                cancel = new CancellationTokenSource();
                task.Status = TaskStatus.Processing;
                task.StartTime = DateTime.Now;
                try
                {
                    db.SaveChanges();
                    await ffmpeg.StartNewAsync(task, cancel.Token);
                    task.Status = TaskStatus.Done;
                }
                catch (Exception ex)
                {
                    if (task.Status != TaskStatus.Cancel)
                    {
                        Logger.Error(task, "运行错误：" + ex.Message);
                        task.Status = TaskStatus.Error;
                        task.Message = ex.Message;
                    }
                    else
                    {
                        Logger.Warn(task, "任务被取消");
                    }
                }
                finally
                {
                    task.FinishTime = DateTime.Now;
                    ProcessingTask = null;
                }
                db.Update(task);
                db.SaveChanges();
            }
            cancelQueue = false;
            Logger.Info("队列完成");
        }


        public void Suspend()
        {
            if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                throw new PlatformNotSupportedException("暂停和恢复功能仅支持Windows");
            }
            if (ProcessingTask == null)
            {
                return;
            }
            paused = true;
            Logger.Info(ProcessingTask, "暂停队列");
            pauseStartTime = DateTime.Now;
            ProcessExtension.SuspendProcess(GetFFmpegProcess().Id);
        }

        public void Resume()
        {
            if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                throw new PlatformNotSupportedException("暂停和恢复功能仅支持Windows");
            }
            if (ProcessingTask == null)
            {
                return;
            }
            paused = false;
            Progress.PauseTime = DateTime.Now - pauseStartTime;
            Logger.Info(ProcessingTask, "恢复队列");
            ProcessExtension.ResumeProcess(GetFFmpegProcess().Id);
        }

        public void Cancel()
        {
            Logger.Info("取消队列");
            ProcessingTask.Status = TaskStatus.Cancel;
            cancelQueue = true;
            cancel.Cancel();
        }

        public void CancelCurrent()
        {
            Logger.Info(ProcessingTask, "取消当前任务");
            ProcessingTask.Status = TaskStatus.Cancel;
            GetFFmpegProcess().Kill();
        }

        public event EventHandler<FFmpegOutputEventArgs> FFmpegOutput
        {
            add => ffmpeg.FFmpegOutput += value;
            remove => ffmpeg.FFmpegOutput -= value;
        }

        private Process GetFFmpegProcess()
        {
            Process current = Process.GetCurrentProcess();
            var ps = Process.GetProcessesByName("ffmpeg")
                .Where(p => Path.GetDirectoryName(p.MainModule.FileName)
                == Path.GetDirectoryName(current.MainModule.FileName))
                .Where(p => p.StartTime > current.StartTime);
            if (ps.Count() != 1)
            {
                throw new Exception("存在多个或不存在FFmpeg进程");
            }
            return ps.First();
        }

    }
}