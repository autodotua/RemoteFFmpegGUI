using Furion.FriendlyException;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SimpleFFmpegGUI.Dto;
using SimpleFFmpegGUI.Model;
using SimpleFFmpegGUI.WebAPI.Dto;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleFFmpegGUI.WebAPI.Controllers
{
    public class TaskController : FFmpegControllerBase
    {
        public TaskController(ILogger<MediaInfoController> logger,
            IConfiguration config) : base(logger, config) { }

        /// <summary>
        ///
        /// </summary>
        /// <param name="status">1：队列中；2：进行中；3：完成；4：错误；5：取消</param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        [HttpGet]
        public Task<List<TaskInfo>> Get(int status = 0, int skip = 0, int take = 0)
        {
            return FFmpegManager.Instance.InvokeAsync(p => p.GetTasks(status == 0 ? null : (Model.TaskStatus)status, skip, take));
        }

        [HttpGet]
        [Route("Status")]
        public async Task<StatusDto> GetStatus()
        {
            var status=await FFmpegManager.Instance.InvokeAsync(p => p.GetStatus());
            return status;
        }

        [HttpPost]
        [Route("Add/Code")]
        public async Task CreateCodeTaskAsync([FromBody] CodeTaskDto request)
        {
            if (request.Input == null || request.Input.Count() == 0 || request.Input.Any(p => p == null))
            {
                throw Oops.Oh("输入文件为空");
            }
            foreach (var file in request.Input)
            {
                CheckFileExist(file);
            }
            CheckNull(request.Output);
            CheckFileExist(request.Output);
            await FFmpegManager.Instance.InvokeAsync(p =>
             p.CreateCodeTask(request.Input.Select(p => Path.Combine(GetMediaFolder(), p)),
             Path.Combine(GetMediaFolder(), request.Output),
             request.Argument,
             request.Start));
        }

        [HttpPost]
        [Route("Start")]
        public async Task StartAsync()
        {
            await FFmpegManager.Instance.InvokeAsync(p => p.StartQueue());
        }

        [HttpPost]
        [Route("Pause")]
        public async Task PauseAsync()
        {
            await FFmpegManager.Instance.InvokeAsync(p => p.PauseQueue());
        }

        [HttpPost]
        [Route("Resume")]
        public async Task ResumeAsync()
        {
            await FFmpegManager.Instance.InvokeAsync(p => p.ResumeQueue());
        }

        [HttpPost]
        [Route("Cancel")]
        public async Task CancelAsync()
        {
            await FFmpegManager.Instance.InvokeAsync(p => p.CancelQueue());
        }

        [HttpPost]
        [Route("Reset")]
        public async Task ResetTaskAsync(int id)
        {
            await FFmpegManager.Instance.InvokeAsync(p => p.ResetTask(id));
        }
    }
}