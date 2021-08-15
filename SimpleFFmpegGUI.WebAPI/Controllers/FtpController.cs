using Furion.FriendlyException;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SimpleFFmpegGUI.WebAPI.Dto;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleFFmpegGUI.WebAPI.Controllers
{
    public class FtpController : FFmpegControllerBase
    {
        private static FtpManager inputFtp;
        private static FtpManager outputFtp;

        public FtpController(ILogger<MediaInfoController> logger,
            IConfiguration config,
        PipeClient pipeClient) : base(logger, config, pipeClient) { }

        [HttpPost]
        [Route("Input/On")]
        public async Task OpenInput()
        {
            if (inputFtp != null)
            {
                return;
            }
            inputFtp = new FtpManager(GetInputDir(), config.GetValue<int>("InputFtpPort", FtpManager.FreeTcpPort()));
            await inputFtp.StartAsync();
        }

        [HttpPost]
        [Route("Input/Off")]
        public async Task CloseInput()
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
        public async Task OpenOutput()
        {
            if (outputFtp != null)
            {
                return;
            }
            outputFtp = new FtpManager(GetOutputDir(), config.GetValue<int>("OutputFtpPort", FtpManager.FreeTcpPort()));
            await outputFtp.StartAsync();
        }

        [HttpPost]
        [Route("Output/Off")]
        public async Task CloseOutput()
        {
            if (outputFtp == null)
            {
                return;
            }
            await outputFtp.StopAsync();
            outputFtp = null;
        }

        /// <summary>
        /// 获取FTP状态
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Status")]
        public FtpStatusDto GetStatus(int id)
        {
            var status = new FtpStatusDto()
            {
                InputOn = inputFtp != null,
                OutputOn = outputFtp != null,
                InputPort = inputFtp?.Port ?? 0,
                OutputPort = outputFtp?.Port ?? 0
            };
            return status;
        }
    }
}