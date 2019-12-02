using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebSocket.GameServer
{
    public class ServerPlayer
    {
        string PlayerId { get; set; }
        string InRoomId { get; set; }
        string SocketId { get; set; }
        PlayerInfo Info { get; set; }
    }
}
