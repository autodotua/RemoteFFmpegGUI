using FFMpegCore.Exceptions;
using SimpleFFmpegGUI.FFmpegLib;
using SimpleFFmpegGUI.Model;
using System.IO;
using System;
using System.Linq;
using FzLib.IO;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SimpleFFmpegGUI.FFmpegArgument
{
    public static class ArgumentsGenerator
    {
        public static string GetArguments(TaskInfo task, int pass, string output = null)
        {
            StringBuilder str = new StringBuilder();
            foreach (var input in task.Inputs)
            {
                str.Append(GetInputArguments(input));
                str.Append(' ');
            }
            str.Append(GetOutputArguments(task.Arguments, pass));
            str.Append(" \"");
            str.Append(output ?? task.RealOutput);
            str.Append("\" -y");
            return str.ToString();
        }
        public static string GetArguments(IEnumerable<InputArguments> inputs, string outputArguments, string output = null)
        {
            StringBuilder str = new StringBuilder();
            foreach (var input in inputs)
            {
                str.Append(GetInputArguments(input));
                str.Append(' ');
            }
            str.Append(outputArguments);
            str.Append(' ');
            str.Append(output ==null? "":$"\"{output}\"");
            str.Append(" -y");
            return str.ToString();
        }

        public static string GetInputArguments(InputArguments ia)
        {
            InputArgumentsGenerator ig = new InputArgumentsGenerator();
            ig.Seek(ia.From);
            ig.To(ia.To);
            ig.Duration(ia.Duration);
            ig.Format(ia.Format);
            ig.Input(ia.FilePath);
            return ia.Extra == null ? ig.GetArguments() : $"{ia.Extra}  {ig.GetArguments()}";
        }
        public static string GetOutputArguments(
            Func<VideoArgumentsGenerator, VideoArgumentsGenerator> video,
            Func<AudioArgumentsGenerator, AudioArgumentsGenerator> audio,
            Func<StreamArgumentsGenerator, StreamArgumentsGenerator> stream)
        {
            VideoArgumentsGenerator vg = new VideoArgumentsGenerator();
            AudioArgumentsGenerator ag = new AudioArgumentsGenerator();
            StreamArgumentsGenerator sg = new StreamArgumentsGenerator();
            vg = video(vg);
            ag = audio(ag);
            sg = stream(sg);

            return string.Join(' ', vg.GetArguments(), ag.GetArguments(), sg.GetArguments());

        }
        public static string GetOutputArguments(OutputArguments oa, int pass)
        {
            VideoArgumentsGenerator vg = new VideoArgumentsGenerator();
            AudioArgumentsGenerator ag = new AudioArgumentsGenerator();
            StreamArgumentsGenerator sg = new StreamArgumentsGenerator();
            if (oa.DisableVideo && oa.DisableAudio)
            {
                throw new FFmpegArgumentException("不能同时禁用视频和音频");
            }
            if (oa.DisableVideo)
            {
                vg.Disable();
            }
            else if (oa.Video == null)
            {
                vg.Copy();
            }
            else
            {
                vg.Codec(oa.Video.Code);
                vg.Speed(oa.Video.Preset);
                vg.CRF(oa.Video.Crf);
                vg.AverageBitrate(oa.Video.AverageBitrate);
                vg.MaxBitrate(oa.Video.MaxBitrate);
                if (oa.Video.MaxBitrate != null)
                {
                    vg.BufferRatio(oa.Video.MaxBitrateBuffer);
                }
                vg.Aspect(oa.Video.AspectRatio);
                vg.PixelFormat(oa.Video.PixelFormat);
                vg.FrameRate(oa.Video.Fps);
                vg.Scale(oa.Video.Size);
                vg.Pass(pass);
            }

            if (oa.DisableAudio || pass == 1)
            {
                ag.Disable();
            }
            else if (oa.Audio == null)
            {
                ag.Copy();
            }
            else
            {
                ag.Codec(oa.Audio.Code);
                ag.Bitrate(oa.Audio.Bitrate);
                ag.SamplingRate(oa.Audio.SamplingRate);
            }

            if (oa.Stream != null && oa.Stream.Maps != null && oa.Stream.Maps.Count > 0)
            {
                foreach (var map in oa.Stream.Maps)
                {
                    sg.Map(map.InputIndex, map.Channel, map.StreamIndex);
                }
            }

            string extra = "";
            if (oa.Format != null)
            {
                extra = $"-f {oa.Format}";
            }
            if (oa.Extra != null)
            {
                extra = $"{extra} {oa.Extra}";
            }

            return string.Join(' ', vg.GetArguments(), ag.GetArguments(), sg.GetArguments(), extra);
        }
    }
}
