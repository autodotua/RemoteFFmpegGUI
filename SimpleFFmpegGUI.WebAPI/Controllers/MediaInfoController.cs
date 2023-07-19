using Furion.FriendlyException;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SimpleFFmpegGUI.Dto;
using SimpleFFmpegGUI.Manager;
using SimpleFFmpegGUI.Model.MediaInfo;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SimpleFFmpegGUI.WebAPI.Controllers
{
    public class MediaInfoController : FFmpegControllerBase
    {
        public MediaInfoController(ILogger<MediaInfoController> logger,
            IConfiguration config,
        PipeClient pipeClient) : base(logger, config, pipeClient) { }

        [HttpGet]
        public async Task<MediaInfoGeneral> GetAsync(string name)
        {
            CheckNull(name, "文件");
            string path = await CheckAndGetInputFilePathAsync(name);

            var result = await pipeClient.InvokeAsync(p => p.GetInfo(path));
            return result;
        }

        [HttpGet]
        [Route("Snapshot")]
        public async Task<IActionResult> GetSnapshotAsync(string videoPath, double seconds)
        {
            try
            {
                videoPath = await CheckAndGetInputFilePathAsync(videoPath);
                string path = await pipeClient.InvokeAsync(p => p.GetSnapshot(videoPath, seconds));

                if (CanAccessInputDir())
                {
                    return PhysicalFile(path, "image/jpeg");
                }
                else
                {
                    return File(await pipeClient.InvokeAsync(p => p.ReadFiles(path)), "image/jpeg");
                }
            }
            catch (Exception ex)
            {
                throw Oops.Oh("获取截图失败：" + ex.Message);
            }
        }
    }
}