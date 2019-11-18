using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebSocket.GameServer
{
    public class ClientSocketEventArgs: EventArgs
    {
        public enum MessageType { None, Server_Center, Server_Hall, Server_Room };

        public MessageType Type { get; set; }
        public string Data { get; set; }
    }
}
