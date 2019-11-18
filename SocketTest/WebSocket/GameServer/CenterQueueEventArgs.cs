using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSocketSharp.Server;

namespace WebSocket
{
    public class CenterQueueEventArgs : EventArgs
    {
        public enum MessageType { None, Client_Center, Client_Hall, Client_Room };

        public GameSocket Socket { get; set; }
        public MessageType Type { get; set; }
        public string Data { get; set; }
    }
}
