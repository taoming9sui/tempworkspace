using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;

namespace WebSocket.Utils
{
    public class LogHelper
    {

        static readonly private ILog _infoLogger = log4net.LogManager.GetLogger("loginfo");
        static readonly private ILog _errorLogger = log4net.LogManager.GetLogger("logerror");

        static public void LogInfo(string message)
        {
            _infoLogger.Info(message);
        }

        static public void LogError(string message)
        {
            _errorLogger.Error(message);
        }
    }
}
