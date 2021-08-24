using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleFFmpegGUI.WebAPI
{
    public class Program
    {
        internal static bool WebApp { get; private set; }
        internal static string PipeName { get; private set; }

        public static void Main(string[] args)
        {
            CreateHostBuilder().Build().Run();
        }

        public static void Main(int port, string pipeName)
        {
            PipeName = pipeName;
            WebApp = true;
            CreateHostBuilder($"http://localhost:{port}/").Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string url = null) =>
            Host.CreateDefaultBuilder()
            .ConfigureServices(c =>
            {
                c.AddSingleton<PipeClient>();
            })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    var builder = webBuilder
                        .Inject()
                        .UseStartup<Startup>();
                    if (url != null)
                    {
                        builder.UseUrls(url);
                    }
                });
    }
}