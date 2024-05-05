using System;

namespace SimpleFFmpegGUI.WPF.Messages
{
    public class QueueMessagesMessage(char type, string message, Exception exception = null)
    {
        public char Type { get; } = type;
        public string Message { get; } = message;
        public Exception Exception { get; } = exception;
    }
}
