using FubarDev.FtpServer;
using FubarDev.FtpServer.FileSystem.DotNet;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleFFmpegGUI
{
    internal class FtpManager : IDisposable
    {
        internal FtpManager(string path, int port)
        {
            if (port <= 0)
            {
                port = FreeTcpPort();
            }
            var services = new ServiceCollection();

            services.Configure<DotNetFileSystemOptions>(opt => opt
                .RootPath = path);

            services.AddFtpServer(builder => builder
                .UseDotNetFileSystem()
                .EnableAnonymousAuthentication());

            services.Configure<FtpServerOptions>(opt => opt.Port = port);

            serviceProvider = services.BuildServiceProvider();
            ftpServerHost = serviceProvider.GetRequiredService<IFtpServerHost>();
            Path = path;
            Port = port;
        }

        private readonly IFtpServerHost ftpServerHost;
        private readonly ServiceProvider serviceProvider;

        public string Path { get; }
        public int Port { get; }

        public Task StartAsync()
        {
            if (ftpServerHost == null)
            {
                throw new NullReferenceException("请先初始化");
            }
            return ftpServerHost.StartAsync(CancellationToken.None);
        }

        public Task StopAsync()
        {
            if (ftpServerHost == null)
            {
                throw new NullReferenceException("请先初始化");
            }
            return ftpServerHost.StopAsync(CancellationToken.None);
        }

        public static int FreeTcpPort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }

        public void Dispose()
        {
            serviceProvider?.Dispose();
        }

        ~FtpManager()
        {
            Dispose();
        }
    }
}