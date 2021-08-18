using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SimpleFFmpegGUI.WebAPI.Controllers
{
    public class TokenController : FFmpegControllerBase
    {
        public TokenController(ILogger<MediaInfoController> logger,
       IConfiguration config,
   PipeClient pipeClient) : base(logger, config, pipeClient) { }

        [HttpGet]
        [Route("Need")]
        public bool NeedToken()
        {
            return config.GetValue<string>("Token") != null;
        }

        [HttpGet]
        [Route("Check")]
        public bool CheckToken(string token)
        {
            return config.GetValue<string>("Token") == token;
        }
    }
}