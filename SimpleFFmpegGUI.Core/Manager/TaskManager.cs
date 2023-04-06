using SimpleFFmpegGUI.Dto;
using SimpleFFmpegGUI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskStatus = SimpleFFmpegGUI.Model.TaskStatus;

namespace SimpleFFmpegGUI.Manager
{
    public static class TaskManager
    {
        static TaskManager()
        {
            using var db = FFmpegDbContext.GetNew();
            foreach (var item in db.Tasks.Where(p => p.Status == TaskStatus.Processing))
            {
                item.Status = TaskStatus.Error;
                item.Message = "状态异常：启动时处于正在运行状态";
            }
            db.SaveChanges();
        }

        public static TaskInfo AddTask(TaskType type, List<InputArguments> path, string outputPath, OutputArguments arg)
        {
            using FFmpegDbContext db = FFmpegDbContext.GetNew();
            var task = new TaskInfo()
            {
                Type = type,
                Inputs = path,
                Output = outputPath,
                Arguments = arg
            };
            db.Tasks.Add(task);
            db.SaveChanges();
            using Logger logger = new Logger();
            logger.Info(task, "新建任务");
            return task;
        }

        public static List<TaskInfo> GetCurrentTasks(DateTime startTime)
        {
            using var db = FFmpegDbContext.GetNew();
            var tasks = db.Tasks.Where(p => p.IsDeleted == false);
            var runningTasks = tasks.Where(p => p.Status == TaskStatus.Processing);
            var queueTasks = tasks.Where(p => p.Status == TaskStatus.Queue);
            var doneTasks = tasks.Where(p => p.Status == TaskStatus.Done).Where(p => p.StartTime > startTime);
            var errorTasks = tasks.Where(p => p.Status == TaskStatus.Done).Where(p => p.StartTime > startTime);
            var cancelTasks = tasks.Where(p => p.Status == TaskStatus.Done).Where(p => p.StartTime > startTime);
            return runningTasks
                .Concat(queueTasks)
                .Concat(doneTasks)
                .Concat(errorTasks)
                .Concat(cancelTasks)
                .ToList();
        }

        public static PagedListDto<TaskInfo> GetTasks(TaskStatus? status = null, int skip = 0, int take = 0)
        {
            using var db = FFmpegDbContext.GetNew();
            IQueryable<TaskInfo> tasks = db.Tasks
                .Where(p => p.IsDeleted == false);
            if (status.HasValue)
            {
                tasks = tasks.Where(p => p.Status == status);
                switch (status.Value)
                {
                    case TaskStatus.Queue:
                        tasks = tasks.OrderBy(p => p.CreateTime);
                        break;

                    case TaskStatus.Processing:
                    case TaskStatus.Done:
                        tasks = tasks.OrderByDescending(p => p.StartTime);
                        break;

                    default:
                        tasks = tasks.OrderByDescending(p => p.CreateTime);
                        break;
                }
            }
            else
            {
                tasks = tasks.OrderByDescending(p => p.CreateTime);
            }
            int count = tasks.Count();
            if (skip > 0)
            {
                tasks = tasks.Skip(skip);
            }
            if (take > 0)
            {
                tasks = tasks.Take(take);
            }
            return new PagedListDto<TaskInfo>(tasks, count);
        }

        public static TaskInfo GetTask(int id)
        {
            using var db = FFmpegDbContext.GetNew();
            var task = db.Tasks.Find(id);
            return task;
        }

        public static bool HasQueueTasks()
        {
            using var db = FFmpegDbContext.GetNew();
            return db.Tasks.Any(p => p.IsDeleted == false && p.Status == TaskStatus.Queue);
        }

        public static void ResetTask(int id, QueueManager queue)
        {
            using var db = FFmpegDbContext.GetNew();
            TaskInfo task = db.Tasks.Find(id);
            if (task == null)
            {
                throw new ArgumentException($"找不到ID为{id}的任务");
            }
            if (queue.Tasks.Any(p => p.Id == id))
            {
                throw new Exception("ID为{id}的任务正在进行中");
            }
            task.Status = TaskStatus.Queue;
            db.Update(task);
            db.SaveChanges();
        }

