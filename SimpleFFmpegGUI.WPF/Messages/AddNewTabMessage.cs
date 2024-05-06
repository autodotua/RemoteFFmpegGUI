using Microsoft.Win32;
using System;
using System.Windows.Controls;

namespace SimpleFFmpegGUI.WPF.Messages
{
    public class AddNewTabMessage(Type type) 
    {
        public object Page { get; set; }
        public Type Type { get; } = type;
    }
}
