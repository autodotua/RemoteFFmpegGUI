using SimpleFFmpegGUI.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleFFmpegGUI.Manager
{
    public static class PresetManager
    {
        public static int AddOrUpdatePreset(string name, TaskType type, OutputArguments arguments)
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
            using var db = FFmpegDbContext.GetNew();
            if (db.Presets.Any(p => p.Name == name && p.Type == type))
            {
                db.Presets.RemoveRange(db.Presets.Where(p => p.Name == name && p.Type == type).ToArray());
            }
            db.Presets.Add(preset);
            db.SaveChanges();
            return preset.Id;
        }

        public static void DeletePreset(int id)
        {
            using var db = FFmpegDbContext.GetNew();
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
            using var db = FFmpegDbContext.GetNew();
            return db.Presets.Where(p => !p.IsDeleted).OrderBy(p => p.Type).ToList();
        }
    }
}