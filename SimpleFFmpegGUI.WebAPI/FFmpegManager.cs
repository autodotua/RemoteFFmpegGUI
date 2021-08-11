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
    public class FFmpegManager
    {
        public static FFmpegManager Instance { get; } = new FFmpegManager();
        private IIpcClient<IPipeService> mediaInfoClient;

        public FFmpegManager()
        {
            ServiceProvider serviceProvider = new ServiceCollection()
.AddNamedPipeIpcClient<IPipeService>("m", pipeName: "pipeinternal")
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