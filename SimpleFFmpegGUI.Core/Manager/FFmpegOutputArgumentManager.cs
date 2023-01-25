using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Exceptions;
using FzLib.IO;
using SimpleFFmpegGUI.ConstantData;
using SimpleFFmpegGUI.FFMpegArgumentExtension;
using SimpleFFmpegGUI.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoCodec = SimpleFFmpegGUI.ConstantData.VideoCodec;

namespace SimpleFFmpegGUI.Manager
{
    internal class FFmpegOutputArgumentManager
    {
        public static void ApplyOutputArguments(FFMpegArgumentOptions fa, OutputArguments a, int twoPass)
        {
            if (a.DisableVideo && a.DisableAudio)
            {
                throw new FFMpegArgumentException("不能同时禁用视频和音频");
            }
            if (a.Video == null && a.Audio == null && !a.DisableAudio && !a.DisableVideo)
            {
                fa.CopyChannel(Channel.Both);
                return;
            }
            if (a.DisableVideo)
            {
                a.Video = null;
            }
            if (a.DisableAudio)
            {
                a.Audio = null;
            }

            if (a.Video == null)
            {
                if (a.DisableVideo)
                {
                    fa.DisableChannel(Channel.Video);
                }
                else
                {
                    fa.CopyChannel(Channel.Video);
                }
            }
            if (a.Audio == null || twoPass == 1)
            {
                if (a.DisableAudio || twoPass == 1)
                {
                    fa.DisableChannel(Channel.Audio);
                }
                else
                {
                    fa.CopyChannel(Channel.Audio);
                }
            }

            if (a.Video != null)
            {
                ApplyVideoArguments(fa, a, twoPass);
            }
            if (a.Audio != null)
            {
                ApplyAudioArguments(fa, a);
            }

            if (!string.IsNullOrWhiteSpace(a.Extra))
            {
                fa.WithArguments(a.Extra);
            }
            if (!string.IsNullOrWhiteSpace(a.Format))
            {
                fa.ForceFormat(a.Format);
            }
        }

        private static void ApplyAudioArguments(FFMpegArgumentOptions fa, OutputArguments a)
        {
            string code = a.Audio.Code?.ToLower()?.Replace(".", "") switch
            {
                "opus" => "libopus",
                "自动" => null,
                "auto" => null,
                "" => null,
                null => null,
                _ => a.Audio.Code
            };
            if (code != null)
            {
                fa.WithAudioCodec(code);
            }
            if (a.Audio.Bitrate.HasValue)
            {
                fa.WithAudioBitrate(a.Audio.Bitrate.Value);
            }
            if (a.Audio.SamplingRate.HasValue)
            {
                fa.WithAudioSamplingRate(a.Audio.SamplingRate.Value);
            }
        }

        private static void ApplyVideoArguments(FFMpegArgumentOptions fa, OutputArguments a, int twoPass)
        {
            string code = a.Video.Code?.ToUpper()?.Replace(".", "");
            string codecLib = null;
            if (VideoCodec.VideoCodecs.Any(p => p.Name.ToUpper() == code))
            {
                codecLib = VideoCodec.VideoCodecs.First(p => p.Name.ToUpper() == code).Lib;
            }
            else if (code is not ("自动" or "AUTO" or null))
            {
                codecLib = code.ToLower();
            }
            if (codecLib != null)
            {
                fa.WithVideoCodec(codecLib);
                fa.WithCpuSpeed(codecLib, a.Video.Preset);
            }
            if (a.Video.Crf.HasValue && twoPass == 0)
            {
                fa.WithConstantRateFactor(a.Video.Crf.Value);
            }

            if (codecLib == VideoCodec.AV1SVT.Lib)
            {
                if (a.Video.Crf.HasValue && a.Video.AverageBitrate.HasValue)
                {
                    throw new FFMpegArgumentException("SVTAV1编码器不支持同时指定CRF和码率");
                }
                if (a.Video.MaxBitrate.HasValue)
                {
                    throw new FFMpegArgumentException("SVTAV1编码器不支持指定最大码率");
                }
                if (a.Video.TwoPass)
                {
                    throw new FFMpegArgumentException("SVTAV1编码器不支持二次编码");
                }
                int width = int.MaxValue;
                int height = int.MaxValue;
                if (a.Video.Size != null)
                {
                    string[] parts = a.Video.Size.Split(':', 'x', '×');
                    if (parts.Length != 2 || !int.TryParse(parts[0], out width) || !int.TryParse(parts[1], out height))
                    {
                        throw new FFMpegArgumentException("SVTAV1编码器不支持的分辨率格式");
                    }
                }
                fa.WithArgument(new SvtAV1Argument(a.Video.AverageBitrate, a.Video.Fps, false,
                    width is > 0 and < int.MaxValue ? width : null, height is > 0 and < int.MaxValue ? height : null));
                return;
            }

            if (a.Video.Fps.HasValue)
            {
                fa.WithFramerate(a.Video.Fps.Value);
            }
            if (a.Video.AverageBitrate.HasValue)
            {
                fa.WithVideoMBitrate(a.Video.AverageBitrate.Value);
            }
            if (a.Video.MaxBitrate.HasValue && a.Video.MaxBitrateBuffer.HasValue)
            {
                fa.WithVideoMMaxBitrate(a.Video.MaxBitrate.Value, a.Video.MaxBitrateBuffer.Value);
            }
            if (a.Video.PixelFormat != null)
            {
                fa.WithVideoPixelFormat(a.Video.PixelFormat);
            }
            if (a.Video.AspectRatio != null)
            {
                fa.WithVideoAspect(a.Video.AspectRatio);
            }
            if (a.Video.TwoPass && twoPass == 0 || !a.Video.TwoPass && twoPass > 0)
            {
                throw new FFMpegArgumentException("2Pass参数不匹配");
            }
            if (twoPass > 0)
            {
                fa.WithTwoPass(code, twoPass);
            }
            if (a.Video.Size != null)
            {
                fa.WithVideoFilters(o =>
                {
                    o.Scale(a.Video.Size);
                });
            }
        }

        public static void GenerateOutputPath(TaskInfo task)
        {
            string output = task.Output.Trim();
            var a = task.Arguments;
            if (string.IsNullOrEmpty(output))
            {
                if (task.Inputs.Count == 0)
                {
                    throw new Exception("没有指定输出路径，且输入文件为空");
                }
                output = task.Inputs[0].FilePath;
            }
            if (!string.IsNullOrEmpty(a?.Format))
            {
                VideoFormat format = VideoFormat.Formats.Where(p => p.Name == a.Format || p.Extension == a.Format).FirstOrDefault();
                if (format != null)
                {
                    string dir = Path.GetDirectoryName(output);
                    string name = Path.GetFileNameWithoutExtension(output);
                    string extension = format.Extension;
                    output = Path.Combine(dir, name + "." + extension);
                }
            }
            task.RealOutput = FileSystem.GetNoDuplicateFile(output);
            if (!new FileInfo(task.RealOutput).Directory.Exists)
            {
                new FileInfo(task.RealOutput).Directory.Create();
            }
        }

    }
}
