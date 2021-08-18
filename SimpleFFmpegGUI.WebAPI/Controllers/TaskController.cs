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
            IConfiguration config,
        PipeClient pipeClient) : base(logger, config, pipeClient) { }

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
            var tasks = await pipeClient.InvokeAsync(p => p.GetTasks(status == 0 ? null : (Model.TaskStatus)status, skip, take));

            tasks.List.ForEach(p => HideAbsolutePath(p));
            return tasks;
        }

        [HttpPost]
        [Route("Add/Code")]
        public async Task<int> AddCodeTaskAsync([FromBody] TaskDto request)
        {
            if (request.Input == null || request.Input.Count() == 0 || request.Input.Any(p => p == null))
            {
                throw Oops.Oh("输入文件为空");
            }
            foreach (var file in request.Input)
            {
                await CheckInputFileExistAsync(file);
            }
            CheckFileNameNull(request.Output);
            return await pipeClient.InvokeAsync(p =>
              p.AddTask(TaskType.Code, request.Input.Select(p => Path.Combine(GetInputDir(), p)),
              Path.Combine(GetOutputDir(), request.Output),
              request.Argument,
              request.Start));
        }

        [HttpPost]
        [Route("Add/Combine")]
        public async Task<int> AddCombineTaskAsync([FromBody] TaskDto request)
        {
            if (request.Input == null || request.Input.Any(p => p == null))
            {
                throw Oops.Oh("输入文件为空");
            }
            if (request.Input.Count() != 2)
            {
                throw Oops.Oh("输入文件必须为2个");
            }
            foreach (var file in request.Input)
            {
                await CheckInputFileExistAsync(file);
            }
            CheckFileNameNull(request.Output);
            return await pipeClient.InvokeAsync(p =>
              p.AddTask(TaskType.Combine, request.Input.Select(p => Path.Combine(GetInputDir(), p)),
              Path.Combine(GetOutputDir(), request.Output),
              request.Argument,
              request.Start));
        }

        [HttpPost]
        [Route("Reset")]
        public async Task ResetTaskAsync(int id)
        {
            await pipeClient.InvokeAsync(p => p.ResetTask(id));
        }

        [HttpPost]
        [Route("Reset/List")]
        public async Task ResetTasksAsync(IEnumerable<int> ids)
        {
            await pipeClient.InvokeAsync(p => p.ResetTasks(ids));
        }

        [HttpPost]
        [Route("Cancel")]
        public async Task CancelTaskAsync(int id)
        {
            await pipeClient.InvokeAsync(p => p.CancelTask(id));
        }

        [HttpPost]
        [Route("Cancel/List")]
        public async Task CancelTasksAsync(IEnumerable<int> ids)
        {
            await pipeClient.InvokeAsync(p => p.CancelTasks(ids));
        }

        [HttpPost]
        [Route("Delete")]
        public async Task DeleteTaskAsync(int id)
        {
            await pipeClient.InvokeAsync(p => p.DeleteTask(id));
        }

        [HttpPost]
        [Route("Delete/List")]
        public async Task DeleteTasksAsync(IEnumerable<int> ids)
        {
            await pipeClient.InvokeAsync(p => p.DeleteTasks(ids));
        }

        [HttpGet]
        [Route("Formats")]
        public Task<VideoFormat[]> GetVideoFormats()
        {
            return pipeClient.InvokeAsync(p => p.GetSuggestedFormats());
        }
    }
}