        public static int TryResetTasks(IEnumerable<int> ids, QueueManager queue)
        {
            using var db = FFmpegDbContext.GetNew();
            int count = 0;
            foreach (var id in ids)
            {
                TaskInfo task = db.Tasks.Find(id);
                if (task == null)
                {
                    continue;
                }
                if (queue.Tasks.Any(p => p.Id == id))
                {
                    continue;
                }
                task.Status = TaskStatus.Queue;
                db.Update(task);
                count++;
            }

            db.SaveChanges();
            return count;
        }

        public static void CancelTask(int id, QueueManager queue)
        {
            using var db = FFmpegDbContext.GetNew();
            TaskInfo task = db.Tasks.Find(id);
            if (task == null)
            {
                throw new ArgumentException($"找不到ID为{id}的任务");
            }
            CheckCancelingTask(task);
            if (queue.Tasks.Any(p => p.Id == task.Id))
            {
                queue.Managers.First(p => p.Task.Id == task.Id).Cancel();
            }
            task.Status = TaskStatus.Cancel;
            db.Update(task);
            db.SaveChanges();
        }

        public static async Task CancelTaskAsync(int id, QueueManager queue)
        {
            using var db = FFmpegDbContext.GetNew();
            TaskInfo task = await db.Tasks.FindAsync(id);
            if (task == null)
            {
                throw new ArgumentException($"找不到ID为{id}的任务");
            }
            CheckCancelingTask(task);
            if (queue.Tasks.Any(p => p.Id == task.Id))
            {
                await queue.Managers.First(p => p.Task.Id == task.Id).CancelAsync();
            }
            task.Status = TaskStatus.Cancel;
            db.Update(task);
            await db.SaveChangesAsync();
        }

        private static void CheckCancelingTask(TaskInfo task)
        {
            if (task.Status == TaskStatus.Cancel)
            {
                throw new Exception("ID为{id}的任务已被取消");
            }
            if (task.Status == TaskStatus.Done)
            {
                throw new Exception("ID为{id}的任务已完成");
            }
            if (task.Status == TaskStatus.Error)
            {
                throw new Exception("ID为{id}的任务已完成并出现错误");
            }
        }

        public static int TryCancelTasks(IEnumerable<int> ids, QueueManager queue)
        {
            using var db = FFmpegDbContext.GetNew();
            int count = 0;
            foreach (var id in ids)
            {
                TaskInfo task = db.Tasks.Find(id);
                if (task == null)
                {
                    continue;
                }
                if (task.Status == TaskStatus.Cancel)
                {
                    continue;
                }
                if (task.Status == TaskStatus.Done)
                {
                    continue;
                }
                if (task.Status == TaskStatus.Error)
                {
                    continue;
                }
                if (queue.Tasks.Any(p => p.Id == task.Id))
                {
                    queue.Managers.First(p => p.Task.Id == task.Id).Cancel();
                }
                task.Status = TaskStatus.Cancel;
                db.Update(task);
                count++;
            }
            db.SaveChanges();
            return count;
        }

        public static void DeleteTask(int id, QueueManager queue)
        {
            using var db = FFmpegDbContext.GetNew();
            TaskInfo task = db.Tasks.Find(id);
            if (task == null)
            {
                throw new ArgumentException($"找不到ID为{id}的任务");
            }
            if (queue.Tasks.Any(p => p.Id == task.Id))
            {
                queue.Managers.First(p => p.Task.Id == task.Id).Cancel();
            }
            task.IsDeleted = true;
            db.Update(task);
            db.SaveChanges();
        }

        public static int TryDeleteTasks(IEnumerable<int> ids, QueueManager queue)
        {
            using var db = FFmpegDbContext.GetNew();
            int count = 0;
            foreach (var id in ids)
            {
                TaskInfo task = db.Tasks.Find(id);
                if (task == null)
                {
                    continue;
                }
                if (queue.Tasks.Any(p => p.Id == task.Id))
                {
                    queue.Managers.First(p => p.Task.Id == task.Id).Cancel();
                }
                task.IsDeleted = true;
                db.Update(task);
                count++;
            }
            db.SaveChanges();
            return count;
        }
    }
}