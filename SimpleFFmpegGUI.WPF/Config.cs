using FzLib;
using FzLib.DataStorage.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SimpleFFmpegGUI.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Path = System.IO.Path;

namespace SimpleFFmpegGUI.WPF
{
    public class Config : IJsonSerializable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private static bool loaded = false;

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

        private const string path = "config.json";

        public void Save()
        {
            this.Save(path, new JsonSerializerSettings().SetIndented());
        }

        private bool smoothScroll = true;

        public bool SmoothScroll
        {
            get => smoothScroll;
            set => this.SetValueAndNotify(ref smoothScroll, value, nameof(SmoothScroll));
        }

        private List<RemoteHost> remoteHosts = new List<RemoteHost>();

        public List<RemoteHost> RemoteHosts
        {
            get => remoteHosts;
            set => this.SetValueAndNotify(ref remoteHosts, value, nameof(RemoteHosts));
        }

        private bool clearFilesAfterAddTask;

        public bool ClearFilesAfterAddTask
        {
            get => clearFilesAfterAddTask;
            set => this.SetValueAndNotify(ref clearFilesAfterAddTask, value, nameof(ClearFilesAfterAddTask));
        }

        private bool startQueueAfterAddTask = true;

        public bool StartQueueAfterAddTask
        {
            get => startQueueAfterAddTask;
            set => this.SetValueAndNotify(ref startQueueAfterAddTask, value, nameof(StartQueueAfterAddTask));
        }

        private bool rememberLastArguments = true;

        public bool RememberLastArguments
        {
            get => rememberLastArguments;
            set => this.SetValueAndNotify(ref rememberLastArguments, value, nameof(RememberLastArguments));
        }

        public Dictionary<TaskType, OutputArguments> LastOutputArguments { get; set; } = new Dictionary<TaskType, OutputArguments>();
        private DefaultOutputDirType defaultOutputDirType = DefaultOutputDirType.InputDir;
        public DefaultOutputDirType DefaultOutputDirType
        {
            get => defaultOutputDirType;
            set => this.SetValueAndNotify(ref defaultOutputDirType, value, nameof(DefaultOutputDirType));
        }

        private string defaultOutputDirInputSubDirName = "output";
        public string DefaultOutputDirInputSubDirName
        {
            get => defaultOutputDirInputSubDirName;
            set => this.SetValueAndNotify(ref defaultOutputDirInputSubDirName, value, nameof(DefaultOutputDirInputSubDirName));
        }
        private string defaultOutputDirSpecialDirPath = "C:\\output";
        public string DefaultOutputDirSpecialDirPath
        {
            get => defaultOutputDirSpecialDirPath;
            set => this.SetValueAndNotify(ref defaultOutputDirSpecialDirPath, value, nameof(DefaultOutputDirSpecialDirPath));
        }
        public string PerformanceTestFileName { get; set; } = "test.mp4";
        public bool WindowMaximum { get; set; } = false;
    }

    public class RemoteHost
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Token { get; set; }
    }

    public enum DefaultOutputDirType
    {
        [Description("输入文件所在文件夹")]
        InputDir,
        [Description("输入文件下指定文件夹")]
        InputNewDir,
        [Description("指定文件夹")]
        SpecialDir
    }
}