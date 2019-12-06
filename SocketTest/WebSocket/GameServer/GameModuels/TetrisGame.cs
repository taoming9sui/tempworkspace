using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSocket.GameServer.ServerObjects;

namespace WebSocket.GameServer.GameModuels
{
    public class TetrisGame : GameModuel
    {
        public TetrisGame(CenterRoom room) : base(room)
        {

        }

        public override string GameName => throw new NotImplementedException();

        public override int MaxPlayerCount => throw new NotImplementedException();

        public override bool IsOpened => throw new NotImplementedException();

        protected override void LogicUpdate()
        {
            throw new NotImplementedException();
        }

        protected override void OnPlayerConnect()
        {
            throw new NotImplementedException();
        }

        protected override void OnPlayerDisconnect()
        {
            throw new NotImplementedException();
        }

        protected override void OnPlayerJoin()
        {
            throw new NotImplementedException();
        }

        protected override void OnPlayerLeave()
        {
            throw new NotImplementedException();
        }

        protected override void OnPlayerMessage()
        {
            throw new NotImplementedException();
        }
    }
}
