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
        public const string DefaultProcessPriorityKey = "DefaultProcessPriority";
        public const string SyncModifiedTimeKey = "SyncModifiedTime";
        private static readonly Dictionary<string, object> cache = new Dictionary<string, object>();

        public static int DefaultProcessPriority
        {
            get => GetConfig(DefaultProcessPriorityKey, 2);
            set => SetConfig(DefaultProcessPriorityKey, value);
        }

        public static bool SyncModifiedTime
        {
            get => GetConfig(SyncModifiedTimeKey, true);
            set => SetConfig(SyncModifiedTimeKey, value);
        }
        public static T GetConfig<T>(string key, T defaultValue)
        {
            if (cache.ContainsKey(key))
            {
                return (T)cache[key];
            }
            using var db = FFmpegDbContext.GetNew();
            var item = db.Configs.Where(p => p.Key == key).FirstOrDefault();
            if (item == null)
            {
                return defaultValue;
            }
            T value = Parse<T>(item.Value);
            cache.Add(key, value);

            using Logger logger = new Logger();
            logger.Info($"读取配置：[{key}]={value}");
            return value;
        }
        public static void SetConfig<T>(string key, T value)
        {
            if (cache.ContainsKey(key))
            {
                cache[key] = value;
            }
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

            using Logger logger = new Logger();
            logger.Info($"写入配置：[{key}]={value}");
        }

        private static string GetString<T>(T data)
        {
            return JsonConvert.SerializeObject(data);
        }

        private static T Parse<T>(string data)
        {
            return JsonConvert.DeserializeObject<T>(data);
        }
    }
}
