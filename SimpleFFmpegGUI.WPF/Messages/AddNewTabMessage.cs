using Microsoft.Win32;
using System;
using System.Windows.Controls;

namespace SimpleFFmpegGUI.WPF.Messages
{
    public class AddNewTabMessage(Type type, bool top = false, bool showWindow = false)
    {
        public object Page { get; set; }
        public Type Type { get; } = type;

        public bool Top { get; set; } = top;
        public bool ShowWindow { get; } = showWindow;
    }
}
