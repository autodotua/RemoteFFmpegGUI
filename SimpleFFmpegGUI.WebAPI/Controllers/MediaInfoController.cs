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
            if (string.IsNullOrEmpty(name))
            {
                throw Oops.Oh("文件名为空");
            }
            var path = config.GetValue<string>("MediaPath");
            if (path == null)
            {
                throw Oops.Oh("没有配置媒体文件夹");
            }
            if (!System.IO.File.Exists(Path.Combine(path, name)))
            {
                throw Oops.Oh("不存在文件" + name);
            }
            var result = await FFmpegManager.Instance.InvokeAsync(p => p.GetInfo(Path.Combine(path, name)));
            return result;
        }
    }
}