using JKang.IpcServiceFramework.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SimpleFFmpegGUI.WebAPI.Controllers
{
    public class MediaDirController : FFmpegControllerBase
    {
        public MediaDirController(ILogger<MediaInfoController> logger,
            IConfiguration config) : base(logger, config) { }

        [HttpGet]
        public IEnumerable<string> Get()
        {
            return Directory.EnumerateFiles(GetInputDir()).Select(p => Path.GetFileName(p));
        }
    }
}