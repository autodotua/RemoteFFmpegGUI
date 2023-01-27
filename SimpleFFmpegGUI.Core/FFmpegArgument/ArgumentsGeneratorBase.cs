using SimpleFFmpegGUI.FFmpegLib;
using System.Collections.Generic;
using System.Linq;

namespace SimpleFFmpegGUI.FFmpegArgument
{
    public abstract class ArgumentsGeneratorBase
    {
        protected List<FFmpegArgumentItem> arguments = new List<FFmpegArgumentItem>();

        public virtual string GetArguments()
        {
            List<string> list = new List<string>();
            var groups = arguments.GroupBy(p => p.Parent).ToList();
            foreach (var group in groups)
            {
                if (group.Key == null)
                {
                    foreach (var arg in group)
                    {
                        list.Add($"-{arg.Key} {arg.Value}");
                    }
                }
                else
                {
                    List<string> subList = new List<string>();
                    foreach (var arg in group)
                    {
                        subList.Add($"{arg.Key}={arg.Value}");
                    }
                    list.Add($"-{group.Key} {string.Join(group.First().Seprator, subList)}");
                }
            }

            return string.Join(' ', list);
        }
    }
}
