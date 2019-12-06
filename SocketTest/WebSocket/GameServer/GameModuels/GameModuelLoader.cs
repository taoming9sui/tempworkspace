using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSocket.GameServer.ServerObjects;

namespace WebSocket.GameServer.GameModuels
{
    public class GameModuelLoader
    {
        static public GameModuel GetGameInstance(string gameId, CenterRoom room)
        {
            switch (gameId)
            {
                default:
                    return new TetrisGame(room);
            }
        }
    }
}
