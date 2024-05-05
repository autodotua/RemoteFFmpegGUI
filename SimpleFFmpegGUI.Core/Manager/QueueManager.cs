using Microsoft.EntityFrameworkCore;
using SimpleFFmpegGUI.FFmpegArgument;
using SimpleFFmpegGUI.Model;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Task = System.Threading.Tasks.Task;
using Tasks = System.Threading.Tasks;

namespace SimpleFFmpegGUI.Manager
{
    public class QueueManager
    {
        private readonly FFmpegDbContext db;
        private readonly Logger logger;
        private bool cancelQueue = false;

        /// <summary>
        /// 用于判断是否为有效计划的队列计划ID
        /// </summary>
        private int currentScheduleID = 0;

        private bool running = false;
        private DateTime? scheduleTime = null;
        private List<FFmpegManager> taskProcessManagers = new List<FFmpegManager>();

        public QueueManager(FFmpegDbContext db, Logger logger)
        {
            this.db = db;
            this.logger = logger;
        }

        /// <summary>
        /// 任务发生改变
        /// </summary>
        public event NotifyCollectionChangedEventHandler TaskManagersChanged;

        /// <summary>
        /// 主队列任务
        /// </summary>
        public FFmpegManager MainQueueManager => Managers.FirstOrDefault(p => p.Task == MainQueueTask);

        /// <summary>
        /// 主队列的Task
        /// </summary>
        public TaskInfo MainQueueTask { get; private set; }

        /// <summary>
        /// 所有任务
        /// </summary>
        public IReadOnlyList<FFmpegManager> Managers => taskProcessManagers.AsReadOnly();

        /// <summary>
        /// 电源性能管理
        /// </summary>
        public PowerManager PowerManager { get; } = new PowerManager();

        /// <summary>
        /// 独立任务
        /// </summary>
        public IEnumerable<TaskInfo> StandaloneTasks => Managers.Where(p => p.Task != MainQueueTask).Select(p => p.Task);

        /// <summary>
        /// 所有任务
        /// </summary>
        public IEnumerable<TaskInfo> Tasks => Managers.Select(p => p.Task);

        /// <summary>
        /// 取消主队列
        /// </summary>
        public void Cancel()
        {
            CheckMainQueueProcessingTaskManager();
            cancelQueue = true;

            MainQueueManager.Cancel();
        }

        /// <summary>
        /// 取消主队列
        /// </summary>
        public Task CancelAsync()
        {
            CheckMainQueueProcessingTaskManager();
            cancelQueue = true;

            return MainQueueManager.CancelAsync();
        }

        /// <summary>
        /// 取消计划的队列
        /// </summary>
        public void CancelQueueSchedule()
        {
            currentScheduleID++;
            scheduleTime = null;
        }

        public DateTime? GetQueueScheduleTime()
        {
            return scheduleTime;
        }

        public void ResumeMainQueue()
        {
            CheckMainQueueProcessingTaskManager();
            MainQueueManager.Resume();
        }

        /// <summary>
        /// 计划一个未来某个时刻开始队列的任务
        /// </summary>
        /// <param name="time"></param>
        /// <exception cref="ArgumentException"></exception>
        public async void ScheduleQueue(DateTime time)
        {
            if (time <= DateTime.Now)
            {
                throw new ArgumentException("计划的时间早于当前时间");
            }
            currentScheduleID++;
            scheduleTime = time;
            int id = currentScheduleID;
            await Task.Delay(time - DateTime.Now);
            if (id == currentScheduleID)
            {
                StartQueue();
            }
        }

        /// <summary>
        /// 开始队列
        /// </summary>
        public async void StartQueue()
        {
            if (running)
            {
                logger.Warn("队列正在运行，开始队列失败");
                return;
            }
            running = true;
            scheduleTime = null;
            logger.Info("开始队列");
            List<TaskInfo> tasks;
            while (!cancelQueue && await GetQueueTasksQuery(db).AnyAsync())
            {
                tasks = await GetQueueTasksQuery(db).OrderBy(p => p.CreateTime).ToListAsync();

                var task = tasks[0];

                await ProcessTaskAsync(task, true);
            }
            running = false;
            bool cancelManually = cancelQueue;
            cancelQueue = false;
            logger.Info("队列完成");
            if (!cancelManually && PowerManager.ShutdownAfterQueueFinished)
            {
                PowerManager.Shutdown();
            }
        }

        /// <summary>
        /// 开始独立任务
        /// </summary>
        /// <param name="id"></param>
        /// <exception cref="Exception"></exception>
        public async void StartStandalone(int id)
        {
            var task = db.Tasks.Find(id) ?? throw new Exception("找不到ID为" + id + "的任务");
            if (task.Status != TaskStatus.Queue)
            {
                throw new Exception("任务的状态不正确，不可开始任务");
            }
            if (Tasks.Any(p => p.Id == task.Id))
            {
                throw new Exception("任务正在进行中，但状态不是正在处理中");
            }
            logger.Info(task, "开始独立任务");
            await ProcessTaskAsync(task, false);
            logger.Info(task, "独立任务完成");
        }

        /// <summary>
        /// 暂停主任务
        /// </summary>
        public void SuspendMainQueue()
        {
            CheckMainQueueProcessingTaskManager();
            MainQueueManager.Suspend();
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

        private void CheckMainQueueProcessingTaskManager()
        {
            if (!Managers.Any(p => p.Task == MainQueueTask))
            {
                throw new Exception("主队列未运行或当前任务正在准备中");
            }
        }

        private IQueryable<TaskInfo> GetQueueTasksQuery(FFmpegDbContext db)
        {
            return db.Tasks
                .Where(p => p.Status == TaskStatus.Queue)
                .Where(p => !p.IsDeleted);
        }

        private async Task ProcessTaskAsync(TaskInfo task, bool main)
        {
            FFmpegManager ffmpegManager = new FFmpegManager(task);

            task.Status = TaskStatus.Processing;
            task.StartTime = DateTime.Now;
            task.Message = "";
            task.FFmpegArguments = "";
            db.Update(task);
            await db.SaveChangesAsync();
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
                    logger.Error(task, "运行错误：" + ex.ToString());
                    task.Status = TaskStatus.Error;
                    task.Message = ex is FFmpegArgumentException ?
                        ex.Message : await ffmpegManager.GetErrorMessageAsync() ?? "运行错误，请查看日志";
                }
                else
                {
                    logger.Warn(task, "任务被取消");
                }
            }
            finally
            {
                task.FinishTime = DateTime.Now;
            }
            db.Update(task);
            await db.SaveChangesAsync();
            RemoveManager(task, ffmpegManager, main);
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
    }
}