using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GamePlatformServer.GameServer.ServerObjects;

namespace GamePlatformServer.GameServer.GameModuels
{
    public class Wolfman : GameModuel
    {
        public Wolfman(CenterRoom room) : base(room)
        {

        }

        public override string GameId { get { throw new NotImplementedException(); } }

        public override string GameName { get { throw new NotImplementedException(); } }

        public override int MaxPlayerCount { get { throw new NotImplementedException(); } }

        public override bool IsOpened { get { throw new NotImplementedException(); } }

        protected override void Begin()
        {
        }

        protected override void Finish()
        {
        }

        protected override void LogicUpdate()
        {
            throw new NotImplementedException();
        }

        protected override void OnPlayerConnect(string playerId)
        {
            throw new NotImplementedException();
        }

        protected override void OnPlayerDisconnect(string playerId)
        {
            throw new NotImplementedException();
        }

        protected override void OnPlayerJoin(string playerId)
        {
            throw new NotImplementedException();
        }

        protected override void OnPlayerLeave(string playerId)
        {
            throw new NotImplementedException();
        }

        protected override void OnPlayerMessage(string playerId, string msgData)
        {
            throw new NotImplementedException();
        }
    }
}
