using Furion.FriendlyException;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SimpleFFmpegGUI.Model;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleFFmpegGUI.WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FFmpegControllerBase : ControllerBase
    {
        protected readonly ILogger<MediaInfoController> Logger;
        protected readonly IConfiguration config;
        protected readonly PipeClient pipeClient;

        public FFmpegControllerBase(ILogger<MediaInfoController> Logger,
        IConfiguration config,
        PipeClient pipeClient)
        {
            this.Logger = Logger;
            this.config = config;
            this.pipeClient = pipeClient;
            InputDir = config.GetValue<string>("InputDir") ?? throw Oops.Oh("没有配置输入文件夹");
            OutputDir = config.GetValue<string>("OutputDir") ?? throw Oops.Oh("没有配置输出文件夹");
        }

        public readonly string InputDir = null;
        public readonly string OutputDir = null;

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

        protected async Task<string> CheckAndGetInputFilePathAsync(string name)
        {
            if (name.StartsWith(':'))
            {
                name = name[1..];
                if (CanAccessInputDir())
                {
                    var files=Directory.EnumerateFiles(InputDir,name, SearchOption.AllDirectories);
                    if(!files.Any())
                    {
                        throw Oops.Oh("找不到文件" + name);
                    }
                    if(files.Count()>2)
                    {
                        throw Oops.Oh($"存在多个文件名为{name}的文件");
                    }
                    return files.First() ;
                }
                else
                {
                    try
                    {
                        return await pipeClient.InvokeAsync(p => p.GetSingleFileInDir(InputDir, name));
                    }
                    catch(Exception ex)
                    {
                      throw  Oops.Oh(ex.Message);
                    }
                }
            }
            else
            {
                string path = Path.Combine(InputDir, name);
                if (CanAccessInputDir())
                {
                    if (System.IO.File.Exists(path))
                    {
                        throw Oops.Oh("不存在文件" + path);
                    }
                    return path;
                }

                else
                {
                    if (!await pipeClient.InvokeAsync(p => p.IsFileExist(path)))
                    {
                        throw Oops.Oh("不存在文件" + path);
                    }
                    return path;
                }
            }
        } 
        

        protected TaskInfo HideAbsolutePath(TaskInfo task)
        {
            if (task == null)
            {
                return null;
            }
            if (task.Inputs != null)
            {
                foreach (var input in task.Inputs)
                {
                    input.FilePath = GetInputRelativePath(input.FilePath);
                }
            }
            if (task.Output != null)
            {
                task.Output = GetOutputRelativePath(task.Output);
            }
            return task;
        }

        protected string GetInputRelativePath(string path)
        {
            return path.StartsWith(InputDir) ?
                path.Substring(InputDir.Length).Replace('\\', '/').TrimStart('/')
                : path;
        }

        protected string GetOutputRelativePath(string path)
        {
            return path.StartsWith(OutputDir) ?
                path.Substring(OutputDir.Length).Replace('\\', '/').TrimStart('/')
                : path;
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