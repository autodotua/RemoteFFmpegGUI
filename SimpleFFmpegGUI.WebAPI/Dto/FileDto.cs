using FzLib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleFFmpegGUI.WebAPI.Dto
{
    public class FileDto
    {
        public static ConcurrentDictionary<string, string> Guid2File { get; } = new ConcurrentDictionary<string, string>();
        private static ConcurrentDictionary<string, string> File2Guid { get; } = new ConcurrentDictionary<string, string>();

        public FileDto(string path)
        {
            FileInfo file = new FileInfo(path);
            Name = file.Name;
            Length = file.Length;
            LengthText = NumberConverter.ByteToFitString(Length);
            LastWriteTime = file.LastWriteTime;
            if (!File2Guid.ContainsKey(path))
            {
                string id = Guid.NewGuid().ToString();
                Guid2File.TryAdd(id, path);
                File2Guid.TryAdd(path, id);
            }
            Id = File2Guid[path];
        }

        public string Name { get; set; }
        public long Length { get; set; }
        public string LengthText { get; set; }
        public DateTime LastWriteTime { get; set; }
        public string Id { get; set; }
    }
}