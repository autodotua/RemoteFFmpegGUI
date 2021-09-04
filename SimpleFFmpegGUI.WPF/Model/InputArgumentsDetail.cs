using FzLib;
using SimpleFFmpegGUI.Model;
using System.ComponentModel;

namespace SimpleFFmpegGUI.WPF.Model
{
    public class InputArgumentsDetail : InputArguments, ITempArguments
    {
        public void SetFile(string path)
        {
            FilePath = path;
            this.Notify(nameof(path));
        }

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

        public bool EnableFrom { get; set; }
        public bool EnableTo { get; set; }
        public bool EnableDuration { get; set; }
    }
}