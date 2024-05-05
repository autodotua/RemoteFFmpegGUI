using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SimpleFFmpegGUI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleFFmpegGUI.Manager
{
    public class PresetManager
    {
        private readonly FFmpegDbContext db;

        public PresetManager(FFmpegDbContext db)
        {
            this.db = db;
        }

        public async Task<int> AddOrUpdatePresetAsync(string name, TaskType type, OutputArguments arguments)
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
            var presets = db.Presets.Where(p => p.IsDeleted == false
                    && p.Name == name
                    && p.Type == type);
            if (await presets.AnyAsync())
            {
                db.Presets.RemoveRange(presets);
            }
            db.Presets.Add(preset);
            await db.SaveChangesAsync();
            return preset.Id;
        }

        public async void UpdatePresetAsync(CodePreset preset)
        {
            db.Presets.Update(preset);
            await db.SaveChangesAsync();
        }

        public async Task<CodePreset> AddPresetAsync(string name, TaskType type, OutputArguments arguments)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("名称为空");
            }
            if (await ContainsPresetAsync(name, type))
            {
                throw new Exception($"名为{name}的预设已存在");
            }
            CodePreset preset = new CodePreset()
            {
                Name = name,
                Type = type,
                Arguments = arguments
            };
            db.Presets.Add(preset);
            await db.SaveChangesAsync();
            return preset;
        }

        public async Task<bool> ContainsPresetAsync(string name, TaskType type)
        {
            return await db.Presets.Where(p => p.IsDeleted == false
            && p.Name == name
            && p.Type == type).AnyAsync();
        }

        public async Task DeletePresetsAsync()
        {
            foreach (var preset in db.Presets.Where(p => !p.IsDeleted))
            {
                preset.IsDeleted = true;
                db.Entry(preset).State = EntityState.Modified;
            }
            await db.SaveChangesAsync();
        }

        public async Task DeletePresetAsync(int id)
        {
            CodePreset preset = await db.Presets.FindAsync(id) ?? throw new ArgumentException($"找不到ID为{id}的预设");
            db.Presets.Remove(preset);
            await db.SaveChangesAsync();
        }

        public Task<List<CodePreset>> GetPresetsAsync()
        {
            return db.Presets.Where(p => !p.IsDeleted)
                             .OrderBy(p => p.Type)
                             .ThenBy(p => p.Name)
                             .ToListAsync();
        }

        public Task<List<CodePreset>> GetPresetsAsync(TaskType type)
        {
            return db.Presets.Where(p => !p.IsDeleted)
                             .Where(p => p.Type == type)
                             .OrderBy(p => p.Name)
                             .ToListAsync();
        }

        public async Task SetDefaultPresetAsync(int id)
        {
            CodePreset preset = await db.Presets.FindAsync(id) ?? throw new ArgumentException($"找不到ID为{id}的预设");
            var type = preset.Type;
            if (db.Presets.Any(p => p.Type == type && p.Default && !p.IsDeleted))
            {
                foreach (var p in db.Presets.Where(p => p.Type == type && p.Default).ToList())
                {
                    p.Default = false;
                    db.Entry(p).State = EntityState.Modified;
                }
            }
            preset.Default = true;
            db.Entry(preset).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }

        public Task<CodePreset> GetDefaultPreset(TaskType type)
        {
            return db.Presets.FirstOrDefaultAsync(p => p.Type == type && p.Default && !p.IsDeleted);
        }

        public async Task<string> ExportAsync()
        {
            var presets = await GetPresetsAsync();
            return JsonConvert.SerializeObject(presets, Formatting.Indented);
        }

        public async Task ImportAsync(string json)
        {
            var presets = JsonConvert.DeserializeObject<List<CodePreset>>(json);

            foreach (var preset in presets)
            {
                string name = preset.Name;
                var type = preset.Type;
                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentException("名称为空");
                }
                preset.Id = 0;
                var existedPresets = db.Presets.Where(p => p.IsDeleted == false
                 && p.Name == name
                 && p.Type == type);
                if (await existedPresets.AnyAsync())
                {
                    db.Presets.RemoveRange(existedPresets);
                }
                db.Presets.Add(preset);
            }
            await db.SaveChangesAsync();
        }
    }
}