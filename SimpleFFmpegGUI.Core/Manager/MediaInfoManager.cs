using FFMpegCore;
using Mapster;
using SimpleFFmpegGUI.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleFFmpegGUI.Manager
{
    public static class MediaInfoManager
    {
        public static MediaInfoDto GetMediaInfo(string path)
        {
            IMediaAnalysis result = null;
            try
            {
                result = FFProbe.Analyse(path);
            }
            catch (Exception ex)
            {
                throw new Exception("查询信息失败：" + ex.Message);
            }
            var info = result.Adapt<MediaInfoDto>();
            try
            {
                info.Detail = MediaInfoModule.GetInfo(path);
            }
            catch (Exception ex)
            {
                info.Detail = ex.Message;
            }
            return info;
        }
    }
}