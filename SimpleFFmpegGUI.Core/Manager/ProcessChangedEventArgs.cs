using System;

namespace SimpleFFmpegGUI.Manager
{
    public class ProcessChangedEventArgs : EventArgs
    {
        public ProcessChangedEventArgs(FFmpegProcess oldProcess, FFmpegProcess newProcess)
        {
            OldProcess = oldProcess;
            NewProcess = newProcess;
        }

        public FFmpegProcess NewProcess { get; }
        public FFmpegProcess OldProcess { get; }
    }
}