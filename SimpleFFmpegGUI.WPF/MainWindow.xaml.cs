using JKang.IpcServiceFramework.Client;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SimpleFFmpegGUI.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ServiceProvider serviceProvider = new ServiceCollection()
    .AddNamedPipeIpcClient<IPipeService>("client1", pipeName: "pipeinternal")
    .BuildServiceProvider();

            // resolve IPC client factory
            IIpcClientFactory<IPipeService> clientFactory = serviceProvider
                .GetRequiredService<IIpcClientFactory<IPipeService>>();

            // create client
            IIpcClient<IPipeService> client = clientFactory.CreateClient("client1");

            var output = client.InvokeAsync(p => p.GetInfo(@"C:\Users\autod\Desktop\0803\DJI_0146.MP4")).Result;
        }
    }
}