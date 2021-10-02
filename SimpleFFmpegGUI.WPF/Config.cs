using FzLib;
using FzLib.DataStorage.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
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
            //if (!Directory.Exists(Path.GetDirectoryName(path)))
            //{
            //    Directory.CreateDirectory(Path.GetDirectoryName(path));
            //}
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
    }

    public class RemoteHost
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Token { get; set; }
    }
}