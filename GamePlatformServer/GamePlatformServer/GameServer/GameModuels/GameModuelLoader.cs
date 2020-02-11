using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GamePlatformServer.GameServer.ServerObjects;

namespace GamePlatformServer.GameServer.GameModuels
{
    public class GameModuelLoader
    {
        static public GameModuel GetGameInstance(string gameId, GameServerContainer container)
        {
            switch (gameId)
            {
                default:
                    return new Wolfman_P8(container);
            }
        }
    }
}
