using FFMpegCore;
using FzLib.Collection;
using Mapster;
using MediaInfo;
using Newtonsoft.Json;
using SimpleFFmpegGUI.Dto;
using SimpleFFmpegGUI.Manager;
using SimpleFFmpegGUI.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SimpleFFmpegGUI
{
    public class PipeService : IPipeService
    {
        private static QueueManager manager = new QueueManager();
        public static QueueManager Manager => manager;

        public PipeService()
        {
            Logger.Info("已建立与客户端的连接");
        }

        public void StartQueue()
        {
            manager.StartQueue();
        }

        public void PauseQueue()
        {
            manager.SuspendMainQueue();
        }

        public void ResumeQueue()
        {
            manager.ResumeMainQueue();
        }

        public int AddTask(TaskType type, List<InputArguments> inputs, string outputPath, OutputArguments arg)
        {
            int id = TaskManager.AddTask(type, inputs, outputPath, arg).Id;
            return id;
        }

        public MediaInfoDto GetInfo(string path)
        {
            return MediaInfoManager.GetMediaInfoAsync(path).Result;
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

        public TaskInfo GetTask(int id)
        {
            return TaskManager.GetTask(id);
        }

        public StatusDto GetStatus()
        {
            StatusDto status = manager.MainQueueManager == null
                ? new StatusDto()
                : manager.MainQueueManager.GetStatus();

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

        public int AddOrUpdatePreset(string name, TaskType type, OutputArguments arguments)
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

        public PagedListDto<Log> GetLogs(char? type = null, int taskId = 0, DateTime? from = null, DateTime? to = null, int skip = 0, int take = 0)
        {
            return LogManager.GetLogs(type, taskId, from, to, skip, take);
        }

        public List<string> GetFiles(string dir)
        {
            return Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories).ToList();
        }

        public List<FileInfoDto> GetFileDetails(string dir)
        {
            return Directory.EnumerateFiles(dir).Select(p => new FileInfoDto(p)).ToList();
        }

        private ConcurrentDictionary<int, FtpManager> ftps = new ConcurrentDictionary<int, FtpManager>();

        public void OpenFtp(int id, string path, int port)
        {
            if (ftps.ContainsKey(id))
            {
                return;
            }
            FtpManager manager = new FtpManager(path, port);
            if (ftps.TryAdd(id, manager))
            {
                manager.StartAsync().Wait();
            }
        }

        public void CloseFtp(int id)
        {
            if (ftps.ContainsKey(id))
            {
                ftps[id].StopAsync().Wait();
                ftps.TryRemove(id, out _);
            }
        }

        public int? GetFtpPort(int id)
        {
            return ftps.GetOrDefault(id)?.Port;
        }

        public bool IsFileExist(string path)
        {
            return File.Exists(path);
        }

        public VideoFormat[] GetSuggestedFormats()
        {
            return VideoFormat.Formats;
        }

        public void SetDefaultPreset(int id)
        {
            PresetManager.SetDefaultPreset(id);
        }

        public CodePreset GetDefaultPreset(TaskType type)
        {
            return PresetManager.GetDefaultPreset(type);
        }

        public void ImportPresets(string json)
        {
            PresetManager.Import(json);
        }

        public string ExportPresets()
        {
            return PresetManager.Export();
        }

        public void SetShutdownAfterQueueFinished(bool v)
        {
            Manager.PowerManager.ShutdownAfterQueueFinished = v;
        }
        public bool IsShutdownAfterQueueFinished()
        {
            return Manager.PowerManager.ShutdownAfterQueueFinished;
        }

        public void Shutdown()
        {
            Manager.PowerManager.Shutdown();
        }
        public void AbortShutdown()
        {
            Manager.PowerManager.AbortShutdown();
        }

        public CpuCoreUsageDto[] GetCpuUsage(TimeSpan sampleSpan)
        {
            return PowerManager.GetCpuUsage(sampleSpan);
        }

        public void ScheduleQueue(DateTime time)
        {
            Manager.ScheduleQueue(time);
        }

        public void CancelQueueSchedule()
        {
            Manager.CancelQueueSchedule();
        }

        public DateTime? GetQueueScheduleTime()
        {
            return Manager.GetQueueScheduleTime();
        }
    }
}