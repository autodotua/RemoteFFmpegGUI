using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SimpleFFmpegGUI.Dto;
using SimpleFFmpegGUI.Model;
using System;
using System.Threading.Tasks;

namespace SimpleFFmpegGUI.WebAPI.Controllers
{
    public class PowerController : FFmpegControllerBase
    {
        public PowerController(ILogger<MediaInfoController> logger,
            IConfiguration config,
        PipeClient pipeClient) : base(logger, config, pipeClient) { }

        [HttpPost]
        [Route("Shutdown")]
        public async Task Shutdown()
        {
            await pipeClient.InvokeAsync(p => p.Shutdown());
        }
        [HttpPost]
        [Route("AbortShutdown")]
        public async Task AbortShutdown()
        {
            await pipeClient.InvokeAsync(p => p.AbortShutdown());
        }
        [HttpPost]
        [Route("ShutdownQueue")]
        public async Task SetShutdownAfterQueueFinished([FromForm] bool on)
        {
            await pipeClient.InvokeAsync(p => p.SetShutdownAfterQueueFinished(on));
        }
        [HttpGet]
        [Route("ShutdownQueue")]
        public async Task<bool> IsShutdownAfterQueueFinished()
        {
            return await pipeClient.InvokeAsync(p => p.IsShutdownAfterQueueFinished());
        }
    }
}