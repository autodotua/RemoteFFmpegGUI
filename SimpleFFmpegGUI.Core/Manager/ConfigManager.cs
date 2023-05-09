using Newtonsoft.Json;
using SimpleFFmpegGUI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFFmpegGUI.Manager
{
    public static class ConfigManager
    {
        public const string SyncModifiedTimeKey = "SyncModifiedTime";

        public static bool SyncModifiedTime
        {
            get => GetConfig(SyncModifiedTimeKey, true);
            set => SetConfig(SyncModifiedTimeKey, value);
        }

        public static T GetConfig<T>(string key, T defaultValue)
        {
            using var db = FFmpegDbContext.GetNew();
            var item = db.Configs.Where(p => p.Key == key).FirstOrDefault();
            if (item == null)
            {
                return defaultValue;
            }
            return Parse<T>(item.Value);
        }

        public static void SetConfig<T>(string key, T value)
        {
            using var db = FFmpegDbContext.GetNew();
            var item = db.Configs.Where(p => p.Key == key).FirstOrDefault();
            if (item == null)
            {
                item = new Config()
                {
                    Key = key,
                    Value = GetString(value)
                };
                db.Configs.Add(item);
            }
            else
            {
                item.Value = GetString(value);
                db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            }

            db.SaveChanges();
        }

        private static T Parse<T>(string data)
        {
            return JsonConvert.DeserializeObject<T>(data);
        }

        private static string GetString<T>(T data)
        {
            return JsonConvert.SerializeObject(data);
        }
    }
}
