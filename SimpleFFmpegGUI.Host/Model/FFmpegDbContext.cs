using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleFFmpegGUI.Model
{
    public class FFmpegDbContext : DbContext
    {
        public FFmpegDbContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=db.sqlite");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var strListConverter = new EfJsonConverter<List<string>>();
            var argConverter = new EfJsonConverter<CodeArguments>();
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<TaskInfo>()
                .Property(p => p.Inputs)
                .HasConversion(strListConverter);
            modelBuilder.Entity<TaskInfo>()
                .Property(p => p.Arguments)
                .HasConversion(argConverter);
            modelBuilder.Entity<CodePreset>()
                .Property(p => p.Arguments)
                .HasConversion(argConverter);
        }

        public DbSet<TaskInfo> Tasks { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<CodePreset> Presets { get; set; }
    }

    public class EfJsonConverter<T> : ValueConverter<T, string>
    {
        public EfJsonConverter() : base(p => JsonConvert.SerializeObject(p), p => JsonConvert.DeserializeObject<T>(p))
        {
        }
    }
}