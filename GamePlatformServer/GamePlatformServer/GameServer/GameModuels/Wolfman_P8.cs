using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GamePlatformServer.GameServer.ServerObjects;

namespace GamePlatformServer.GameServer.GameModuels
{
    public class Wolfman_P8 : GameModuel
    {
        private Player[] m_players;

        public Wolfman_P8(CenterRoom room) : base(room)
        {
            //玩家集合
            m_players = new Player[this.MaxPlayerCount];

        }

        #region 游戏规格信息
        public override string GameId { get { return "Wolfman_P8"; } }

        public override string GameName { get { return "狼人杀8人"; } }

        public override int MaxPlayerCount { get { return 8; } }

        public override bool IsOpened { get { return true; } }
        #endregion

        #region 游戏框架事件
        protected override void Begin()
        {

        }

        protected override void Finish()
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

        protected override void LogicUpdate(long milliseconds)
        {
        }
        #endregion

        #region 具体逻辑
        private class Player
        {
            //基本资料
            public string PlayerId = "";
            public string PlayerName = "";
            public int PlayerHeadNo = 0;
            //房间布局资料
            public int SeatNo = 0;
            public bool isReady = false;
            public bool Connected = false;
            //狼人杀资料
            public GameIdentity Identity = null;
        }
        private class GameIdentity
        {
            public bool isDead = false;
            public string IdentityType = "Default";
            public IList<object> OperationFlags = new List<object>();
            public int GameCamp = 0;
        }
        private class Villager : GameIdentity
        { }
        private class Wolfman : GameIdentity
        { }
        private class Prophet : GameIdentity
        { }
        private class Hunter : GameIdentity
        { }
        private class Defender : GameIdentity
        { }
        private class Witch : GameIdentity
        {
            public bool Poison = true;
            public bool Antidote = true;
        }

        #endregion

    }
}
