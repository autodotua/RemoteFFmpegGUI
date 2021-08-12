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
        void AddOrUpdatePreset(string name, TaskType type, CodeArguments arguments);

        void CancelQueue();

        void CancelTask(int id);

        void CancelTasks(IEnumerable<int> ids);

        void CreateCodeTask(IEnumerable<string> path, string outputPath, CodeArguments arg, bool start);

        void DeletePreset(int id);

        void DeleteTask(int id);

        void DeleteTasks(IEnumerable<int> ids);

        MediaInfoDto GetInfo(string path);

        string GetLastOutput();

        List<CodePreset> GetPresets();

        StatusDto GetStatus();

        PagedListDto<TaskInfo> GetTasks(TaskStatus? status = null, int skip = 0, int take = 0);

        void Join(IEnumerable<string> path);

        void PauseQueue();

        void ResetTask(int id);

        void ResetTasks(IEnumerable<int> ids);

        void ResumeQueue();

        void StartQueue();
    }
}