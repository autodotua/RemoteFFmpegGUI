using FzLib;
using SimpleFFmpegGUI.Model;
using System.ComponentModel;

namespace SimpleFFmpegGUI.WPF.Model
{
    public class InputArgumentsDetail : InputArguments, ITempArguments
    {
        public void Update()
        {
            EnableFrom = From.HasValue;
            EnableTo = To.HasValue;
            EnableDuration = Duration.HasValue;
        }

        public void Apply()
        {
            From = EnableFrom ? From : null;
            To = EnableTo ? To : null;
            Duration = EnableDuration ? Duration : null;
        }

        private int index;

        public int Index
        {
            get => index;
            set => this.SetValueAndNotify(ref index, value, nameof(Index));
        }

        private bool canDelete;

        public bool CanDelete
        {
            get => canDelete;
            set => this.SetValueAndNotify(ref canDelete, value, nameof(CanDelete));
        }

        private bool enableFrom;

        public bool EnableFrom
        {
            get => enableFrom;
            set => this.SetValueAndNotify(ref enableFrom, value, nameof(EnableFrom));
        }

        private bool enableTo;

        public bool EnableTo
        {
            get => enableTo;
            set => this.SetValueAndNotify(ref enableTo, value, nameof(EnableTo));
        }

        private bool enableDuration;

        public bool EnableDuration
        {
            get => enableDuration;
            set => this.SetValueAndNotify(ref enableDuration, value, nameof(EnableDuration));
        }
    }
}