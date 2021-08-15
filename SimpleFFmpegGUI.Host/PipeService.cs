using FFMpegCore;
using Mapster;
using Newtonsoft.Json;
using SimpleFFmpegGUI.Dto;
using SimpleFFmpegGUI.Manager;
using SimpleFFmpegGUI.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SimpleFFmpegGUI
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

        private static QueueManager manager = new QueueManager();

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

        public int AddCodeTask(IEnumerable<string> path, string outputPath, CodeArguments arg, bool start)
        {
            int id = TaskManager.AddTask(TaskType.Code, path, outputPath, arg);

            if (start)
            {
                manager.StartQueue();
            }
            return id;
        }

        public MediaInfoDto GetInfo(string path)
        {
            IMediaAnalysis result = null;
            try
            {
                result = FFProbe.Analyse(path);
            }
            catch (Exception ex)
            {
                throw new Exception("查询信息失败");
            }
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

        public PagedListDto<TaskInfo> GetTasks(TaskStatus? status = null, int skip = 0, int take = 0)
        {
            return TaskManager.GetTasks(status, skip, take);
        }

        public StatusDto GetStatus()
        {
            StatusDto status = manager.ProcessingTask == null ? new StatusDto()
                : new StatusDto(manager.ProcessingTask, manager.Progress, lastOutput);

            return status;
        }

        public void ResetTask(int id)
        {
            TaskManager.ResetTask(id, manager);
        }

        public void CancelTask(int id)
        {
            TaskManager.CancelTask(id, manager);
        }

        public void ResetTasks(IEnumerable<int> ids)
        {
            TaskManager.TryResetTasks(ids, manager);
        }

        public void CancelTasks(IEnumerable<int> ids)
        {
            TaskManager.TryCancelTasks(ids, manager);
        }

        public void DeleteTask(int id)
        {
            TaskManager.DeleteTask(id, manager);
        }

        public void DeleteTasks(IEnumerable<int> ids)
        {
            TaskManager.TryDeleteTasks(ids, manager);
        }

        public int AddOrUpdatePreset(string name, TaskType type, CodeArguments arguments)
        {
            return PresetManager.AddOrUpdatePreset(name, type, arguments);
        }

        public void DeletePreset(int id)
        {
            PresetManager.DeletePreset(id);
        }

        public List<CodePreset> GetPresets()
        {
            return PresetManager.GetPresets();
        }

        public PagedListDto<Log> GetLogs(char? type = null, DateTime? from = null, DateTime? to = null, int skip = 0, int take = 0)
        {
            return LogManager.GetLogs(type, from, to, skip, take);
        }
    }
}