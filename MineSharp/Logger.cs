using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MineSharp
{
    public class Logger
    {
        public const bool DEBUG_VERBOSE = true;
        public static void Log(string message, LogType type = LogType.Message)
        {
            ConsoleColor color = ConsoleColor.White;
            if (type == LogType.DebugVerbose)
                if (!DEBUG_VERBOSE) return;
                else type = LogType.Debug;
            switch (type)
            {
                case LogType.Message:
                    color = ConsoleColor.White;
                    break;
                case LogType.Warning:
                    color = ConsoleColor.Yellow;
                    break;
                case LogType.Error:
                    color = ConsoleColor.Red;
                    break;
                case LogType.Fatal:
                    color = ConsoleColor.DarkRed;
                    break;
                case LogType.Debug:
                    color = ConsoleColor.Cyan;
                    break;
            }
            Console.ForegroundColor = color;
            Console.Write("[" + type.ToString() + "] ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(message);
        }
    }
    public enum LogType
    {
        Message,
        Warning,
        Error,
        Fatal,
        Debug,
        DebugVerbose
    }
}
