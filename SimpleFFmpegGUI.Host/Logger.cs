using SimpleFFmpegGUI.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleFFmpegGUI
{
    public static class Logger
    {
        private static ConsoleColor DefaultColor = Console.ForegroundColor;

        public static void Info(TaskInfo task, string message)
        {
            Info($"{message}：{task.Type}  {string.Join('+', task.Inputs)} ===> {task.Output}");
        }

        public static void Info(string message)
        {
            Console.ForegroundColor = DefaultColor;
            Console.WriteLine(message);
        }

        public static void Error(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
        }

        public static void Warn(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
        }
    }
}