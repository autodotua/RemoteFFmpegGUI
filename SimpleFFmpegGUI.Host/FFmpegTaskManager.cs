using SimpleFFmpegGUI.Dto;
using SimpleFFmpegGUI.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleFFmpegGUI.Host
{
    public class FFmpegTaskManager
    {
        public static PagedListDto<TaskInfo> GetTasks(TaskStatus? status = null, int skip = 0, int take = 0)
        {
            using var db = new FFmpegDbContext();
            IQueryable<TaskInfo> tasks = db.Tasks
                .Where(p => p.IsDeleted == false)
                .OrderByDescending(p => p.CreateTime);
            if (status.HasValue)
            {
                tasks = tasks.Where(p => p.Status == status);
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

        public static void ResetTask(int id, FFmpegQueueManager queue)
        {
            using var db = new FFmpegDbContext();
            TaskInfo task = db.Tasks.Find(id);
            if (task == null)
            {
                throw new ArgumentException($"找不到ID为{id}的任务");
            }
            if (queue.ProcessingTask?.ID == id)
            {
                throw new Exception("ID为{id}的任务正在进行中");
            }
            task.Status = TaskStatus.Queue;
            db.Update(task);
            db.SaveChanges();
        }

        public static int TryResetTasks(IEnumerable<int> ids, FFmpegQueueManager queue)
        {
            using var db = new FFmpegDbContext();
            int count = 0;
            foreach (var id in ids)
            {
                TaskInfo task = db.Tasks.Find(id);
                if (task == null)
                {
                    continue;
                }
                if (queue.ProcessingTask?.ID == id)
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

        public static void CancelTask(int id, FFmpegQueueManager queue)
        {
            using var db = new FFmpegDbContext();
            TaskInfo task = db.Tasks.Find(id);
            if (task == null)
            {
                throw new ArgumentException($"找不到ID为{id}的任务");
            }
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
            if (queue.ProcessingTask?.ID == id)
            {
                queue.CancelCurrent();
            }
            task.Status = TaskStatus.Cancel;
            db.Update(task);
            db.SaveChanges();
        }

        public static int TryCancelTasks(IEnumerable<int> ids, FFmpegQueueManager queue)
        {
            using var db = new FFmpegDbContext();
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
                if (queue.ProcessingTask?.ID == id)
                {
                    queue.CancelCurrent();
                }
                task.Status = TaskStatus.Cancel;
                db.Update(task);
                count++;
            }
            db.SaveChanges();
            return count;
        }

        public static void DeleteTask(int id, FFmpegQueueManager queue)
        {
            using var db = new FFmpegDbContext();
            TaskInfo task = db.Tasks.Find(id);
            if (task == null)
            {
                throw new ArgumentException($"找不到ID为{id}的任务");
            }
            if (queue.ProcessingTask?.ID == id)
            {
                queue.CancelCurrent();
            }
            task.IsDeleted = true;
            db.Update(task);
            db.SaveChanges();
        }

        public static int TryDeleteTasks(IEnumerable<int> ids, FFmpegQueueManager queue)
        {
            using var db = new FFmpegDbContext();
            int count = 0;
            foreach (var id in ids)
            {
                TaskInfo task = db.Tasks.Find(id);
                if (task == null)
                {
                    continue;
                }
                if (queue.ProcessingTask?.ID == id)
                {
                    queue.CancelCurrent();
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