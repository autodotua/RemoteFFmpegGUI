using Furion.FriendlyException;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SimpleFFmpegGUI.Dto;
using SimpleFFmpegGUI.Model.MediaInfo;
using System.IO;
using System.Threading.Tasks;

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
    }
}