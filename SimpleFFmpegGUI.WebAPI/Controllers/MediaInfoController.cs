using Furion.FriendlyException;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SimpleFFmpegGUI.Dto;
using System.IO;
using System.Threading.Tasks;

namespace SimpleFFmpegGUI.WebAPI.Controllers
{
    public class MediaInfoController : FFmpegControllerBase
    {
        public MediaInfoController(ILogger<MediaInfoController> logger,
            IConfiguration config) : base(logger, config) { }

        [HttpGet]
        public async Task<MediaInfoDto> GetAsync(string name)
        {
            CheckNull(name, "文件");
            CheckInputFileExist(name);

            var result = await PipeClient.Instance.InvokeAsync(p => p.GetInfo(Path.Combine(GetInputDir(), name)));
            return result;
        }
    }
}