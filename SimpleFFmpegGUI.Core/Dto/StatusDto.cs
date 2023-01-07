using FzLib;
using Newtonsoft.Json;
using SimpleFFmpegGUI.Model;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace SimpleFFmpegGUI.Dto
{
    public class StatusDto : INotifyPropertyChanged
    {
        private static readonly Regex rFFmpegOutput = new Regex(
            @"frame= *(?<f>[0-9]+) *fps= *(?<fps>[0-9\.]+) *(q= *(?<q>[0-9\.\-]+) *)+size= *(?<size>[0-9\.a-zA-Z]+) *time= *(?<time>[0-9\.\-:]+) *bitrate= *(?<b>([0-9\.a-z/]+)|(N/A)).*speed= *(?<speed>([0-9\.]+)|(N/A))x?", RegexOptions.Compiled);

        private string bitrate;

        private double fps;

        private int frame;

        private bool hasDetail = false;

        private bool isPaused;

        private bool isProcessing;

        private string lastOutput;

        private ProgressDto progress;

        private double q;

        private string size;

        private string speed;

        private TaskInfo task;

        private TimeSpan time;

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
                    if(Time< TimeSpan.Zero)
                    {
                        Time = TimeSpan.Zero;
                    }
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
                catch(Exception ex) 
                {
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public string Bitrate
        {
            get => bitrate;
            set => this.SetValueAndNotify(ref bitrate, value, nameof(Bitrate));
        }

        public double Fps
        {
            get => fps;
            set => this.SetValueAndNotify(ref fps, value, nameof(Fps));
        }

        public int Frame
        {
            get => frame;
            set => this.SetValueAndNotify(ref frame, value, nameof(Frame));
        }

        public bool HasDetail
        {
            get => hasDetail;
            set => this.SetValueAndNotify(ref hasDetail, value, nameof(HasDetail));
        }
        public bool IsPaused
        {
            get => isPaused;
            set => this.SetValueAndNotify(ref isPaused, value, nameof(IsPaused));
        }

        public bool IsProcessing
        {
            get => isProcessing;
            set => this.SetValueAndNotify(ref isProcessing, value, nameof(IsProcessing));
        }

        public string LastOutput
        {
            get => lastOutput;
            set => this.SetValueAndNotify(ref lastOutput, value, nameof(LastOutput));
        }

        public ProgressDto Progress
        {
            get => progress;
            set => this.SetValueAndNotify(ref progress, value, nameof(Progress));
        }

        public double Q
        {
            get => q;
            set => this.SetValueAndNotify(ref q, value, nameof(Q));
        }

        public string Size
        {
            get => size;
            set => this.SetValueAndNotify(ref size, value, nameof(Size));
        }

        public string Speed
        {
            get => speed;
            set => this.SetValueAndNotify(ref speed, value, nameof(Speed));
        }

        public TaskInfo Task
        {
            get => task;
            set => this.SetValueAndNotify(ref task, value, nameof(Task));
        }
        public TimeSpan Time
        {
            get => time;
            set => this.SetValueAndNotify(ref time, value, nameof(Time));
        }
    }
}