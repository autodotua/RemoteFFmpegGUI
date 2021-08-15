using JKang.IpcServiceFramework;
using JKang.IpcServiceFramework.Client;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleFFmpegGUI.WebAPI
{
    public class PipeClient
    {
        private IIpcClient<IPipeService> mediaInfoClient;
        private readonly string hostName;
        private readonly string hostPath;
        private readonly string pipeName;
        private readonly IConfiguration config;

        public PipeClient(IConfiguration config)
        {
            pipeName = config.GetValue<string>("PipeName") ?? throw new Exception("不存在PipeName配置项");
            hostName = config.GetValue<string>("HostName", null);
            hostPath = config.GetValue<string>("HostPath", null);
            try
            {
                EnsureHost();
            }
            catch (Exception ex)
            {
            }
            ServiceProvider serviceProvider = new ServiceCollection()
.AddNamedPipeIpcClient<IPipeService>("m", pipeName: pipeName)
.BuildServiceProvider();

            // resolve IPC client factory
            IIpcClientFactory<IPipeService> clientFactory = serviceProvider
                .GetRequiredService<IIpcClientFactory<IPipeService>>();

            // create client
            mediaInfoClient = clientFactory.CreateClient("m");
            this.config = config;
        }

        public async Task<TResult> InvokeAsync<TResult>(Expression<Func<IPipeService, TResult>> exp)
        {
            try
            {
                return await mediaInfoClient.InvokeAsync(exp);
            }
            catch (IpcFaultException ex)
            {
                throw (ex.InnerException ?? ex).InnerException ?? ex.InnerException ?? ex;
            }
        }

        public async Task InvokeAsync(Expression<Action<IPipeService>> exp)
        {
            try
            {
                await mediaInfoClient.InvokeAsync(exp);
            }
            catch (IpcFaultException ex)
            {
                throw (ex.InnerException ?? ex).InnerException ?? ex.InnerException ?? ex;
            }
        }

        private void EnsureHost()
        {
            if (hostName != null)
            {
                var hosts = Process.GetProcessesByName(hostName);
                if (hosts.Length == 0)
                {
                    if (hostPath == null)
                    {
                        throw new Exception("服务提供进程未运行且没有配置路径");
                    }
                    Process p = new Process()
                    {
                        StartInfo = new ProcessStartInfo(hostPath, "-p " + pipeName)
                        {
                            UseShellExecute = true,
                        },
                    };
                    p.Start();
                }
            }
        }
    }
}