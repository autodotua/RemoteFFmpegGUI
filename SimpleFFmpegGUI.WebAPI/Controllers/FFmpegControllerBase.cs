using Furion.FriendlyException;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SimpleFFmpegGUI.Model;
using System.IO;
using System.Linq;

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
            PipeClient.EnsureInstance(config.GetValue<string>("PipeName") ?? throw new System.Exception("不存在PipeName配置项"));
            this.logger = logger;
            this.config = config;
        }

        protected string GetInputDir()
        {
            string path = config.GetValue<string>("InputDir");
            if (path == null)
            {
                throw Oops.Oh("没有配置输入文件夹");
            }
            return path;
        }

        protected string GetOutputDir()
        {
            string path = config.GetValue<string>("OutputDir");
            if (path == null)
            {
                throw Oops.Oh("没有配置输出文件夹");
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

        protected void CheckInputFileExist(string name)
        {
            if (!System.IO.File.Exists(Path.Combine(GetInputDir(), name)))
            {
                throw Oops.Oh("不存在文件" + name);
            }
        }

        protected void HideAbsolutePath(TaskInfo task)
        {
            if (task == null)
            {
                return;
            }
            task.Inputs = task.Inputs.Select(p => Path.GetFileName(p)).ToList();
            task.Output = Path.GetFileName(task.Output);
        }
    }
}