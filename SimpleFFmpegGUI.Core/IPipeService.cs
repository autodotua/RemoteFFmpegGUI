using FFMpegCore;
using FFMpegCore.Enums;
using SimpleFFmpegGUI.FFmpegLib;
using SimpleFFmpegGUI.Dto;
using SimpleFFmpegGUI.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SimpleFFmpegGUI.Model.MediaInfo;
using System.Threading.Tasks;
using TaskStatus = SimpleFFmpegGUI.Model.TaskStatus;

namespace SimpleFFmpegGUI
{
    public interface IPipeService
    {
        void AbortShutdown();

        Task<int> AddOrUpdatePresetAsync(string name, TaskType type, OutputArguments arguments);

        Task<int> AddTaskAsync(TaskType type, List<InputArguments> inputs, string outputPath, OutputArguments arg);

        void CancelQueue();

        void CancelQueueSchedule();

        Task CancelTaskAsync(int id);

        Task CancelTasksAsync(IEnumerable<int> ids);
        void CloseFtp(int id);

        Task DeletePresetAsync(int id);

        Task DeleteTaskAsync(int id);

        Task DeleteTasksAsync(IEnumerable<int> ids);

        Task<string> ExportPresetsAsync();

        CpuCoreUsageDto[] GetCpuUsage(TimeSpan sampleSpan);

        Task<CodePreset> GetDefaultPresetAsync(TaskType type);

        public int GetDefaultProcessPriority();

        List<FileInfoDto> GetFileDetails(string dir);

        List<string> GetFiles(string dir);

        int? GetFtpPort(int id);

        Task<MediaInfoGeneral> GetInfoAsync(string path);

        Task<PagedListDto<Log>> GetLogsAsync(char? type = null, int taskId = 0, DateTime? from = null, DateTime? to = null, int skip = 0, int take = 0);

        Task<List<CodePreset>> GetPresetsAsync();

        DateTime? GetQueueScheduleTime();

        string GetSingleFileInDir(string dir, string name);

        public string GetSnapshot(string path, double seconds);

        StatusDto GetStatus();

        VideoFormat[] GetSuggestedFormats();

        Task<TaskInfo> GetTaskAsync(int id);

        Task<PagedListDto<TaskInfo>> GetTasksAsync(TaskStatus? status = null, int skip = 0, int take = 0);

        Task ImportPresetsAsync(string json);

        bool IsFileExist(string path);

        bool IsShutdownAfterQueueFinished();

        void Join(IEnumerable<string> path);

        void OpenFtp(int id, string path, int port);

        void PauseQueue();

        public byte[] ReadFiles(string path);

        Task ResetTaskAsync(int id);

        Task ResetTasksAsync(IEnumerable<int> ids);

        void ResumeQueue();

        void ScheduleQueue(DateTime time);

        Task SetDefaultPresetAsync(int id);
        public void SetDefaultProcessPriority(int priority);

        void SetShutdownAfterQueueFinished(bool v);

        void Shutdown();

        void StartQueue();
        System.Threading.Tasks.Task TestAsync();
    }
}