﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebSocket.Exceptions
{
    public class InfoException : Exception
    {
        public InfoException(string message): base(message)
        {
        }
    }
}
