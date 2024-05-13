using Furion.FriendlyException;
using FzLib;
using Microsoft.AspNetCore.Http;
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
    public class PresetController : FFmpegControllerBase
    {
        public PresetController(ILogger<MediaInfoController> logger,
            IConfiguration config,
        PipeClient pipeClient) : base(logger, config, pipeClient) { }

        [HttpGet]
        [Route("List")]
        public async Task<List<CodePreset>> GetPresets(TaskType? type)
        {
            if (type.HasValue)
            {
                return (await pipeClient.InvokeAsync(p => p.GetPresetsAsync())).Where(p => p.Type == type).ToList();
            }
            return await pipeClient.InvokeAsync(p => p.GetPresetsAsync());
        }

        [HttpPost]
        [Route("Add")]
        public Task<int> AddAsync([FromBody] CodePresetDto request)
        {
            CheckNull(request, "请求");
            return pipeClient.InvokeAsync(p => p.AddOrUpdatePresetAsync(request.Name, request.Type, request.Arguments));
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            await pipeClient.InvokeAsync(p => p.DeletePresetAsync(id));
            return Ok();
        }

        [HttpGet]
        [Route("Export")]
        public async Task<FileResult> ExportAsync()
        {
            string json = await pipeClient.InvokeAsync(p => p.ExportPresetsAsync());
            return File(json.ToUTF8Bytes(), "application/json");
        }

        [HttpPost, HttpOptions]
        [Route("Import")]
        public async Task<IActionResult> ImportAsync([FromQuery] IFormFile file)
        {
            using var s = file.OpenReadStream();
            byte[] buffer = new byte[s.Length];
            await s.ReadAsync(buffer, 0, buffer.Length);
            string json = buffer.ToUTF8String();
            await pipeClient.InvokeAsync(p => p.ImportPresetsAsync(json));
            return Ok();
        }
    }
}