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

        #region 服务端变量
        private IDictionary<string, PlayerSeat> m_playerMapper;
        private IEnumerator<int> m_gameFlowLoop;
        private bool m_isPlaying = false;
        private string m_publicProcess = "Default";
        private string m_gameloopProcess = "Default";
        private enum DayTime { None = 0, Night = 1, Day = 2 };
        private DayTime m_dayTime = DayTime.None;
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
            public string IdentityExpection = "Random";
            public bool isSpeaking = false;
            public bool isReady = false;
            public GameIdentity Identity = null;
        }
        private PlayerSeat[] m_playerSeats;
        private class OperationFlag
        {
            public int SourceSeatNo = 0;
            public string FlagType = "";
        }
        private class GameIdentity
        {
            public string IdentityType = "Default";
            public string CurrentAction = "Default";
            public bool isDead = false;
            public int GameCamp = 0;
            public IList<OperationFlag> OperationFlagList = new List<OperationFlag>();
        }
        private class Villager : GameIdentity
        {
            public Villager()
            {
                IdentityType = "Villager";
            }
        }
        private class Wolfman : GameIdentity
        {
            public Wolfman()
            {
                IdentityType = "Wolfman";
            }
        }
        private class Prophet : GameIdentity
        {
            public Prophet()
            {
                IdentityType = "Prophet";
            }
        }
        private class Hunter : GameIdentity
        {
            public Hunter()
            {
                IdentityType = "Hunter";
            }
        }
        private class Defender : GameIdentity
        {
            public int LastDefendNo = -1;
            public Defender()
            {
                IdentityType = "Defender";
            }
        }
        private class Witch : GameIdentity
        {
            public bool Poison = true;
            public bool Antidote = true;
            public Witch()
            {
                IdentityType = "Witch";
            }
        }
        #endregion

        #region JSON视图模型
        private JObject GetModelView(PlayerSeat playerSeat)
        {
            JObject modelView = new JObject();
            //1玩家列表名片状态
            JArray playerSeatArray = GetPlayerSeatJArray();
            modelView.Add("PlayerSeatArray", playerSeatArray);
            //2游戏全局属性
            JObject gameProperty = GetGamePropertyJObject();
            modelView.Add("GameProperty", gameProperty);
            //3该玩家当前属性
            JObject playerProperty = GetPlayerPropertyJObject(playerSeat);
            modelView.Add("PlayerProperty", playerProperty);
            return modelView;
        }
        private JArray GetPlayerSeatJArray()
        {
            JArray jArray = new JArray();
            foreach (PlayerSeat seat in m_playerSeats)
            {
                JObject jobj = GetPlayerSeatJArrayItem(seat);
                jArray.Add(jobj);
            }
            return jArray;
        }
        private JObject GetPlayerSeatJArrayItem(PlayerSeat playerSeat)
        {
            JObject jobj = new JObject();
            jobj.Add("SeatNo", playerSeat.SeatNo);
            jobj.Add("HasPlayer", playerSeat.HasPlayer);
            jobj.Add("Name", playerSeat.PlayerName);
            jobj.Add("HeadNo", playerSeat.PlayerHeadNo);
            jobj.Add("Connected", playerSeat.Connected);
            jobj.Add("IsSpeaking", playerSeat.isSpeaking);
            jobj.Add("IsReady", playerSeat.isReady);
            bool isDead = playerSeat.Identity != null ? playerSeat.Identity.isDead : false;
            jobj.Add("IsDead", isDead);

            return jobj;
        }
        private JObject GetGamePropertyJObject()
        {
            JObject jobj = new JObject();
            {
                jobj.Add("IsPlaying", m_isPlaying);
                jobj.Add("PublicProcess", m_publicProcess);
                jobj.Add("GameloopProcess", m_gameloopProcess);
                jobj.Add("DayTime", (int)m_dayTime);
                jobj.Add("DayNumber", m_dayNumber);
            }
            return jobj;
        }
        private JObject GetPlayerPropertyJObject(PlayerSeat playerSeat)
        {
            JObject jobj = new JObject();
            {
                jobj.Add("PlayerId", playerSeat.PlayerId);
                jobj.Add("PlayerName", playerSeat.PlayerName);
                jobj.Add("PlayerHeadNo", playerSeat.PlayerHeadNo);
                jobj.Add("SeatNo", playerSeat.SeatNo);
                jobj.Add("IsSpeaking", playerSeat.isSpeaking);
                jobj.Add("IsReady", playerSeat.isReady);
                JToken identityJObj = playerSeat.Identity != null ? GetIdentityJObject(playerSeat.Identity) : null;
                jobj.Add("Identity", identityJObj);
            }
            return jobj;
        }
        private JObject GetIdentityJObject(GameIdentity identity)
        {

            JObject identityJObj = new JObject();
            identityJObj.Add("IsDead", identity.isDead);
            identityJObj.Add("IdentityType", identity.IdentityType);
            identityJObj.Add("GameCamp", identity.GameCamp);
            identityJObj.Add("CurrentAction", identity.CurrentAction);
            switch (identity.IdentityType)
            {
                case "Defender":
                    {
                        Defender defender = (Defender)identity;
                        identityJObj.Add("LastDefendNo", defender.LastDefendNo);
                    }
                    break;
                case "Witch":
                    {
                        Witch witch = (Witch)identity;
                        identityJObj.Add("Antidote", witch.Antidote);
                        identityJObj.Add("Poison", witch.Poison);
                    }
                    break;
            }
            return identityJObj;
        }
        #endregion

        #region 具体逻辑
        private void PlayerSynchronizeGame(string playerId)
        {
            PlayerSeat seat = null;
            if (m_playerMapper.TryGetValue(playerId, out seat))
            {
                JObject jsonObj = new JObject();
                jsonObj.Add("Action", "ResponseGameStatus");
                JObject modelViewObj = this.GetModelView(seat);
                jsonObj.Add("ModelView", modelViewObj);
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
            JArray changeArray = new JArray();
            {
                JObject change = new JObject(); 
                change.Add("JPath", string.Format("PlayerSeatArray[{0}]", seat.SeatNo));
                change.Add("Value", GetPlayerSeatJArrayItem(seat));
                changeArray.Add(change);
            }
            jsonObj.Add("ModelViewChange", changeArray);
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
                //2通知客户端
                JObject jsonObj = new JObject();
                jsonObj.Add("Action", "SeatChange");
                JArray changeArray = new JArray();
                {
                    JObject change = new JObject();
                    change.Add("JPath", string.Format("PlayerSeatArray[{0}]", seat.SeatNo));
                    change.Add("Value", GetPlayerSeatJArrayItem(seat));
                    changeArray.Add(change);
                }
                jsonObj.Add("ModelViewChange", changeArray);
                BroadMessage(jsonObj.ToString());
            }
        }
        private void PlayerReady(string playerId, bool ready)
        {
            PlayerSeat seat = null;
            if (m_playerMapper.TryGetValue(playerId, out seat))
            {
                //车准备好了
                seat.isReady = ready;
                //响应客户端
                {
                    JObject jsonObj = new JObject();
                    jsonObj.Add("Action", "ReadyResponse");
                    JArray changeArray = new JArray();
                    {
                        JObject change = new JObject();
                        change.Add("JPath", "PlayerProperty.IsReady");
                        change.Add("Value", ready);
                        changeArray.Add(change);
                    }
                    jsonObj.Add("ModelViewChange", changeArray);
                    SendMessage(playerId, jsonObj.ToString());
                }
                //广播变更
                {
                    JObject jsonObj = new JObject();
                    jsonObj.Add("Action", "SeatChange");
                    JArray changeArray = new JArray();
                    {
                        JObject change = new JObject();
                        change.Add("JPath", string.Format("PlayerSeatArray[{0}].IsReady", seat.SeatNo));
                        change.Add("Value", ready);
                        changeArray.Add(change);
                    }
                    jsonObj.Add("ModelViewChange", changeArray);
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

        private void GameReset()
        {
            //1设置变量
            foreach (PlayerSeat seat in m_playerSeats)
            {
                seat.isReady = false;
                seat.isSpeaking = false;
                seat.Identity = null;
                seat.IdentityExpection = "";
            }
            m_isPlaying = false;
            m_publicProcess = "PlayerReady";
            //2广播重置信息
            JObject jsonObj = new JObject();
            jsonObj.Add("Action", "Reset");
            JArray changeArray = new JArray();
            {
                JArray playerSeatArray = GetPlayerSeatJArray();
                JObject change1 = new JObject();
                change1.Add("JPath", "PlayerSeatArray");
                change1.Add("Value", playerSeatArray);
                changeArray.Add(change1);
                JObject gameProperty = GetGamePropertyJObject();
                JObject change2 = new JObject();
                change2.Add("JPath", "GameProperty");
                change2.Add("Value", gameProperty);
                changeArray.Add(change2);
                JObject change3 = new JObject();
                change3.Add("JPath", "PlayerProperty.Identity");
                change3.Add("Value", null);
                changeArray.Add(change3);
            }
            jsonObj.Add("ModelViewChange", changeArray);
            BroadMessage(jsonObj.ToString());
        }
        private void GameStartPrepare()
        {
            //1设置变量
            foreach (PlayerSeat seat in m_playerSeats)
            {
                seat.isReady = false;
                seat.isSpeaking = false;
            }
            m_isPlaying = true;
            m_publicProcess = "StartPrepare";
            //2发送信息
            JObject jsonObj = new JObject();
            jsonObj.Add("Action", "StartPrepare");
            JArray changeArray = new JArray();
            {
                JArray playerSeatArray = GetPlayerSeatJArray();
                JObject change1 = new JObject();
                change1.Add("JPath", "PlayerSeatArray");
                change1.Add("Value", playerSeatArray);
                changeArray.Add(change1);
                JObject gameProperty = GetGamePropertyJObject();
                JObject change2 = new JObject();
                change2.Add("JPath", "GameProperty");
                change2.Add("Value", gameProperty);
                changeArray.Add(change2);
            }
            jsonObj.Add("ModelViewChange", changeArray);
            BroadMessage(jsonObj.ToString());
        }
        private void GameStart()
        {
            //1设置变量
            m_publicProcess = "GameLoop";
            //2发送消息
            JObject jsonObj = new JObject();
            jsonObj.Add("Action", "GameStart");
            JArray changeArray = new JArray();
            {
                JObject gameProperty = GetGamePropertyJObject();
                JObject change1 = new JObject();
                change1.Add("JPath", "GameProperty");
                change1.Add("Value", gameProperty);
                changeArray.Add(change1);
            }
            jsonObj.Add("ModelViewChange", changeArray);
            BroadMessage(jsonObj.ToString());
        }
        private void DistributeIdentity()
        {
            Random random = new Random();
            //1定义身份池
            List<string> identityPool = new List<string>
            { "Villager", "Villager", "Wolfman", "Wolfman", "Prophet", "Hunter", "Defender", "Witch" };
            //2汇总各个身份的玩家期望
            IDictionary<string, IList<int>> expectionGroup = new Dictionary<string, IList<int>>();
            foreach(PlayerSeat seat in m_playerSeats)
            {
                string expection = seat.IdentityExpection;
                if (!expectionGroup.ContainsKey(expection))
                    expectionGroup[expection] = new List<int>();
                expectionGroup[expection].Add(seat.SeatNo);
            }
            //3分配身份
            IList<int> randomSeatNoList = new List<int>();
            IDictionary<int, string> seatNoIdentityMapper = new Dictionary<int, string>();
            //3.1优先分配
            foreach(KeyValuePair<string, IList<int>> kv in expectionGroup)
            {
                string identity = kv.Key;
                IList<int> seatNos = new List<int>(kv.Value);
                while(seatNos.Count > 0)
                {

                    int seatNoIdx = (int)(random.NextDouble() * seatNos.Count);
                    int seatNo = seatNos[seatNoIdx];
                    seatNos.RemoveAt(seatNoIdx);
                    int identityIdx = identityPool.FindIndex(a => a == identity);
                    if(identityIdx >= 0)
                    {
                        seatNoIdentityMapper[seatNo] = identity;
                        identityPool.RemoveAt(identityIdx);
                    }
                    else
                    {
                        randomSeatNoList.Add(seatNo);
                    }
                }
            }
            //3.2随机分配
            foreach (int seatNo in randomSeatNoList)
            {
                int identityIdx = (int)(random.NextDouble() * identityPool.Count);
                seatNoIdentityMapper[seatNo] = identityPool[identityIdx];
                identityPool.RemoveAt(identityIdx);
            }
            //4按照最终的分配情况玩家作为变量
            foreach(KeyValuePair<int, string> kv in seatNoIdentityMapper)
            {
                int seatNo = kv.Key;
                string identity = kv.Value;
                GameIdentity identityObj = null;
                switch (identity)
                {
                    case "Villager":
                        identityObj = new Villager();
                        break;
                    case "Wolfman":
                        identityObj = new Wolfman();
                        break;
                    case "Prophet":
                        identityObj = new Prophet();
                        break;
                    case "Hunter":
                        identityObj = new Hunter();
                        break;
                    case "Defender":
                        identityObj = new Defender();
                        break;
                    case "Witch":
                        identityObj = new Witch();
                        break;
                }
                m_playerSeats[seatNo].Identity = identityObj;
            }
            //5更新状态变量 向玩家同步更新信息
            m_gameloopProcess = "CheckIdentity";
            foreach (PlayerSeat seat in m_playerSeats)
            {
                JObject jsonObj = new JObject();
                jsonObj.Add("Action", "GameLoopProcess");
                JObject content = new JObject();
                {   
                    JArray changeArray = new JArray();
                    {
                        JObject change1 = new JObject();
                        change1.Add("JPath", "GameProperty");
                        change1.Add("Value", GetGamePropertyJObject());
                        changeArray.Add(change1);
                        JObject change2 = new JObject();
                        change2.Add("JPath", "PlayerProperty.Identity");
                        change2.Add("Value", GetIdentityJObject(seat.Identity));
                        changeArray.Add(change2);
                    }
                    content.Add("ModelViewChange", changeArray);
                    content.Add("Process", "DistributeIdentity");
                }
                jsonObj.Add("Content", content);
                SendMessage(seat.PlayerId, jsonObj.ToString());
            }
        }
        private void NightCloseEye()
        {
            //1设置变量
            m_gameloopProcess = "NightCloseEye";
            //2发送消息
            JObject jsonObj = new JObject();
            jsonObj.Add("Action", "GameLoopProcess");
            JObject content = new JObject();
            {
                JArray changeArray = new JArray();
                {
                    JObject gameProperty = GetGamePropertyJObject();
                    JObject change1 = new JObject();
                    change1.Add("JPath", "GameProperty");
                    change1.Add("Value", gameProperty);
                    changeArray.Add(change1);
                }
                content.Add("ModelViewChange", changeArray);
                content.Add("Process", "NightCloseEye");
            }
            jsonObj.Add("Content", content);
            BroadMessage(jsonObj.ToString());
        }
        private void DayOpenEye()
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
                //游戏正式开始
                GameStart();
                //分配身份 10秒钟确认时间
                DistributeIdentity();
                stopwatch.Restart();
                while (stopwatch.ElapsedMilliseconds < 10000)
                    yield return 0;
                //开始初夜循环
                m_dayNumber = 1;
                while (true)
                {
                    //天黑请闭眼
                    NightCloseEye();
                    stopwatch.Restart();
                    while (stopwatch.ElapsedMilliseconds < 3000)
                        yield return 0;
                    //守卫行动

                    //预言家行动

                    //狼人行动

                    //女巫行动

                    //白天请睁眼

                    //猎人行动

                    //聚众发言

                    //聚众投票
                    while (true)
                        yield return 0;
                    yield return 0;
                }
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
