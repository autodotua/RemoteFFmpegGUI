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
    public class PresetController : FFmpegControllerBase
    {
        public PresetController(ILogger<MediaInfoController> logger,
            IConfiguration config) : base(logger, config) { }

        [HttpGet]
        [Route("List")]
        public Task<List<CodePreset>> GetPresets()
        {
            return PipeClient.Instance.InvokeAsync(p => p.GetPresets());
        }

        [HttpPost]
        [Route("Add")]
        public Task<int> AddAsync([FromBody] CodePresetDto request)
        {
            CheckNull(request, "请求");
            return PipeClient.Instance.InvokeAsync(p => p.AddOrUpdatePreset(request.Name, TaskType.Code, request.Arguments));
        }

        [HttpPost]
        [Route("Delete")]
        public async Task DeleteAsync(int id)
        {
            await PipeClient.Instance.InvokeAsync(p => p.DeletePreset(id));
        }
    }
}