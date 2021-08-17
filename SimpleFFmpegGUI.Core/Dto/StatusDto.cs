using Newtonsoft.Json;
using SimpleFFmpegGUI.Model;
using System;
using System.Text.RegularExpressions;

namespace SimpleFFmpegGUI.Dto
{
    public class StatusDto
    {
        private static readonly Regex rFFmpegOutput = new Regex(
            @"frame= *(?<f>[0-9]+) *fps= *(?<fps>[0-9\.]+) *q= *(?<q>[0-9\.\-]+) *size= *(?<size>[0-9\.a-zA-Z]+) *time= *(?<time>[0-9\.:]+) *bitrate= *(?<b>[0-9\.a-z/]+).*speed= *(?<speed>[0-9\.]+)x", RegexOptions.Compiled);

        public StatusDto()
        {
        }

        public StatusDto(TaskInfo task, ProgressDto progress, string lastOutput, bool paused)
        {
            Task = task;
            LastOutput = lastOutput;
            IsProcessing = true;
            IsPaused = paused;
            if (lastOutput != null && rFFmpegOutput.IsMatch(lastOutput))
            {
                try
                {
                    var match = rFFmpegOutput.Match(lastOutput);
                    Frame = int.Parse(match.Groups["f"].Value);
                    Fps = double.Parse(match.Groups["fps"].Value);
                    Size = match.Groups["size"].Value.ToUpper();
                    Time = TimeSpan.Parse(match.Groups["time"].Value);
                    Bitrate = match.Groups["b"].Value;
                    Speed = match.Groups["speed"].Value;
                    Q = double.Parse(match.Groups["q"].Value);
                    Progress = progress;
                    if (progress != null)
                    {
                        if (!IsPaused)
                        {
                            progress.Update(Time);
                        }
                        HasDetail = true;
                    }
                }
                catch
                {
                }
            }
        }

        public bool HasDetail { get; set; } = false;
        public TaskInfo Task { get; set; }
        public ProgressDto Progress { get; set; }
        public bool IsProcessing { get; set; }
        public bool IsPaused { get; set; }
        public string LastOutput { get; set; }
        public int Frame { get; set; }
        public double Fps { get; set; }
        public string Size { get; set; }
        public string Bitrate { get; set; }
        public TimeSpan Time { get; set; }
        public string Speed { get; set; }
        public double Q { get; set; }
    }
}