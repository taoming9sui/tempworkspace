using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GamePlatformServer.GameServer.ServerObjects;
using GamePlatformServer.Utils;
using Newtonsoft.Json.Linq;

namespace GamePlatformServer.GameServer.GameModuels
{
    public class Wolfman_P8 : GameModuel
    {
        private IDictionary<string, Player> m_playerSet;
        private Player[] m_players;

        public Wolfman_P8(GameServerContainer container) : base(container)
        {
            //玩家集合
            m_playerSet = new Dictionary<string, Player>();
            //玩家列表
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

        protected override void OnPlayerReconnect(string playerId)
        {
        }

        protected override void OnPlayerDisconnect(string playerId)
        {
        }

        protected override void OnPlayerJoin(string playerId, PlayerInfo info)
        {
            PlayerJoinGame(playerId, info);
        }

        protected override void OnPlayerLeave(string playerId)
        {
            PlayerLeaveGame(playerId);
        }

        protected override void OnPlayerMessage(string playerId, string msgData)
        {
            try
            {
                JObject jsonObj = JObject.Parse(msgData);
                string action = jsonObj.GetValue("Action").ToString();
                switch (action)
                {
                    case "RequestGameStatus":
                        PlayerRequestGameStatus(playerId);
                        break;
                    case "GetReady":
                        PlayerGetReady(playerId);
                        break;
                    case "CancelReady":
                        PlayerCancelReady(playerId);
                        break;
                }
            }
            catch (Exception ex) { LogHelper.LogError(ex.Message + "|" + ex.StackTrace); }
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
            public bool Aborted = false;
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

        private void PlayerJoinGame(string playerId, PlayerInfo info)
        {
            int seatNo = -1;
            for (int i = 0; i < m_players.Length; i++)
            {
                if (m_players[i] == null)
                {
                    seatNo = i;
                    break;
                }
            }
            if (seatNo == -1)
                throw new Exception(string.Format("{0}尝试加入一个已满的房间", playerId));

            //1创建对象
            Player newPlayer = new Player();
            newPlayer.PlayerId = info.Id;
            newPlayer.PlayerHeadNo = info.IconNo;
            newPlayer.PlayerName = info.Name;
            newPlayer.SeatNo = seatNo;
            newPlayer.isReady = false;
            newPlayer.Connected = true;
            newPlayer.Aborted = false;
            //2整理关系
            m_playerSet[playerId] = newPlayer;
            m_players[seatNo] = newPlayer;
            //3通知客户端
            JObject jsonObj = new JObject();
            jsonObj.Add("Action", "PlayerChange");
            JObject content = new JObject();
            {
                content.Add("Change", "Join");
                content.Add("Name", newPlayer.PlayerName);
                content.Add("HeadNo", newPlayer.PlayerHeadNo);
                content.Add("SeatNo", newPlayer.SeatNo);
            }
            jsonObj.Add("Content", content);
            BroadMessage(jsonObj.ToString());
        }
        private void PlayerLeaveGame(string playerId)
        {
            Player player = null;
            if (m_playerSet.TryGetValue(playerId, out player))
            {
                //1整理关系
                m_players[player.SeatNo] = null;
                m_playerSet.Remove(player.PlayerId);
                //2通知游戏客户端
                JObject jsonObj = new JObject();
                jsonObj.Add("Action", "PlayerChange");
                JObject content = new JObject();
                {
                    content.Add("Change", "Leave");
                    content.Add("SeatNo", player.SeatNo);
                }
                jsonObj.Add("Content", content);
                BroadMessage(jsonObj.ToString());
            }
        }
        private void PlayerRequestGameStatus(string playerId)
        {
            //构建JSON并返回
            JObject jsonObj = new JObject();
            jsonObj.Add("Action", "ResponseGameStatus");
            JObject content = new JObject();
            {
                //1玩家名片状态
                JArray playerHeadArray = new JArray();
                foreach (Player player in m_players)
                {
                    if (player != null)
                    {
                        JObject jobj = new JObject();
                        jobj.Add("SeatNo", player.SeatNo);
                        jobj.Add("Name", player.PlayerName);
                        jobj.Add("HeadNo", player.PlayerHeadNo);
                        playerHeadArray.Add(jobj);
                    }
                    else
                    {
                        playerHeadArray.Add(null);
                    }                  
                }
                content.Add("PlayerHeadArray", playerHeadArray);
            }
            jsonObj.Add("Content", content);
            SendMessage(playerId, jsonObj.ToString());
        }
        private void PlayerGetReady(string playerId)
        {
            Player player = null;
            if (m_playerSet.TryGetValue(playerId, out player))
            {
                //车准备好了
                player.isReady = true;
                //响应客户端
                {
                    JObject jsonObj = new JObject();
                    jsonObj.Add("Action", "GetReadyResponse");
                    SendMessage(playerId, jsonObj.ToString());
                }
                //广播变更
                {
                    JObject jsonObj = new JObject();
                    jsonObj.Add("Action", "PlayerChange");
                    JObject content = new JObject();
                    {
                        content.Add("Change", "Ready");
                        content.Add("SeatNo", player.SeatNo);
                        content.Add("IsReady", player.isReady);
                    }
                    jsonObj.Add("Content", content);
                    BroadMessage(jsonObj.ToString());
                }
            }
        }
        private void PlayerCancelReady(string playerId)
        {
            Player player = null;
            if (m_playerSet.TryGetValue(playerId, out player))
            {
                //车准备不好
                player.isReady = false;
                //响应客户端
                {
                    JObject jsonObj = new JObject();
                    jsonObj.Add("Action", "CancelReadyResponse");
                    SendMessage(playerId, jsonObj.ToString());
                }
                //广播变更
                {
                    JObject jsonObj = new JObject();
                    jsonObj.Add("Action", "PlayerChange");
                    JObject content = new JObject();
                    {
                        content.Add("Change", "Ready");
                        content.Add("SeatNo", player.SeatNo);
                        content.Add("IsReady", player.isReady);
                    }
                    jsonObj.Add("Content", content);
                    BroadMessage(jsonObj.ToString());
                }
            }
        }
        #endregion

    }
}
