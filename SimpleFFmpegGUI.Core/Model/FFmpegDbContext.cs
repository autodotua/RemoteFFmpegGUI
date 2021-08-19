using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleFFmpegGUI.Model
{
    public class FFmpegDbContext : DbContext
    {
        private static FFmpegDbContext db;
        private bool hasDb = false;

        //internal static FFmpegDbContext Get()
        //{
        //    if (db == null)
        //    {
        //        db = new FFmpegDbContext();
        //    }
        //    return db;
        //}

        internal static FFmpegDbContext GetNew()
        {
            return new FFmpegDbContext();
        }

        private FFmpegDbContext()
        {
            if (!hasDb)
            {
                Database.EnsureCreated();
                hasDb = true;
            }
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
        public DbSet<Config> Configs { get; set; }

        public void Check()
        {
            var changed = false;
            foreach (var item in Tasks.Where(p => p.Status == TaskStatus.Processing))
            {
                changed = true;
                item.Status = TaskStatus.Error;
                item.Message = "状态异常：启动时处于正在运行状态";
            }
            if (changed)
            {
                SaveChanges();
            }
        }
    }
}