using FFMpegCore;
using Mapster;
using SimpleFFmpegGUI.Dto;
using SimpleFFmpegGUI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SimpleFFmpegGUI.Manager
{
    public static class MediaInfoManager
    {
        public static async Task<string> GetSnapshotAsync(string path, TimeSpan time)
        {
            string tempPath = System.IO.Path.GetTempFileName() + ".jpg";
            FFmpegProcess process = new FFmpegProcess($"-ss {time.TotalSeconds:0.000}  -i \"{path}\" -vframes 1 {tempPath}");
            await process.StartAsync(null, null);
            return tempPath;
        }

        public static async Task<MediaInfoDto> GetMediaInfoAsync(string path, bool includeDetail = true)
        {
            IMediaAnalysis result = null;
            try
            {
                result = await FFProbe.AnalyseAsync(path);
            }
            catch (Exception ex)
            {
                throw new Exception("查询信息失败：" + ex.Message);
            }
            var info = result.Adapt<MediaInfoDto>();
            if (includeDetail)
            {
                await Task.Run(() =>
                {
                    try
                    {
                        string mediaInfo = MediaInfoModule.GetInfo(path);
                        info.Detail = mediaInfo;
                        var encodingSettingsString = FindEncodingSettingsString(mediaInfo);
                        var streams = ParseMediaInfoOutput(mediaInfo);
                        if (encodingSettingsString != null)
                        {
                            var settings = ParseEncodingSettings(encodingSettingsString);
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                });
            }
            return info;
        }

        public static List<MediaInfoItem> ParseEncodingSettings(string input)
        {
            List<MediaInfoItem> settings = new List<MediaInfoItem>(); // 创建一个空列表来存储编码设置项
            var parts = input.Split('/').Select(p => p.Trim()); // 用"/"来分割输入字符串为一个字符串数组
            foreach (string part in parts) // 遍历数组中的每个字符串
            {
                MediaInfoItem setting = new MediaInfoItem(); // 创建一个新的编码设置项对象
                if (part.Contains('=')) // 检查字符串是否包含"="
                {
                    string[] pair = part.Split('='); // 用"="来分割字符串为两部分
                    setting.Name = pair[0]; // 把第一部分赋值给编码设置项的名称属性
                    string value = pair[1]; // 把第二部分作为一个字符串
                    if (int.TryParse(value, out int intValue)) // 尝试把值解析为一个整数
                    {
                        setting.Value = intValue; // 把整数值赋值给编码设置项的值属性
                    }
                    else if (double.TryParse(value, out double doubleValue)) // 尝试把值解析为一个双精度浮点数
                    {
                        setting.Value = doubleValue; // 把双精度浮点数赋值给编码设置项的值属性
                    }
                    else // 如果值不是一个数字
                    {
                        setting.Value = value; // 把字符串值赋值给编码设置项的值属性
                    }
                }
                else // 如果字符串不包含"="
                {
                    setting.Name = part; // 把整个字符串赋值给编码设置项的名称属性
                    setting.Value = true; // 把true赋值给编码设置项的值属性
                }
                settings.Add(setting); // 把编码设置项对象添加到列表中
            }
            return settings; // 返回编码设置项列表
        }

        private static VideoCodeArguments EncodingSettings2VideoArguments(List<MediaInfoItem> items)
        {
            Dictionary<string, bool> key2bools = items
                .Where(p => p.Value is bool)
                .ToDictionary(p => p.Name, p => (bool)p.Value);
            Dictionary<string, int> key2ints = items
                .Where(p => p.Value is int)
                .ToDictionary(p => p.Name, p => (int)p.Value);

            Dictionary<string, double> key2doubles = items
                            .Where(p => p.Value is double)
                            .ToDictionary(p => p.Name, p => (double)p.Value);

            Dictionary<string, string> key2strings = items
                            .Where(p => p.Value is string)
                            .ToDictionary(p => p.Name, p => (string)p.Value);

            VideoCodeArguments args = new VideoCodeArguments();
            throw new NotImplementedException();
        }

        private static string FindEncodingSettingsString(string str)
        {
            string pattern = @"Encoding settings\s*:\s*(.*)"; // 定义一个正则表达式来匹配编码设置项字符串
            Match match = Regex.Match(str, pattern); // 用正则表达式匹配字符串
            if (match.Success) // 如果匹配成功
            {
                return match.Groups[1].Value; // 返回匹配到的编码设置项字符串
            }
            return null; // 如果没有找到，返回null
        }
        
        public static List<MediaInfoStream> ParseMediaInfoOutput(string output)
        {
            // 初始化一个空的MediaInfoStream列表
            List<MediaInfoStream> streams = new List<MediaInfoStream>();

            // Split the output by line breaks
            string[] lines = output.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            // 初始化一个空引用，表示当前的流对象
            MediaInfoStream currentStream = null;

            // 遍历每一行
            foreach (string line in lines)
            {
                // 去掉行首和行尾的空白字符
                string trimmedLine = line.Trim();

                // 如果行为空，跳过
                if (string.IsNullOrEmpty(trimmedLine))
                {
                    continue;
                }

                // 如果行没有冒号，说明是一个流类型的名称
                if (!trimmedLine.Contains(":"))
                {
                    // 创建一个新的MediaInfoStream对象，以去掉空白字符的行作为名称
                    currentStream = new MediaInfoStream(trimmedLine);

                    // 把MediaInfoStream对象添加到列表中
                    streams.Add(currentStream);
                }
                else
                {
                    // 如果没有当前的流对象，跳过这一行
                    if (currentStream == null)
                    {
                        continue;
                    }

                    // 按第一个冒号分割这一行，得到流属性的名称和值
                    string[] parts = trimmedLine.Split(new[] { ":" }, 2, StringSplitOptions.None);

                    // 如果不是正好两个部分，跳过这一行
                    if (parts.Length != 2)
                    {
                        continue;
                    }

                    // 去掉部分首尾的空白字符
                    string name = parts[0].Trim();
                    string value = parts[1].Trim();

                    // 创建一个新的MediaInfoStreamItem对象，以名称和值作为参数
                    MediaInfoItem item = new MediaInfoItem(name, value);

                    // 把MediaInfoStreamItem对象添加到当前的MediaInfoStream对象中
                    currentStream.Add(item);
                }
            }

            // 返回MediaInfoStream列表
            return streams;
        }
    }
}