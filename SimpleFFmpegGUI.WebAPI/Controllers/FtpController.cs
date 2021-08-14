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
    public class FtpController : FFmpegControllerBase
    {
        private FtpManager inputFtp;
        private FtpManager outputFtp;

        public FtpController(ILogger<MediaInfoController> logger,
            IConfiguration config) : base(logger, config) { }

        [HttpPost]
        [Route("Input/On")]
        public async Task OpenInput(int id)
        {
            if (inputFtp != null)
            {
                return;
            }
            inputFtp = new FtpManager(GetInputDir(), config.GetValue<int>("FtpPort", FtpManager.FreeTcpPort()));
            await inputFtp.StartAsync();
        }

        [HttpPost]
        [Route("Input/Off")]
        public async Task CloseInput(int id)
        {
            if (inputFtp == null)
            {
                return;
            }
            await inputFtp.StopAsync();
            inputFtp = null;
        }

        [HttpPost]
        [Route("Output/On")]
        public async Task OpenOutput(int id)
        {
            if (outputFtp != null)
            {
                return;
            }
            outputFtp = new FtpManager(GetOutputDir(), config.GetValue<int>("FtpPort", FtpManager.FreeTcpPort()));
            await outputFtp.StartAsync();
        }

        [HttpPost]
        [Route("Output/Off")]
        public async Task CloseOutput(int id)
        {
            if (outputFtp == null)
            {
                return;
            }
            await outputFtp.StopAsync();
            outputFtp = null;
        }
    }
}