using Furion.FriendlyException;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SimpleFFmpegGUI.Model;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleFFmpegGUI.WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FFmpegControllerBase : ControllerBase
    {
        protected readonly ILogger<MediaInfoController> logger;
        protected readonly IConfiguration config;
        protected readonly PipeClient pipeClient;

        public FFmpegControllerBase(ILogger<MediaInfoController> logger,
        IConfiguration config,
        PipeClient pipeClient)
        {
            this.logger = logger;
            this.config = config;
            this.pipeClient = pipeClient;
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

        protected async Task CheckInputFileExistAsync(string name)
        {
            string path = Path.Combine(GetInputDir(), name);
            if (CanAccessInputDir())
            {
                if (!System.IO.File.Exists(path))
                {
                    throw Oops.Oh("不存在文件" + name);
                }
            }
            else
            {
                if (!await pipeClient.InvokeAsync(p => p.IsFileExist(path)))
                {
                    throw Oops.Oh("不存在文件" + name);
                }
            }
        }

        protected void HideAbsolutePath(TaskInfo task)
        {
            if (task == null)
            {
                return;
            }
            if (task.Inputs != null)
            {
                task.Inputs = task.Inputs.Select(p => Path.GetFileName(p)).ToList();
            }
            if (task.Output != null)
            {
                task.Output = Path.GetFileName(task.Output);
            }
        }

        protected bool CanAccessInputDir()
        {
            return config.GetValue("InputDirAccessable", true);
        }

        protected bool CanAccessOutputDir()
        {
            return config.GetValue("OutputDirAccessable", true);
        }
    }
}