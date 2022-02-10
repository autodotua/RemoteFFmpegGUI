using System;

namespace SimpleFFmpegGUI.WPF.Pages
{
    public interface ICloseablePage
    {
        public event EventHandler RequestToClose;
    }
}