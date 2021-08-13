using SimpleFFmpegGUI.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleFFmpegGUI
{
    public static class FFmpegPresetManager
    {
        public static void AddOrUpdatePreset(string name, TaskType type, CodeArguments arguments)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("名称为空");
            }
            CodePreset preset = new CodePreset()
            {
                Name = name,
                Type = type,
                Arguments = arguments
            };
            using var db = new FFmpegDbContext();
            if (db.Presets.Any(p => p.Name == name))
            {
                db.Presets.RemoveRange(db.Presets.Where(p => p.Name == name).ToArray());
            }
            db.Presets.Add(preset);
            db.SaveChanges();
        }

        public static void DeletePreset(int id)
        {
            using var db = new FFmpegDbContext();
            CodePreset preset = db.Presets.Find(id);
            if (preset == null)
            {
                throw new ArgumentException($"找不到ID为{id}的预设");
            }
            db.Presets.Remove(preset);
            db.SaveChanges();
        }

        public static List<CodePreset> GetPresets()
        {
            using var db = new FFmpegDbContext();
            return db.Presets.Where(p => !p.IsDeleted).ToList();
        }
    }
}