using FzLib;
using FzLib.DataStorage.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SimpleFFmpegGUI.Model;
using SimpleFFmpegGUI.WPF.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Path = System.IO.Path;

namespace SimpleFFmpegGUI.WPF
{
    public enum DefaultOutputDirType
    {
        [Description("输入文件所在文件夹")]
        InputDir,
        [Description("输入文件下指定文件夹")]
        InputNewDir,
        [Description("指定文件夹")]
        SpecialDir
    }

    public class Config : IJsonSerializable, INotifyPropertyChanged
    {
        private const string path = "config.json";

        private static bool loaded = false;

        private bool clearFilesAfterAddTask;

        private string defaultOutputDirInputSubDirName = "output";

        private string defaultOutputDirSpecialDirPath = "C:\\output";

        private DefaultOutputDirType defaultOutputDirType = DefaultOutputDirType.InputDir;

        private bool rememberLastArguments = true;

        private List<RemoteHost> remoteHosts = new List<RemoteHost>();

        private bool smoothScroll = true;

        private bool startQueueAfterAddTask = true;

        public event PropertyChangedEventHandler PropertyChanged;
        public static Config Instance
        {
            get
            {
                if (!loaded)
                {
                    loaded = true;
                    try
                    {
                        App.ServiceProvider.GetService<Config>().TryLoadFromJsonFile(path);
                    }
                    catch (Exception ex)
                    {
                    }
                }
                return App.ServiceProvider.GetService<Config>();
            }
        }
        public bool ClearFilesAfterAddTask
        {
            get => clearFilesAfterAddTask;
            set => this.SetValueAndNotify(ref clearFilesAfterAddTask, value, nameof(ClearFilesAfterAddTask));
        }

        public string DefaultOutputDirInputSubDirName
        {
            get => defaultOutputDirInputSubDirName;
            set => this.SetValueAndNotify(ref defaultOutputDirInputSubDirName, value, nameof(DefaultOutputDirInputSubDirName));
        }

        public string DefaultOutputDirSpecialDirPath
        {
            get => defaultOutputDirSpecialDirPath;
            set => this.SetValueAndNotify(ref defaultOutputDirSpecialDirPath, value, nameof(DefaultOutputDirSpecialDirPath));
        }

        public DefaultOutputDirType DefaultOutputDirType
        {
            get => defaultOutputDirType;
            set => this.SetValueAndNotify(ref defaultOutputDirType, value, nameof(DefaultOutputDirType));
        }

        public Dictionary<TaskType, OutputArguments> LastOutputArguments { get; set; } = new Dictionary<TaskType, OutputArguments>();

        public bool RememberLastArguments
        {
            get => rememberLastArguments;
            set => this.SetValueAndNotify(ref rememberLastArguments, value, nameof(RememberLastArguments));
        }

        public List<RemoteHost> RemoteHosts
        {
            get => remoteHosts;
            set => this.SetValueAndNotify(ref remoteHosts, value, nameof(RemoteHosts));
        }

        public bool SmoothScroll
        {
            get => smoothScroll;
            set => this.SetValueAndNotify(ref smoothScroll, value, nameof(SmoothScroll));
        }

        public bool StartQueueAfterAddTask
        {
            get => startQueueAfterAddTask;
            set => this.SetValueAndNotify(ref startQueueAfterAddTask, value, nameof(StartQueueAfterAddTask));
        }

        public bool WindowMaximum { get; set; } = false;

        public string TestVideo { get; set; }
        public PerformanceTestLine[] TestItems { get; set; }
        public PerformanceTestCodecParameter[] TestCodecs { get; set; }

        public void Save()
        {
            this.Save(path, new JsonSerializerSettings().SetIndented());
        }
    }

    public class RemoteHost
    {
        public string Address { get; set; }
        public string Name { get; set; }
        public string Token { get; set; }
    }
}