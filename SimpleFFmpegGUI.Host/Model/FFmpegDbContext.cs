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
            Logger.Info("数据库已建立");
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
            Logger.Info("数据库已初始化");
        }

        public DbSet<TaskInfo> Tasks { get; set; }
    }

    public class EfJsonConverter<T> : ValueConverter<T, string>
    {
        public EfJsonConverter() : base(p => JsonConvert.SerializeObject(p), p => JsonConvert.DeserializeObject<T>(p))
        {
        }
    }
}