using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using GamePlatformServer.GameServer.ServerObjects;
using GamePlatformServer.Utils;
using Newtonsoft.Json.Linq;

namespace GamePlatformServer.GameServer.GameModuels
{
    public class Wolfman_P8 : GameModuel
    {
        public Wolfman_P8(GameServerContainer container) : base(container)
        {
            //玩家集合
            m_playerMapper = new Dictionary<string, PlayerSeat>();
            //玩家列表
            m_playerSeats = new PlayerSeat[this.MaxPlayerCount];
            for (int i = 0; i < m_playerSeats.Length; i++)
            {
                PlayerSeat newSeat = new PlayerSeat();
                newSeat.SeatNo = i;
                m_playerSeats[i] = newSeat;
            }
            //游戏逻辑循环 
            m_gameFlowLoop = JudgeMainLoop();
        }

        #region 游戏规格信息
        public override string GameId { get { return "Wolfman_P8"; } }

        public override string GameName { get { return "狼人杀8人"; } }

        public override int MaxPlayerCount { get { return 8; } }

        public override bool IsOpened { get { return !m_isPlaying; } }
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
                case "SynchronizeGameCommand":
                    PlayerSynchronizeGame(playerId);
                    break;
                case "ReadyCommand":
                    PlayerReady(playerId, (bool)jsonObj.GetValue("Ready"));
                    break;
                case "IdentityExpectionCommand":
                    PlayerSetIdentityExpection(playerId, (string)jsonObj.GetValue("IdentityExpection"));
                    break;
            }
        }

        protected override void LogicUpdate(long milliseconds)
        {
            //主循环
            m_gameFlowLoop.MoveNext();
        }
        #endregion

        #region 具体逻辑
        private IDictionary<string, PlayerSeat> m_playerMapper;
        private IEnumerator<int> m_gameFlowLoop;
        private bool m_isPlaying = false;
        private string m_publicProgress = "";
        private int m_dayTime = 0;
        private int m_dayNumber = 0;
        private class PlayerSeat
        {
            //玩家字段  
            public string PlayerId = "";
            public string PlayerName = "";
            public int PlayerHeadNo = 0;
            //座位字段
            public int SeatNo = 0;
            public bool HasPlayer = false;
            public bool Connected = false;
            //狼人游戏字段
            public bool isReady = false;
            public GameIdentity Identity = null;
            public string IdentityExpection = "";
            public bool isSpeaking = false;
            public bool isVoting = false;
        }
        private PlayerSeat[] m_playerSeats;
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


        private void PlayerSynchronizeGame(string playerId)
        {
            PlayerSeat playerSeat = null;
            if (m_playerMapper.TryGetValue(playerId, out playerSeat))
            {
                //构建JSON并返回
                JObject jsonObj = new JObject();
                jsonObj.Add("Action", "ResponseGameStatus");
                JObject content = new JObject();
                {
                    //1玩家列表名片状态
                    JArray playerSeatArray = new JArray();
                    foreach (PlayerSeat seat in m_playerSeats)
                    {
                        JObject jobj = new JObject();
                        jobj.Add("SeatNo", seat.SeatNo);
                        jobj.Add("HasPlayer", seat.HasPlayer);
                        jobj.Add("IsReady", seat.isReady);
                        jobj.Add("Name", seat.PlayerName);
                        jobj.Add("HeadNo", seat.PlayerHeadNo);
                        jobj.Add("IsSpeaking", seat.isSpeaking);
                        playerSeatArray.Add(jobj);
                    }
                    content.Add("PlayerSeatArray", playerSeatArray);
                    //2游戏全局属性
                    JObject gameProperty = new JObject();
                    {
                        gameProperty.Add("IsPlaying", m_isPlaying);
                        gameProperty.Add("PublicProgress", m_publicProgress);
                        gameProperty.Add("DayTime", m_dayTime);
                        gameProperty.Add("DayNumber", m_dayNumber);
                    }
                    content.Add("GameProperty", gameProperty);
                    //3该玩家当前属性
                    JObject playerProperty = new JObject();
                    {
                        playerProperty.Add("PlayerId", playerSeat.PlayerId);
                        playerProperty.Add("PlayerName", playerSeat.PlayerName);
                        playerProperty.Add("PlayerHeadNo", playerSeat.PlayerHeadNo);
                        playerProperty.Add("SeatNo", playerSeat.SeatNo);
                        playerProperty.Add("IsReady", playerSeat.isReady);
                        playerProperty.Add("IsSpeaking", playerSeat.isSpeaking);
                        playerProperty.Add("IsVoting", playerSeat.isVoting);
                    }
                    content.Add("PlayerProperty", playerProperty);
                }
                jsonObj.Add("Content", content);
                SendMessage(playerId, jsonObj.ToString());
            }
        }
        private void PlayerJoinGame(string playerId, PlayerInfo info)
        {
            int seatNo = -1;
            for (int i = 0; i < m_playerSeats.Length; i++)
            {
                if (!m_playerSeats[i].HasPlayer)
                {
                    seatNo = i;
                    break;
                }
            }
            if (seatNo == -1)
                throw new Exception(string.Format("{0}尝试加入一个已满的房间", playerId));

            //1更新对象
            PlayerSeat seat = m_playerSeats[seatNo];
            seat.HasPlayer = true;
            seat.Connected = true;
            seat.PlayerId = info.Id;
            seat.PlayerHeadNo = info.IconNo;
            seat.PlayerName = info.Name;
            seat.isReady = false;
            seat.isSpeaking = false;
            //2整理关系
            m_playerMapper[playerId] = seat;
            //3通知客户端
            JObject jsonObj = new JObject();
            jsonObj.Add("Action", "SeatChange");
            JArray content = new JArray();
            {
                JObject changeObj = new JObject();
                changeObj.Add("Change", "Join");
                changeObj.Add("Name", seat.PlayerName);
                changeObj.Add("HeadNo", seat.PlayerHeadNo);
                changeObj.Add("SeatNo", seat.SeatNo);
                content.Add(changeObj);
            }
            jsonObj.Add("Content", content);
            BroadMessage(jsonObj.ToString());
        }
        private void PlayerLeaveGame(string playerId)
        {
            PlayerSeat seat = null;
            if (m_playerMapper.TryGetValue(playerId, out seat))
            {
                //1更新对象
                seat.HasPlayer = false;
                seat.PlayerId = "";
                seat.PlayerName = "";
                seat.PlayerHeadNo = 0;
                seat.Connected = false;
                seat.isReady = false;
                seat.isSpeaking = false;
                m_playerMapper.Remove(seat.PlayerId);
                //2通知游戏客户端
                JObject jsonObj = new JObject();
                jsonObj.Add("Action", "SeatChange");
                JArray content = new JArray();
                {
                    JObject changeObj = new JObject();
                    changeObj.Add("Change", "Leave");
                    changeObj.Add("SeatNo", seat.SeatNo);
                    content.Add(changeObj);
                }
                jsonObj.Add("Content", content);
                BroadMessage(jsonObj.ToString());
            }
        }
        private void PlayerReady(string playerId, bool ready)
        {
            PlayerSeat player = null;
            if (m_playerMapper.TryGetValue(playerId, out player))
            {
                //车准备好了
                player.isReady = ready;
                //响应客户端
                {
                    JObject jsonObj = new JObject();
                    jsonObj.Add("Action", "ReadyResponse");
                    jsonObj.Add("Ready", player.isReady);
                    SendMessage(playerId, jsonObj.ToString());
                }
                //广播变更
                {
                    JObject jsonObj = new JObject();
                    jsonObj.Add("Action", "SeatChange");
                    JArray content = new JArray();
                    {
                        JObject changeObj = new JObject();
                        changeObj.Add("Change", "Ready");
                        changeObj.Add("SeatNo", player.SeatNo);
                        changeObj.Add("IsReady", player.isReady);
                        content.Add(changeObj);
                    }
                    jsonObj.Add("Content", content);
                    BroadMessage(jsonObj.ToString());
                }
            }
        }
        private void PlayerSetIdentityExpection(string playerId, string idExpection)
        {
            PlayerSeat player = null;
            if (m_playerMapper.TryGetValue(playerId, out player))
            {
                player.IdentityExpection = idExpection;
            }
        }

        private void SendGameTip(string playerId, string tip)
        {
            JObject jsonObj = new JObject();
            jsonObj.Add("Action", "GameTip");
            jsonObj.Add("Tip", tip);
            SendMessage(playerId, jsonObj.ToString());
        }
        private void GameReset()
        {
            //1设置变量
            foreach (PlayerSeat seat in m_playerSeats)
            {
                seat.isReady = false;
                seat.Identity = null;
                seat.IdentityExpection = "";
                seat.isSpeaking = false;
                seat.isVoting = false;
            }
            m_isPlaying = false;
            m_publicProgress = "PlayerReady";
            //2发送信息
            JObject jsonObj = new JObject();
            jsonObj.Add("Action", "Reset");
            BroadMessage(jsonObj.ToString());
        }
        private void GameStartPrepare()
        {
            //1设置变量
            m_isPlaying = true;
            m_publicProgress = "StartPrepare";
            //2发送信息
            JObject jsonObj = new JObject();
            jsonObj.Add("Action", "StartPrepare");
            BroadMessage(jsonObj.ToString());
        }
        private void GameStart()
        {

        }
        private void Nightfall()
        {

        }
        private void Sunraise()
        {

        }
        private IEnumerator<int> JudgeMainLoop()
        {
            Stopwatch stopwatch = new Stopwatch();

            while (true)
            {
                //重置游戏
                GameReset();
                //等待所有玩家准备...
                while (!IsAllReady())
                {
                    yield return 0;
                }
                //所有人已就绪 等待3秒...
                GameStartPrepare();
                stopwatch.Restart();
                while (stopwatch.ElapsedMilliseconds < 3000)
                    yield return 0;
                //游戏正式开始 等待3秒...
                GameStart();
                while (stopwatch.ElapsedMilliseconds < 3000)
                    yield return 0;
                //分配身份
                while (true)
                    yield return 0;
            }
        }
        private bool IsAllReady()
        {
            int count = 0;
            int onlineCount = 0;
            foreach (PlayerSeat seat in m_playerSeats)
            {
                if (seat.isReady)
                    count++;
                if (seat.HasPlayer)
                    onlineCount++;
            }
            if (count == onlineCount)
                return true;
            return false;
        }
        #endregion

    }
}
