namespace SimpleFFmpegGUI.WPF.ViewModels
{
    public static class ArgumentDescriptions
    {

        public static string AspectRatio => "视频宽度与高度的比例。可以输入宽:高或宽/高的数值。";
        public static string VideoAverageBitrate => "整个视频部分的目标比特率。平均码率可以帮助保持视频文件大小在一个合理的范围内。";
        public static string AudioAverageBitrate => "音频文件的目标比特率。平均码率可以帮助保持音频部分大小在一个合理的范围内。";
        public static string VideoCode => "视频的编码格式。压制速度方面，H264>H265≈VP9>AV1 (SVT)>AV1 (aom)；同等码率的画质方面，H264<H265≈VP9<AV1 (SVT)<AV1 (aom)。";
        public static string CRF => "一个用于控制视频质量的选项，特别是在使用可变比特率 (VBR) 编码时。CRF 值越小，画质越好。不同编码格式的CRF质检没有参考性。CRF 是一个相对恒定的质量模式，适用于在保证一定视觉质量的同时，让编码器自动调整输出比特率。";
        public static string FPS => "每秒显示的画面数量。较高的帧率可以使动作看起来更加流畅，但也可能导致较大的文件大小。";
        public static string MaxBitrate => "编码过程中允许达到的最大比特率。这有助于在网络带宽有限的情况下防止数据过载。当视频复杂度突然增加时，可能会短暂地达到这个上限。";
        public static string MaxBitrateBuffer => "用于控制视频码率超过最大码率的时间限制。值越大允许在遇到复杂场景时有更长时间超过视频的最大码率。";
        public static string PixelFormat => "描述了单个像素如何在数字信号中表示。常见的格式包括 YUV420p, YUV422p, YUV444p 等，它们决定了色彩采样率以及颜色深度等信息。";
        public static string Preset => "表示编码的速度与压缩效率之间的权衡。值越小，压制速度越慢，单位大小的视频片段质量越高；值越大，压制速度越快，单位大小的视频片段质量越低";
        public static string Size => "视频图像的尺寸。640:480表示640×480；640:-1表示宽度为640，高度自动调节；640:-2表示宽度为640，高度自动调节且调整到偶数；iw/2:ih/2表示视频分辨率缩放到原来的一半。";
        public static string TwoPass => "一种在编码视频时使用的策略，目的是为了更精确地控制视频的比特率，并且优化最终输出的视频质量。可以获得更好的比特率管理和视频质量，尤其是在恒定比特率（CBR）模式下。然而，这种方法的缺点是需要两次编码过程，因此会消耗更多的时间。";
        public static string AudioCode => "音频编码格式";
        public static string AudioBitrate => "音频比特率，单位为 kbps。较高的音频比特率通常意味着更好的音质，但也会导致更大的文件体积。";
        public static string SampleRate => "每秒钟对音频信号进行数字化采样的次数，单位通常是赫兹 (Hz)。常见的采样率有 44.1 kHz（用于 CD ）、48 kHz（DVD和网络视频）、以及 96 kHz等。采样率越高，音频信号的频率范围就越宽，理论上能够捕捉到的声音细节也就越多，音质也就越好。";
        public static string Format => "多媒体文件的封装格式，它可以包含一个或多个音频、视频、字幕轨道以及其他元数据。常用的有mp4（简单容器）、mkv（复杂容器）。容器的主要功能是将不同类型的媒体数据整合在一起，并提供一种方式来描述这些数据是如何组织和同步的。";
        public static string VideoOutput => "重新编码表示会对视频流重新计算，转码到新的编码、码率、分辨率等。复制表示仅复制数据，不重新计算，速度很快。不导出表示最终文件中不包含画面，只包含音频。";
        public static string AudioOutput => "重新编码表示会对音频流重新计算，转码到新的编码、码率、分辨率等。复制表示仅复制数据，不重新计算。不导出表示最终文件中不包含音频，只包含画面。";
        public static string Extra => "自定义的FFmpeg输出参数，这些参数将附加在输入文件后、输出文件前。";
        public static string SyncModifiedTime => "将输出文件的修改时间设置为最后一个输入文件的修改时间。";
        public static string DeleteInputFiles => "在处理完成后删除所有输入文件（优先删除到回收站）。";


    }
}