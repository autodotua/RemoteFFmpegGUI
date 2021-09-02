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
        private List<FFmpegManager> taskProcessManagers = new List<FFmpegManager>();

        public TaskInfo MainQueueTask { get; private set; }
        public FFmpegManager MainQueueManager => Managers.FirstOrDefault(p => p.Task == MainQueueTask);
        public IReadOnlyList<FFmpegManager> Managers => taskProcessManagers.AsReadOnly();
        public IEnumerable<TaskInfo> StandaloneTasks => Managers.Where(p => p.Task != MainQueueTask).Select(p => p.Task);
        public IEnumerable<TaskInfo> Tasks => Managers.Select(p => p.Task);

        private bool cancelQueue = false;

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
            if (MainQueueTask != null)
            {
                Logger.Warn("队列正在运行，开始队列失败");
                return;
            }
            Logger.Info("开始队列");
            using FFmpegDbContext db = FFmpegDbContext.GetNew();
            List<TaskInfo> tasks;
            while (!cancelQueue && GetQueueTasks(db).Any())
            {
                tasks = GetQueueTasks(db).OrderBy(p => p.CreateTime).ToList();

                var task = tasks[0];

                await ProcessTaskAsync(db, task, true);
            }
            cancelQueue = false;
            Logger.Info("队列完成");
        }

        private async Task ProcessTaskAsync(FFmpegDbContext db, TaskInfo task, bool queue)
        {
            FFmpegManager ffmpegManager = new FFmpegManager(task);
            MainQueueTask = task;
            taskProcessManagers.Add(ffmpegManager);

            task.Status = TaskStatus.Processing;
            task.StartTime = DateTime.Now;
            task.Message = "";
            db.Update(task);
            db.SaveChanges();

            try
            {
                await ffmpegManager.RunAsync();
                task.Status = TaskStatus.Done;
            }
            catch (Exception ex)
            {
                if (task.Status != TaskStatus.Cancel)
                {
                    Logger.Error(task, "运行错误：" + ex.ToString());
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
                MainQueueTask = null;
                taskProcessManagers.Remove(ffmpegManager);
            }
            db.Update(task);
            db.SaveChanges();
        }

        public void SuspendMainQueue()
        {
            CheckMainQueueProcessingTaskManager();
            MainQueueManager.Suspend();
        }

        public void ResumeMainQueue()
        {
            CheckMainQueueProcessingTaskManager();
            MainQueueManager.Resume();
        }

        public void Cancel()
        {
            CheckMainQueueProcessingTaskManager();
            cancelQueue = true;

            MainQueueManager.Cancel();
        }

        public void CancelCurrent()
        {
            CheckMainQueueProcessingTaskManager();
            MainQueueManager.Cancel();
        }

        private void CheckMainQueueProcessingTaskManager()
        {
            if (!Managers.Any(p => p.Task == MainQueueTask))
            {
                throw new Exception("主队列未运行或当前任务正在准备中");
            }
        }
    }
}