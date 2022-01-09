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
            if (db.Presets.Where(p => p.IsDeleted == false).Any(p => p.Name == name && p.Type == type))
            {
                db.Presets.RemoveRange(db.Presets.Where(p => p.IsDeleted == false).Where(p => p.Name == name && p.Type == type).ToArray());
            }
            db.Presets.Add(preset);
            db.SaveChanges();
            return preset.Id;
        }

        public static void UpdatePreset(CodePreset preset)
        {
            using var db = FFmpegDbContext.GetNew();
            db.Presets.Update(preset);
            db.SaveChanges();
        }
        public static CodePreset AddPreset(string name, TaskType type, OutputArguments arguments)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("名称为空");
            }
            if (ContainsPreset(name, type))
            {
                throw new Exception($"名为{name}的预设已存在");
            }
            CodePreset preset = new CodePreset()
            {
                Name = name,
                Type = type,
                Arguments = arguments
            };
            using var db = FFmpegDbContext.GetNew();

            db.Presets.Add(preset);
            db.SaveChanges();
            return preset;
        }

        public static bool ContainsPreset(string name, TaskType type)
        {
            using var db = FFmpegDbContext.GetNew();
            if (db.Presets.Where(p => p.IsDeleted == false).Any(p => p.Name == name && p.Type == type))
            {
                return true;
            }
            return false;
        }

        public static void DeletePresets()
        {
            using var db = FFmpegDbContext.GetNew();
            foreach (var preset in db.Presets)
            {
                preset.IsDeleted = true;
                db.Entry(preset).State=Microsoft.EntityFrameworkCore.EntityState.Modified;
            }
            db.SaveChanges();
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
        public static List<CodePreset> GetPresets(TaskType type)
        {
            using var db = FFmpegDbContext.GetNew();
            return db.Presets.Where(p => !p.IsDeleted).Where(p=>p.Type==type).ToList();
        }
    }
}