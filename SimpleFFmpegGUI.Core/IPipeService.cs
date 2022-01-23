using FFMpegCore;
using FFMpegCore.Enums;
using SimpleFFmpegGUI.Dto;
using SimpleFFmpegGUI.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SimpleFFmpegGUI
{
    public interface IPipeService
    {
        int AddOrUpdatePreset(string name, TaskType type, OutputArguments arguments);

        void CancelQueue();

        void CancelTask(int id);

        void CancelTasks(IEnumerable<int> ids);

        int AddTask(TaskType type, List<InputArguments> inputs, string outputPath, OutputArguments arg);

        void DeletePreset(int id);

        void DeleteTask(int id);

        void DeleteTasks(IEnumerable<int> ids);

        MediaInfoDto GetInfo(string path);

        List<CodePreset> GetPresets();

        void SetDefaultPreset(int id);

        CodePreset GetDefaultPreset(TaskType type);

        StatusDto GetStatus();

        PagedListDto<TaskInfo> GetTasks(TaskStatus? status = null, int skip = 0, int take = 0);

        void Join(IEnumerable<string> path);

        void PauseQueue();

        void ResetTask(int id);

        void ResetTasks(IEnumerable<int> ids);

        void ResumeQueue();

        void StartQueue();

        PagedListDto<Log> GetLogs(char? type = null, int taskId = 0, DateTime? from = null, DateTime? to = null, int skip = 0, int take = 0);

        void CloseFtp(int id);

        List<FileInfoDto> GetFileDetails(string dir);

        List<string> GetFiles(string dir);

        void OpenFtp(int id, string path, int port);

        int? GetFtpPort(int id);

        bool IsFileExist(string path);

        VideoFormat[] GetSuggestedFormats();

        TaskInfo GetTask(int id);
    }
}