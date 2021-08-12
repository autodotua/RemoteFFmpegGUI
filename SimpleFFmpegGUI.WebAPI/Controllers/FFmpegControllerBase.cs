using Furion.FriendlyException;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO;

namespace SimpleFFmpegGUI.WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FFmpegControllerBase : ControllerBase
    {
        protected readonly ILogger<MediaInfoController> logger;
        protected readonly IConfiguration config;

        public FFmpegControllerBase(ILogger<MediaInfoController> logger,
        IConfiguration config)
        {
            this.logger = logger;
            this.config = config;
        }

        protected string GetMediaFolder()
        {
            string path = config.GetValue<string>("MediaPath");
            if (path == null)
            {
                throw Oops.Oh("没有配置媒体文件夹");
            }
            return path;
        }

        protected void CheckFileNameNull(string path)
        {
            if (path == null || path is string s && s == "")
            {
                throw Oops.Oh("文件名为空");
            }
        }

        protected void CheckNull(object obj, string objName)
        {
            if (obj == null || obj is string s && s == "")
            {
                throw Oops.Oh(objName + "为空");
            }
        }

        protected void CheckFileExist(string name)
        {
            if (!System.IO.File.Exists(Path.Combine(GetMediaFolder(), name)))
            {
                throw Oops.Oh("不存在文件" + name);
            }
        }
    }
}