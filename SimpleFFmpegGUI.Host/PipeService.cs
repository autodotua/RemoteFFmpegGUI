using AutoMapper;
using FFMpegCore;
using Mapster;
using Newtonsoft.Json;
using SimpleFFmpegGUI.Dto;
using SimpleFFmpegGUI.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SimpleFFmpegGUI.Host
{
    public class PipeService : IPipeService
    {
        static PipeService()
        {
            manager.FFmpegOutput += (p1, p2) =>
            {
                lastOutput = p2.Data;
            };
        }

        private static FFmpegTaskManager manager = new FFmpegTaskManager();

        public PipeService()
        {
            Logger.Info("实例化新的PipeService类");
        }

        private static string lastOutput;

        public string GetLastOutput()
        {
            return lastOutput;
        }

        public void StartQueue()
        {
            manager.StartQueue();
        }

        public void PauseQueue()
        {
            manager.Suspend();
        }

        public void ResumeQueue()
        {
            manager.Resume();
        }

        public void CreateCodeTask(IEnumerable<string> path, string outputPath, CodeArguments arg, bool start)
        {
            using (FFmpegDbContext db = new FFmpegDbContext())
            {
                var task = new TaskInfo()
                {
                    Type = TaskType.Code,
                    Inputs = path.ToList(),
                    Output = outputPath,
                    Arguments = arg
                };
                Logger.Info(task, "新建任务");
                db.Tasks.Add(task);
                db.SaveChanges();
            }
            if (start)
            {
                manager.StartQueue();
            }
        }

        public MediaInfoDto GetInfo(string path)
        {
            var result = FFProbe.Analyse(path);
            return result.Adapt<MediaInfoDto>();
        }

        public void Join(IEnumerable<string> path)
        {
            throw new NotImplementedException();
        }

        public void CancelQueue()
        {
            manager.Cancel();
        }

        public List<TaskInfo> GetTasks(TaskStatus? status = null, int skip = 0, int take = 0)
        {
            using (var db = new FFmpegDbContext())
            {
                IQueryable<TaskInfo> tasks = db.Tasks;
                if (status.HasValue)
                {
                    tasks = tasks.Where(p => p.Status == status);
                }
                if (skip > 0)
                {
                    tasks = tasks.Skip(skip);
                }
                if (take > 0)
                {
                    tasks = tasks.Take(take);
                }
                return tasks.ToList();
            }
        }

        public StatusDto GetStatus()
        {
            StatusDto status = manager.ProcessingTask == null ? new StatusDto()
                : new StatusDto(manager.ProcessingTask, manager.Progress, lastOutput);

            return status;
        }

        public void ResetTask(int id)
        {
            using (var db = new FFmpegDbContext())
            {
                TaskInfo task = db.Tasks.Find(id);
                if (task == null)
                {
                    throw new ArgumentException($"找不到ID为{id}的任务");
                }
                if (manager.ProcessingTask?.ID == id)
                {
                    throw new Exception("ID为{id}的任务正在进行中");
                }
                task.Status = TaskStatus.Queue;
                db.Update(task);
                db.SaveChanges();
            }
        }
    }
}