using System;
using System.Collections.Generic;
using System.Text;

namespace LazyPinger.Base.Common
{
    public class LazyLogger
    {
        public static void LogAll(string msg, LogSeverity severity)
        {
            Console.WriteLine($"{DateTime.Now}:{severity}:{msg}");
        }
    }

    public enum LogSeverity
    {
        Info, 
        Warning, 
        Error
    }
}
