using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GamePlatformServer.Exceptions
{
    public class InfoException : Exception
    {
        public string ResultCode { get; set; }

        public InfoException(string resultCode, string message = "") : base(message)
        {
            this.ResultCode = resultCode;
        }
    }
}
