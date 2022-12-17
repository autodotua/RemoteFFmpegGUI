using FzLib;
using System.ComponentModel;

namespace SimpleFFmpegGUI.Model
{
    public class CodePreset : ModelBase, INotifyPropertyChanged
    {
        private bool @default;
        private OutputArguments arguments;
        private string name;
        private TaskType type;

        public event PropertyChangedEventHandler PropertyChanged;

        public OutputArguments Arguments
        {
            get => arguments;
            set => this.SetValueAndNotify(ref arguments, value, nameof(Arguments));
        }

        public bool Default
        {
            get => @default;
            set => this.SetValueAndNotify(ref @default, value, nameof(Default));
        }

        public string Name
        {
            get => name;
            set => this.SetValueAndNotify(ref name, value, nameof(Name));
        }
        public TaskType Type
        {
            get => type;
            set => this.SetValueAndNotify(ref type, value, nameof(Type));
        }
    }
}