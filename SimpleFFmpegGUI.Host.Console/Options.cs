using CommandLine;

namespace SimpleFFmpegGUI
{
    internal class Options
    {
        [Option('p', Required = false, HelpText = "命名管道名称")]
        public string PipeName { get; set; }

        [Option('d', Default = false, Required = false, HelpText = "设置工作目录为程序所在目录")]
        public bool WorkingDirectoryHere { get; set; }
    }
}