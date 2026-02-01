using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace web.Utils
{
    public enum LogType 
    {
        ERROR = -1,
        INFO = 0,
        WARNING = 1,
    }
    public class Logger<T> where T : class
    {
        private readonly ILog _log;
        public Logger()
        {
            _log = LogManager.GetLogger(typeof(T));
        }

        public void LogDetails(LogType type, string message)
        {
            switch (type)
            {
                case LogType.ERROR:
                    _log.Error(message);
                    break;
                case LogType.INFO:
                    _log.Info(message);
                    break;
                case LogType.WARNING:
                    _log.Warn(message);
                    break;
                default:
                    _log.Fatal(message);
                    break;
            }
        }
    }
}