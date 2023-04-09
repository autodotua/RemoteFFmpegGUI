using FFMpegCore;
using Mapster;
using Newtonsoft.Json.Linq;
using SimpleFFmpegGUI.Dto;
using SimpleFFmpegGUI.Model;
using SimpleFFmpegGUI.Model.MediaInfo;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SimpleFFmpegGUI.Manager
{
    public static class MediaInfoManager
    {
        public static async Task<MediaInfoGeneral> GetMediaInfoAsync(string path)
        {
            MediaInfoGeneral mediaInfo = null;
            await Task.Run(() =>
            {
                var mediaInfoJSON = GetMediaInfoProcessOutput(path);
                mediaInfo = ParseMediaInfoJSON(mediaInfoJSON);
                mediaInfo.Raw = mediaInfoJSON.ToString(Newtonsoft.Json.Formatting.Indented);
                foreach (var video in mediaInfo.Videos)
                {
                    if (!string.IsNullOrEmpty(video.Encoded_Library_Settings))
                    {
                        video.EncodingSettings = ParseEncodingSettings(video.Encoded_Library_Settings);
                    }
                }

            });
            return mediaInfo;
        }

        public static async Task<string> GetSnapshotAsync(string path, TimeSpan time)
        {
            string tempPath = System.IO.Path.GetTempFileName() + ".jpg";
            FFmpegProcess process = new FFmpegProcess($"-ss {time.TotalSeconds:0.000}  -i \"{path}\" -vframes 1 {tempPath}");
            await process.StartAsync(null, null);
            return tempPath;
        }

        public static async Task<TimeSpan> GetVideoDurationByFFprobeAsync(string path)
        {
            return (await FFProbe.AnalyseAsync(path)).Duration;
        }

        public static TimeSpan GetVideoDurationByFFprobe(string path)
        {
            return FFProbe.Analyse(path).Duration;
        }
        private static JObject GetMediaInfoProcessOutput(string path)
        {
            string tmpFile = System.IO.Path.GetTempFileName();
            var p = Process.Start(new ProcessStartInfo
            {
                FileName = "MediaInfo",
                Arguments = $"--output=JSON --BOM --LogFile=\"{tmpFile}\" \"{path}\"",
                CreateNoWindow = true,
            });
            p.WaitForExit();
            string output = System.IO.File.ReadAllText(tmpFile);
            return JObject.Parse(output);
        }

        private static List<MediaInfoItem> ParseEncodingSettings(string input)
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

        private static MediaInfoGeneral ParseMediaInfoJSON(JObject json)
        {
            MediaInfoGeneral info = null;
            var tracks = json["media"]["track"] as JArray;
            foreach (JObject track in tracks)
            {
                if (track["@type"].Value<string>() == "General")
                {
                    info = track.ToObject<MediaInfoGeneral>();
                }
                else if (track["@type"].Value<string>() == "Video")
                {
                    Debug.Assert(info != null);
                    info.Videos.Add(track.ToObject<MediaInfoVideo>());
                    info.Videos[^1].Index = info.Videos.Count;
                }
                else if (track["@type"].Value<string>() == "Audio")
                {
                    Debug.Assert(info != null);
                    info.Audios.Add(track.ToObject<MediaInfoAudio>());
                    info.Audios[^1].Index = info.Audios.Count;
                }
                else if (track["@type"].Value<string>() == "Text")
                {
                    Debug.Assert(info != null);
                    info.Texts.Add(track.ToObject<MediaInfoText>());
                    info.Texts[^1].Index = info.Texts.Count;
                }
            }
            return info;
        }
    }
}