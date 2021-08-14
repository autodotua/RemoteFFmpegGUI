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

namespace SimpleFFmpegGUI.WebAPI
{
    public class FtpManager : IDisposable
    {
        public FtpManager(string path, int port)
        {
            var services = new ServiceCollection();

            // use %TEMP%/TestFtpServer as root folder
            services.Configure<DotNetFileSystemOptions>(opt => opt
                .RootPath = path);

            // Add FTP server services
            // DotNetFileSystemProvider = Use the .NET file system functionality
            // AnonymousMembershipProvider = allow only anonymous logins
            services.AddFtpServer(builder => builder
                .UseDotNetFileSystem() // Use the .NET file system functionality
                .EnableAnonymousAuthentication()); // allow anonymous logins

            // Configure the FTP server
            services.Configure<FtpServerOptions>(opt => opt.Port = port);

            // Build the service provider
            serviceProvider = services.BuildServiceProvider();
            // Initialize the FTP server
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