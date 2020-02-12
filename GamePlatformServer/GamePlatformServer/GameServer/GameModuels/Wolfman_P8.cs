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
        private PlayerSeat[] m_players;
        private IDictionary<string, PlayerSeat> m_playerMapper;


        public Wolfman_P8(GameServerContainer container) : base(container)
        {
            //玩家集合
            m_playerMapper = new Dictionary<string, PlayerSeat>();
            //玩家列表
            m_players = new PlayerSeat[this.MaxPlayerCount];
            for (int i = 0; i < m_players.Length; i++)
            {
                PlayerSeat newSeat = new PlayerSeat();
                newSeat.SeatNo = i;
                m_players[i] = newSeat;
            }
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

        protected override void LogicUpdate(long milliseconds)
        {
        }
        #endregion

        #region 具体逻辑
        private class PlayerSeat
        {
            //基本资料
            public int SeatNo = 0;
            public bool HasPlayer = false;
            public bool Connected = false;
            public string PlayerId = "";
            public string PlayerName = "";
            public int PlayerHeadNo = 0;
            public bool isReady = false;
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
                if (!m_players[i].HasPlayer)
                {
                    seatNo = i;
                    break;
                }
            }
            if (seatNo == -1)
                throw new Exception(string.Format("{0}尝试加入一个已满的房间", playerId));

            //1更新对象
            PlayerSeat seat = m_players[seatNo];
            seat.HasPlayer = true;
            seat.PlayerId = info.Id;
            seat.PlayerHeadNo = info.IconNo;
            seat.PlayerName = info.Name;
            seat.SeatNo = seatNo;
            seat.isReady = false;
            seat.Connected = true;
            //2整理关系
            m_playerMapper[playerId] = seat;
            //3通知客户端
            JObject jsonObj = new JObject();
            jsonObj.Add("Action", "PlayerChange");
            JObject content = new JObject();
            {
                content.Add("Change", "Join");
                content.Add("Name", seat.PlayerName);
                content.Add("HeadNo", seat.PlayerHeadNo);
                content.Add("SeatNo", seat.SeatNo);
            }
            jsonObj.Add("Content", content);
            BroadMessage(jsonObj.ToString());
        }
        private void PlayerLeaveGame(string playerId)
        {
            PlayerSeat seat = null;
            if (m_playerMapper.TryGetValue(playerId, out seat))
            {
                //1整理关系
                PlayerSeat newSeat = new PlayerSeat();
                newSeat.SeatNo = seat.SeatNo;
                m_players[seat.SeatNo] = newSeat;
                m_playerMapper.Remove(seat.PlayerId);
                //2通知游戏客户端
                JObject jsonObj = new JObject();
                jsonObj.Add("Action", "PlayerChange");
                JObject content = new JObject();
                {
                    content.Add("Change", "Leave");
                    content.Add("SeatNo", seat.SeatNo);
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
                JArray playerSeatArray = new JArray();
                foreach (PlayerSeat seat in m_players)
                {
                    JObject jobj = new JObject();
                    jobj.Add("SeatNo", seat.SeatNo);
                    jobj.Add("HasPlayer", seat.HasPlayer);
                    jobj.Add("IsReady", seat.isReady);
                    jobj.Add("Name", seat.PlayerName);
                    jobj.Add("HeadNo", seat.PlayerHeadNo);
                    playerSeatArray.Add(jobj);                
                }
                content.Add("PlayerSeatArray", playerSeatArray);
            }
            jsonObj.Add("Content", content);
            SendMessage(playerId, jsonObj.ToString());
        }
        private void PlayerGetReady(string playerId)
        {
            PlayerSeat player = null;
            if (m_playerMapper.TryGetValue(playerId, out player))
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
            PlayerSeat player = null;
            if (m_playerMapper.TryGetValue(playerId, out player))
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
