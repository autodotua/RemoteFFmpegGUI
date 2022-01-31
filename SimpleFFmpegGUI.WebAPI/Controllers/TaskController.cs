﻿using Furion.FriendlyException;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SimpleFFmpegGUI.Dto;
using SimpleFFmpegGUI.Model;
using SimpleFFmpegGUI.WebAPI.Dto;
using System.Collections.Generic;
using System.Diagnostics;
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

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetTask(int id)
        {
            var task = await pipeClient.InvokeAsync(p => p.GetTask(id));
            if (task == null)
            {
                return NotFound();
            }
            return Ok(HideAbsolutePath(task));
        }

        private string GetOutput(TaskDto request, int inputIndex)
        {
            Debug.Assert(inputIndex >= 0 && inputIndex < request.Inputs.Count);
            string output = request.Output;
            if (string.IsNullOrWhiteSpace(output))
            {
                if (request.Inputs.Count > 0)
                {
                    output = Path.Combine(OutputDir, Path.GetFileName(request.Inputs[inputIndex].FilePath));
                }
            }
            else
            {
                output = Path.Combine(OutputDir, request.Output);
            }

            return output;
        }

        [HttpPost]
        [Route("Add/Code")]
        public async Task<List<int>> AddCodeTaskAsync([FromBody] TaskDto request)
        {
            if (request.Inputs == null || request.Inputs.Count() == 0
                || request.Inputs.Any(p => string.IsNullOrEmpty(p.FilePath)))
            {
                throw Oops.Oh("输入文件为空");
            }
            List<int> ids = new List<int>();

            for (int i = 0; i < request.Inputs.Count; i++)
            {
                var file = request.Inputs[i];
                //检查输入文件存在
                await CheckInputFileExistAsync(file.FilePath);
                //拼接输入路径
                file.FilePath = Path.Combine(InputDir, file.FilePath);

                ids.Add(await pipeClient.InvokeAsync(p =>
                 p.AddTask(TaskType.Code,
                 new List<InputArguments>() { file },
              GetOutput(request, i),
                 request.Argument)));
            }
            if (request.Start)
            {
                await pipeClient.InvokeAsync(p => p.StartQueue()).ConfigureAwait(false);
            }
            return ids;
        }

        [HttpPost]
        [Route("Add/Concat")]
        public async Task<List<int>> AddConcatTaskAsync([FromBody] TaskDto request)
        {
            if (request.Inputs == null || request.Inputs.Count() < 2
                || request.Inputs.Any(p => string.IsNullOrEmpty(p.FilePath)))
            {
                throw Oops.Oh("输入文件为空或少于2个");
            }
            List<int> ids = new List<int>();

            foreach (var file in request.Inputs)
            {
                await CheckInputFileExistAsync(file.FilePath);
                file.FilePath = Path.Combine(InputDir, file.FilePath);
            }
            ids.Add(await pipeClient.InvokeAsync(p =>
                 p.AddTask(TaskType.Concat,
                 request.Inputs,
                 GetOutput(request, 0),
                 request.Argument)));
            if (request.Start)
            {
                await pipeClient.InvokeAsync(p => p.StartQueue()).ConfigureAwait(false);
            }
            return ids;
        }

        [HttpPost]
        [Route("Add/Combine")]
        public async Task<int> AddCombineTaskAsync([FromBody] TaskDto request)
        {
            if (request.Inputs == null || request.Inputs.Count() == 0 || request.Inputs.Any(p => string.IsNullOrEmpty(p.FilePath)))
            {
                throw Oops.Oh("输入文件为空");
            }
            if (request.Inputs.Count() != 2)
            {
                throw Oops.Oh("输入文件必须为2个");
            }
            foreach (var file in request.Inputs)
            {
                await CheckInputFileExistAsync(file.FilePath);
                file.FilePath = Path.Combine(InputDir, file.FilePath);
            }
            request.Inputs.ForEach(p => p.FilePath = Path.Combine(InputDir, p.FilePath));
            var id = await pipeClient.InvokeAsync(p =>
               p.AddTask(TaskType.Combine,
               request.Inputs,
              GetOutput(request, 0),
               request.Argument));
            if (request.Start)
            {
                await pipeClient.InvokeAsync(p => p.StartQueue()).ConfigureAwait(false);
            }
            return id;
        }

        [HttpPost]
        [Route("Add/Compare")]
        public async Task<int> AddCompareTaskAsync([FromBody] TaskDto request)
        {
            if (request.Inputs == null || request.Inputs.Count() == 0 || request.Inputs.Any(p => string.IsNullOrEmpty(p.FilePath)))
            {
                throw Oops.Oh("输入文件为空");
            }
            if (request.Inputs.Count() != 2)
            {
                throw Oops.Oh("输入文件必须为2个");
            }
            foreach (var file in request.Inputs)
            {
                await CheckInputFileExistAsync(file.FilePath);
                file.FilePath = Path.Combine(InputDir, file.FilePath);
            }
            request.Inputs.ForEach(p => p.FilePath = Path.Combine(InputDir, p.FilePath));
            var id = await pipeClient.InvokeAsync(p =>
               p.AddTask(TaskType.Compare, request.Inputs, null, null));
            if (request.Start)
            {
                await pipeClient.InvokeAsync(p => p.StartQueue()).ConfigureAwait(false);
            }
            return id;
        }

        [HttpPost]
        [Route("Add/Custom")]
        public async Task<int> AddCustomTaskAsync([FromBody] TaskDto request)
        {
            CheckNull(request.Argument, "参数");
            CheckNull(request.Argument.Extra, "参数");
            var id = await pipeClient.InvokeAsync(p =>
               p.AddTask(TaskType.Custom, null, null,
               request.Argument));
            if (request.Start)
            {
                await pipeClient.InvokeAsync(p => p.StartQueue()).ConfigureAwait(false);
            }
            return id;
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