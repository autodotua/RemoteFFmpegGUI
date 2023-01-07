using FFMpegCore;
using Mapster;
using SimpleFFmpegGUI.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFFmpegGUI.Manager
{
    public static class MediaInfoManager
    {
        public static async Task<string> GetSnapshotAsync(string path, TimeSpan time)
        {
            string tempPath = System.IO.Path.GetTempFileName() + ".jpg";
            FFmpegProcess process = new FFmpegProcess($"-ss {time.TotalSeconds:0.000}  -i \"{path}\" -vframes 1 {tempPath}");
            await process.StartAsync(null,null);
            return tempPath;
        }

        public async static Task<MediaInfoDto> GetMediaInfoAsync(string path, bool includeDetail = true)
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
                        info.Detail = MediaInfoModule.GetInfo(path);
                    }
                    catch (Exception ex)
                    {
                        info.Detail = ex.Message;
                    }
                });
            }
            return info;
        }
    }
}