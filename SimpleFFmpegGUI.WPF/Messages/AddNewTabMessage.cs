using Microsoft.Win32;
using System;
using System.Windows.Controls;

namespace SimpleFFmpegGUI.WPF.Messages
{
    public class AddNewTabMessage(Type type, bool top = false)
    {
        public object Page { get; set; }
        public Type Type { get; } = type;

        public bool Top { get; set; } = top;
    }
}
