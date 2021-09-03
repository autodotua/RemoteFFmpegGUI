using FzLib;
using Newtonsoft.Json;
using SimpleFFmpegGUI.Model;
using System;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace SimpleFFmpegGUI.Dto
{
    public class StatusDto : INotifyPropertyChanged
    {
        private static readonly Regex rFFmpegOutput = new Regex(
            @"frame= *(?<f>[0-9]+) *fps= *(?<fps>[0-9\.]+) *q= *(?<q>[0-9\.\-]+) *size= *(?<size>[0-9\.a-zA-Z]+) *time= *(?<time>[0-9\.:]+) *bitrate= *(?<b>[0-9\.a-z/]+).*speed= *(?<speed>[0-9\.]+)x", RegexOptions.Compiled);

        public StatusDto()
        {
        }

        public StatusDto(TaskInfo task)
        {
            Task = task;
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

                    if (progress != null)
                    {
                        if (!IsPaused)
                        {
                            progress.Update(Time);
                        }
                        HasDetail = true;
                    }
                    Progress = progress;
                }
                catch
                {
                }
            }
        }

        private bool hasDetail = false;

        public bool HasDetail
        {
            get => hasDetail;
            set => this.SetValueAndNotify(ref hasDetail, value, nameof(HasDetail));
        }

        private TaskInfo task;

        public TaskInfo Task
        {
            get => task;
            set => this.SetValueAndNotify(ref task, value, nameof(Task));
        }

        private ProgressDto progress;

        public ProgressDto Progress
        {
            get => progress;
            set => this.SetValueAndNotify(ref progress, value, nameof(Progress));
        }

        private bool isProcessing;

        public bool IsProcessing
        {
            get => isProcessing;
            set => this.SetValueAndNotify(ref isProcessing, value, nameof(IsProcessing));
        }

        private bool isPaused;

        public bool IsPaused
        {
            get => isPaused;
            set => this.SetValueAndNotify(ref isPaused, value, nameof(IsPaused));
        }

        private string lastOutput;

        public string LastOutput
        {
            get => lastOutput;
            set => this.SetValueAndNotify(ref lastOutput, value, nameof(LastOutput));
        }

        private int frame;

        public int Frame
        {
            get => frame;
            set => this.SetValueAndNotify(ref frame, value, nameof(Frame));
        }

        private double fps;

        public double Fps
        {
            get => fps;
            set => this.SetValueAndNotify(ref fps, value, nameof(Fps));
        }

        private string size;

        public string Size
        {
            get => size;
            set => this.SetValueAndNotify(ref size, value, nameof(Size));
        }

        private string bitrate;

        public string Bitrate
        {
            get => bitrate;
            set => this.SetValueAndNotify(ref bitrate, value, nameof(Bitrate));
        }

        private TimeSpan time;

        public TimeSpan Time
        {
            get => time;
            set => this.SetValueAndNotify(ref time, value, nameof(Time));
        }

        private string speed;

        public string Speed
        {
            get => speed;
            set => this.SetValueAndNotify(ref speed, value, nameof(Speed));
        }

        private double q;

        public event PropertyChangedEventHandler PropertyChanged;

        public double Q
        {
            get => q;
            set => this.SetValueAndNotify(ref q, value, nameof(Q));
        }
    }
}