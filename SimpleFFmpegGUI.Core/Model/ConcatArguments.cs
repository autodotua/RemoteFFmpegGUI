using FzLib;
using System.ComponentModel;

namespace SimpleFFmpegGUI.Model
{
    public class ConcatArguments : INotifyPropertyChanged
    {
        private ConcatType type;

        public ConcatType Type
        {
            get => type;
            set => this.SetValueAndNotify(ref type, value, nameof(Type));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}