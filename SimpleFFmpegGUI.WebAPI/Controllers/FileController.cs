using Furion.FriendlyException;
using FzLib.IO;
using Microsoft.AspNetCore.Http;
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
    public class FileController : FFmpegControllerBase
    {
        private static FtpManager inputFtp;
        private static FtpManager outputFtp;

        public FileController(ILogger<MediaInfoController> logger,
            IConfiguration config,
        PipeClient pipeClient) : base(logger, config, pipeClient) { }

        [HttpGet]
        [Route("List/Input")]
        public IEnumerable<string> GetInputFiles()
        {
            return Directory.EnumerateFiles(GetInputDir()).Select(p => Path.GetFileName(p));
        }

        [HttpGet]
        [Route("List/Output")]
        public IEnumerable<FileDto> GetOutputFiles()
        {
            return Directory.EnumerateFiles(GetOutputDir()).Select(p => new FileDto(p));
        }
        [HttpGet]
        [Route("Download")]
        public IActionResult Download(string id)
        {
            if (FileDto.Guid2File.ContainsKey(id))
            {
                string path = FileDto.Guid2File[id];
                var stream = System.IO.File.OpenRead(path);
                return File(stream, "application/octet-stream", Path.GetFileName(path));
            }
            return NotFound();
        }

        [HttpPost]
        [Route("Ftp/Input/On")]
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
        [Route("Ftp/Input/Off")]
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
        [Route("Ftp/Output/On")]
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
        [Route("Ftp/Output/Off")]
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
        [Route("Ftp/Status")]
        public FtpStatusDto GetStatus()
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

        [HttpPost]
        [Route("Upload")]
        [HttpOptions]
        [DisableRequestSizeLimit]
        public async Task UploadFile([FromQuery] IFormFile file)
        {
            if (file.Length > 0)
            {
                string name = Path.Combine(GetInputDir(), file.FileName);
                name = FileSystem.GetNoDuplicateFile(name);
                using var stream = System.IO.File.Create(name);
                await file.CopyToAsync(stream);
            }
        }
    }
}