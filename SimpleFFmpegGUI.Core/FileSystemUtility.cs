﻿using FzLib.IO;
using SimpleFFmpegGUI.FFmpegLib;
using SimpleFFmpegGUI.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SimpleFFmpegGUI
{
    public static class FileSystemUtility
    {
        private const string TempDirKey = "TempDir";

        /// <summary>
        /// 不合法的文件名字符集合
        /// </summary>
        private static HashSet<char> invalidFileNameChars = Path.GetInvalidFileNameChars().ToHashSet();

        public static string GetSequence(string sampleFilePath)
        {
            string dir = Path.GetDirectoryName(sampleFilePath);
            string filename = Path.GetFileNameWithoutExtension(sampleFilePath);
            string ext = Path.GetExtension(sampleFilePath);
            var filesInDir = Directory.EnumerateFiles(dir, $"*{ext}").ToList();
            for (int i = filename.Length - 1; i >= 0; i--)
            {
                var thisChar = filename[i];
                var rightChar = i == filename.Length - 1 ? '\0' : filename[i + 1];
                if (thisChar is '0' or '1' && rightChar is < '0' or > '9') //当前字符是0或1，右侧是非数字
                {
                    int indexLength = 1;
                    for (int j = i - 1; j >= 0; j--)
                    {
                        if (filename[j] == '0')
                        {
                            indexLength++;
                        }
                    }
                    string leftPart = filename[..(i - indexLength + 1)];
                    string rightPart = filename[(i + 1)..];
                    int indexFrom = thisChar - '0';
                    string nextFileName = leftPart + (indexFrom + 1).ToString().PadLeft(indexLength, '0') + rightPart;
                    nextFileName = Path.Combine(dir, nextFileName + ext);
                    if (filesInDir.Contains(nextFileName))
                    {
                        if (indexLength == 1)
                        {
                            return Path.Combine(dir, $"{leftPart}%d{rightPart}{ext}");
                        }
                        return Path.Combine(dir, $"{leftPart}%0{indexLength}d{rightPart}{ext}");
                    }
                }
            }
            return null;
        }

        public static string GetTempDir()
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

        /// <summary>
        /// 生成输出路径
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string GenerateOutputPath(TaskInfo task)
        {
            string output = task.Output.Trim();
            var a = task.Arguments;
            if (string.IsNullOrEmpty(output))
            {
                if (task.Inputs.Count == 0)
                {
                    throw new Exception("没有指定输出路径，且输入文件为空");
                }
                output = task.Inputs[0].FilePath;
            }

            //删除非法字符
            string dir = Path.GetDirectoryName(output);
            string filename = Path.GetFileName(output);
            if (filename.Any(p => invalidFileNameChars.Contains(p)))
            {
                foreach (var c in invalidFileNameChars)
                {
                    filename = filename.Replace(c.ToString(), "");
                }
                output = Path.Combine(dir, filename);
            }


            //修改扩展名
            if (!string.IsNullOrEmpty(a?.Format))
            {
                VideoFormat format = VideoFormat.Formats.Where(p => p.Name == a.Format || p.Extension == a.Format).FirstOrDefault();
                if (format != null)
                {
                    string name = Path.GetFileNameWithoutExtension(output);
                    string extension = format.Extension;
                    output = Path.Combine(dir, name + "." + extension);
                }
            }

            //获取非重复文件名
            task.RealOutput = FileSystem.GetNoDuplicateFile(output);

            //创建目录
            if (!new FileInfo(task.RealOutput).Directory.Exists)
            {
                new FileInfo(task.RealOutput).Directory.Create();
            }
            return task.RealOutput;
        }
    }
}