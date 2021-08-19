using SimpleFFmpegGUI.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SimpleFFmpegGUI
{
    public static class ConfigHelper
    {
        private const string TempDirKey = nameof(TempDir);

        public static string TempDir
        {
            get
            {
                using var db = FFmpegDbContext.GetNew();
                var value = db.Configs.FirstOrDefault(p => p.Key == TempDirKey)?.Value;
                if (value == null)
                {
                    string str = Guid.NewGuid().ToString();
                    value = Path.Combine(Path.GetTempPath(), str);
                    Directory.CreateDirectory(value);
                }
                return value;
            }
            set
            {
                using var db = FFmpegDbContext.GetNew();
                db.Configs.Add(new Config(TempDir, value));
                db.SaveChanges();
            }
        }
    }
}