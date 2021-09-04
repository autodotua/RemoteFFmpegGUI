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
using System.Collections.Specialized;
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
        private bool running = false;

        public event NotifyCollectionChangedEventHandler TaskManagersChanged;

        public QueueManager()
        {
        }

        private IQueryable<TaskInfo> GetQueueTasks(FFmpegDbContext db)
        {
            return db.Tasks
                .Where(p => p.Status == TaskStatus.Queue)
                .Where(p => !p.IsDeleted);
        }

        public async void StartStandalone(int id)
        {
            using FFmpegDbContext db = FFmpegDbContext.GetNew();
            var task = db.Tasks.Find(id);
            if (task == null)
            {
                throw new Exception("找不到ID为" + id + "的任务");
            }
            if (task.Status != TaskStatus.Queue)
            {
                throw new Exception("任务的状态不正确，不可开始任务");
            }
            if (Tasks.Any(p => p.Id == task.Id))
            {
                throw new Exception("任务正在进行中，但状态不是正在处理中");
            }
            Logger.Info(task, "开始独立任务");
            await ProcessTaskAsync(db, task, false);
            Logger.Info(task, "独立任务完成");
        }

        public async void StartQueue()
        {
            if (running)
            {
                Logger.Warn("队列正在运行，开始队列失败");
                return;
            }
            running = true;
            Logger.Info("开始队列");
            using FFmpegDbContext db = FFmpegDbContext.GetNew();
            List<TaskInfo> tasks;
            while (!cancelQueue && GetQueueTasks(db).Any())
            {
                tasks = GetQueueTasks(db).OrderBy(p => p.CreateTime).ToList();

                var task = tasks[0];

                await ProcessTaskAsync(db, task, true);
            }
            running = false;
            cancelQueue = false;
            Logger.Info("队列完成");
        }

        private async Task ProcessTaskAsync(FFmpegDbContext db, TaskInfo task, bool main)
        {
            FFmpegManager ffmpegManager = new FFmpegManager(task);

            task.Status = TaskStatus.Processing;
            task.StartTime = DateTime.Now;
            task.Message = "";
            task.FFmpegArguments = "";
            db.Update(task);
            db.SaveChanges();
            AddManager(task, ffmpegManager, main);
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
            }
            db.Update(task);
            db.SaveChanges();
            RemoveManager(task, ffmpegManager, main);
        }

        private void AddManager(TaskInfo task, FFmpegManager ffmpegManager, bool main)
        {
            taskProcessManagers.Add(ffmpegManager);
            if (main)
            {
                MainQueueTask = task;
            }
            TaskManagersChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, ffmpegManager));
        }

        private void RemoveManager(TaskInfo task, FFmpegManager ffmpegManager, bool main)
        {
            if (!taskProcessManagers.Remove(ffmpegManager))
            {
                throw new Exception("管理器未在管理器集合中");
            }
            if (main)
            {
                MainQueueTask = null;
            }
            TaskManagersChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, ffmpegManager));
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

        private void CheckMainQueueProcessingTaskManager()
        {
            if (!Managers.Any(p => p.Task == MainQueueTask))
            {
                throw new Exception("主队列未运行或当前任务正在准备中");
            }
        }
    }
}