using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebSocket.GameServer.ServerObjects
{
    public class ServerPlayer
    {
        public string PlayerId { get; set; }
        public string InRoomId { get; set; }
        public string SocketId { get; set; }
        public PlayerInfo Info { get; set; }
    }
}
