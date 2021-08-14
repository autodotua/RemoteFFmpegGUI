using JKang.IpcServiceFramework.Client;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleFFmpegGUI.WebAPI
{
    public class PipeClient
    {
        public static PipeClient Instance { get; private set; }

        public static void EnsureInstance(string pipeName)
        {
            if (Instance == null)
            {
                Instance = new PipeClient(pipeName);
            }
        }

        private IIpcClient<IPipeService> mediaInfoClient;

        public PipeClient(string pipeName)
        {
            ServiceProvider serviceProvider = new ServiceCollection()
.AddNamedPipeIpcClient<IPipeService>("m", pipeName: pipeName)
.BuildServiceProvider();

            // resolve IPC client factory
            IIpcClientFactory<IPipeService> clientFactory = serviceProvider
                .GetRequiredService<IIpcClientFactory<IPipeService>>();

            // create client
            mediaInfoClient = clientFactory.CreateClient("m");
        }

        public Task<TResult> InvokeAsync<TResult>(Expression<Func<IPipeService, TResult>> exp)
        {
            return mediaInfoClient.InvokeAsync(exp);
        }

        public Task InvokeAsync(Expression<Action<IPipeService>> exp)
        {
            return mediaInfoClient.InvokeAsync(exp);
        }
    }
}