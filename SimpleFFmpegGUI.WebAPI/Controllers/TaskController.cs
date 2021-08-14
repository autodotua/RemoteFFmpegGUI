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
        [Route("List")]
        public async Task<PagedListDto<TaskInfo>> GetTasks(int status = 0, int skip = 0, int take = 0)
        {
            var tasks = await PipeClient.Instance.InvokeAsync(p => p.GetTasks(status == 0 ? null : (Model.TaskStatus)status, skip, take));

            tasks.ForEach(p => HideAbsolutePath(p));
            return tasks;
        }

        [HttpPost]
        [Route("Add/Code")]
        public async Task<int> AddCodeTaskAsync([FromBody] CodeTaskDto request)
        {
            if (request.Input == null || request.Input.Count() == 0 || request.Input.Any(p => p == null))
            {
                throw Oops.Oh("输入文件为空");
            }
            foreach (var file in request.Input)
            {
                CheckInputFileExist(file);
            }
            CheckFileNameNull(request.Output);
            return await PipeClient.Instance.InvokeAsync(p =>
              p.AddCodeTask(request.Input.Select(p => Path.Combine(GetInputDir(), p)),
              Path.Combine(GetOutputDir(), request.Output),
              request.Argument,
              request.Start));
        }

        [HttpPost]
        [Route("Reset")]
        public async Task ResetTaskAsync(int id)
        {
            await PipeClient.Instance.InvokeAsync(p => p.ResetTask(id));
        }

        [HttpPost]
        [Route("Reset/List")]
        public async Task ResetTasksAsync(IEnumerable<int> ids)
        {
            await PipeClient.Instance.InvokeAsync(p => p.ResetTasks(ids));
        }

        [HttpPost]
        [Route("Cancel")]
        public async Task CancelTaskAsync(int id)
        {
            await PipeClient.Instance.InvokeAsync(p => p.CancelTask(id));
        }

        [HttpPost]
        [Route("Cancel/List")]
        public async Task CancelTasksAsync(IEnumerable<int> ids)
        {
            await PipeClient.Instance.InvokeAsync(p => p.CancelTasks(ids));
        }

        [HttpPost]
        [Route("Delete")]
        public async Task DeleteTaskAsync(int id)
        {
            await PipeClient.Instance.InvokeAsync(p => p.DeleteTask(id));
        }

        [HttpPost]
        [Route("Delete/List")]
        public async Task DeleteTasksAsync(IEnumerable<int> ids)
        {
            await PipeClient.Instance.InvokeAsync(p => p.DeleteTasks(ids));
        }
    }
}