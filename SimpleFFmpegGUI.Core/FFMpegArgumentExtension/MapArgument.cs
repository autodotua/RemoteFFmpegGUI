using FFMpegCore.Arguments;
using FFMpegCore.Enums;
using System.Text;

namespace SimpleFFmpegGUI.FFMpegArgumentExtension
{
    public class MapArgument : IArgument
    {
        private readonly Channel channel;
        private readonly int inputIndex;
        private readonly int? streamIndex;

        public MapArgument( int inputIndex,Channel channel,int? streamIndex)
        {
            this.channel = channel;
            this.inputIndex = inputIndex;
            this.streamIndex = streamIndex;
        }
        public string Text
        {
            get
            {
                StringBuilder str = new StringBuilder();
                str.Append("-map ").Append(inputIndex);
                if (channel != Channel.Both)
                {
                    str.Append(':').Append(channel == Channel.Video ? 'v' : 'a');
                }
                if (streamIndex.HasValue)
                {
                    str.Append(':').Append(streamIndex.Value);
                }
                return str.ToString();
            }
        }
    }
}