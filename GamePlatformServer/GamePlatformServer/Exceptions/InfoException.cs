using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GamePlatformServer.Exceptions
{
    public class InfoException : Exception
    {
        public int ErrorCode { get; set; }

        public InfoException(string message, int errorCode = 0): base(message)
        {
            this.ErrorCode = errorCode;
        }
    }
}
