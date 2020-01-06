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

        public override string GameId { get { return "Wolfman"; } }

        public override string GameName { get { return "狼人杀"; } }

        public override int MaxPlayerCount { get { return 12; } }

        public override bool IsOpened { get { return true; } }

        protected override void Begin()
        {

        }

        protected override void Finish()
        {

        }

        protected override void LogicUpdate()
        {
        }

        protected override void OnPlayerConnect(string playerId)
        {
        }

        protected override void OnPlayerDisconnect(string playerId)
        {
        }

        protected override void OnPlayerJoin(string playerId)
        {
        }

        protected override void OnPlayerLeave(string playerId)
        {
        }

        protected override void OnPlayerMessage(string playerId, string msgData)
        {
        }
    }
}
