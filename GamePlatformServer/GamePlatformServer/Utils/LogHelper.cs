using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;

namespace GamePlatformServer.Utils
{
    public class LogHelper
    {

        static readonly private ILog _infoLogger = log4net.LogManager.GetLogger("loginfo");
        static readonly private ILog _errorLogger = log4net.LogManager.GetLogger("logerror");

        static public void LogInfo(string message)
        {
            Console.WriteLine(string.Format("[{0}]{1}", DateTime.Now.ToString(), message));
            _infoLogger.Info(message);
        }

        static public void LogError(string message)
        {
            Console.WriteLine(string.Format("[{0}]{1}", DateTime.Now.ToString(), message));
            _errorLogger.Error(message);
        }
    }
}
