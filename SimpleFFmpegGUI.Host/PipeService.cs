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

        private static FFmpegQueueManager manager = new FFmpegQueueManager();

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
            FFmpegTaskManager.AddTask(TaskType.Code, path, outputPath, arg);

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

        public PagedListDto<TaskInfo> GetTasks(TaskStatus? status = null, int skip = 0, int take = 0)
        {
            return FFmpegTaskManager.GetTasks(status, skip, take);
        }

        public StatusDto GetStatus()
        {
            StatusDto status = manager.ProcessingTask == null ? new StatusDto()
                : new StatusDto(manager.ProcessingTask, manager.Progress, lastOutput);

            return status;
        }

        public void ResetTask(int id)
        {
            FFmpegTaskManager.ResetTask(id, manager);
        }

        public void CancelTask(int id)
        {
            FFmpegTaskManager.CancelTask(id, manager);
        }

        public void ResetTasks(IEnumerable<int> ids)
        {
            FFmpegTaskManager.TryResetTasks(ids, manager);
        }

        public void CancelTasks(IEnumerable<int> ids)
        {
            FFmpegTaskManager.TryCancelTasks(ids, manager);
        }

        public void DeleteTask(int id)
        {
            FFmpegTaskManager.DeleteTask(id, manager);
        }

        public void DeleteTasks(IEnumerable<int> ids)
        {
            FFmpegTaskManager.TryDeleteTasks(ids, manager);
        }

        public void AddOrUpdatePreset(string name, TaskType type, CodeArguments arguments)
        {
            FFmpegPresetManager.AddOrUpdatePreset(name, type, arguments);
        }

        public void DeletePreset(int id)
        {
            FFmpegPresetManager.DeletePreset(id);
        }

        public List<CodePreset> GetPresets()
        {
            return FFmpegPresetManager.GetPresets();
        }
    }
}