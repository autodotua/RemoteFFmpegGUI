using FzLib;
using System.ComponentModel;

namespace SimpleFFmpegGUI.WPF.Model
{
    public class FormatArgumentWithSwitch : ITempArguments
    {
        private bool enableFormat = true;

        public bool EnableFormat
        {
            get => enableFormat;
            set => this.SetValueAndNotify(ref enableFormat, value, nameof(EnableFormat));
        }

        private string format = "mp4";

        public event PropertyChangedEventHandler PropertyChanged;

        public string Format
        {
            get => format;
            set => this.SetValueAndNotify(ref format, value, nameof(Format));
        }

        public void Apply()
        {
            Format = EnableFormat ? Format : null;
        }

        public void Update()
        {
            EnableFormat = Format != null;
        }
    }
}