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
    public partial class Wolfman_P8 : GameModuel
    {
        #region 枚举类
        private enum PublicProcessState
        {
            Default, PlayerReady, StartPrepare, GameLoop
        }
        private enum GameloopProcessState
        {
            Default, CheckIdentity, NightCloseEye, DayOpenEye, SquareSpeak, SquareVote, End
        }
        private enum CurrentActionType
        {
            Default, Defender_Defend, Wolfman_Kill, Hunter_Revenge, Witch_Magic, Prophet_Foresee, Square_Speak, Square_Vote
        }
        private enum ActionDecisionType
        {
            Default,
            Defender_Defend_Excute, Defender_Defend_Abandon,
            Wolfman_Kill_Excute, Wolfman_Kill_Abandon,
            Prophet_Foresee_Excute, Prophet_Foresee_Abandon,
            Witch_Magic_Poison, Witch_Magic_Save, Witch_Magic_Abandon,
            Hunter_Revenge_Excute, Hunter_Revenge_Abandon,
            Square_Speak_Begin, Square_Speak_End,
            Square_Vote_Excute, Square_Vote_Abandon
        }
        private enum ActionIntentionType
        {
            Default, Defend, Foresee, Kill, Poison, Save, Vote, Revenge
        }
        private enum GameIdentityType
        {
            Default, Villager, Wolfman, Prophet, Hunter, Defender, Witch
        }
        private enum OperationFlagType
        {
            Default, Poison, Save, Defend, Kill, BanishTicket
        }
        private enum BaseFunctionType
        {
            Default, GameReady, SetIdentityExpection, TestVoice
        }
        private enum SeatChangeType
        {
            Default, Join, Leave, Disconnect, Reconnect, Ready, Speak, Dead
        }
        private enum IdentityTranslateType
        {
            Default, GoDead,
            Defender_Defend_Begin, Defender_Defend_End
        }
        private enum IdentityFunctionType
        {
            Default
        }
        private enum GameTipType
        {
            Default, CommonLog
        }
        private enum JudgeAnnounceType
        {
            Default
        }
        private enum DayTime { Default, Night, Day };
        #endregion

        #region 服务端变量
        private IDictionary<string, PlayerSeat> m_playerMapper;
        private IEnumerator<int> m_gameFlowLoop;
        private bool m_isPlaying = false;
        private PublicProcessState m_publicProcess = PublicProcessState.Default;
        private GameloopProcessState m_gameloopProcess = GameloopProcessState.Default;
        private DayTime m_dayTime = DayTime.Default;
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
            public GameIdentityType IdentityExpection = GameIdentityType.Default;
            public bool isSpeaking = false;
            public bool isReady = false;
            public long WaitTimestamp = 0;
            public GameIdentity Identity = null;
        }
        private PlayerSeat[] m_playerSeats;
        private class OperationFlag
        {
            public int SourceSeatNo = -1;
            public OperationFlagType FlagType = OperationFlagType.Default;
        }
        private class ActionIntention
        {
            public int TargetSeatNo = -1;
            public ActionIntentionType IntentionType = ActionIntentionType.Default;
        }
        private class GameIdentity
        {
            public GameIdentityType IdentityType = GameIdentityType.Default;
            public CurrentActionType CurrentAction = CurrentActionType.Default;
            public bool isDead = false;
            public int GameCamp = 0;
            public ActionIntention Intention = new ActionIntention();
            public IList<OperationFlag> FlagList = new List<OperationFlag>();

            virtual public void DoOperation()
            {

            }
        }
        private class Villager : GameIdentity
        {
            public Villager()
            {
                IdentityType = GameIdentityType.Villager;
            }
        }
        private class Wolfman : GameIdentity
        {
            public Wolfman()
            {
                IdentityType = GameIdentityType.Wolfman;
            }
        }
        private class Prophet : GameIdentity
        {
            public Prophet()
            {
                IdentityType = GameIdentityType.Prophet;
            }
        }
        private class Hunter : GameIdentity
        {
            public Hunter()
            {
                IdentityType = GameIdentityType.Hunter;
            }
        }
        private class Defender : GameIdentity
        {
            public int LastDefendNo = -1;
            public Defender()
            {
                IdentityType = GameIdentityType.Defender;
            }
        }
        private class Witch : GameIdentity
        {
            public bool Poison = true;
            public bool Antidote = true;
            public int ForeseeSeatNo = -1;
            public Witch()
            {
                IdentityType = GameIdentityType.Witch;
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
                jobj.Add("PublicProcess", (int)m_publicProcess);
                jobj.Add("GameloopProcess", (int)m_gameloopProcess);
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
                jobj.Add("WaitTimestamp", playerSeat.WaitTimestamp);
                JToken identityJObj = playerSeat.Identity != null ? GetIdentityJObject(playerSeat.Identity) : null;
                jobj.Add("Identity", identityJObj);
            }
            return jobj;
        }
        private JObject GetIdentityJObject(GameIdentity identity)
        {

            JObject identityJObj = new JObject();
            identityJObj.Add("IsDead", identity.isDead);
            identityJObj.Add("IdentityType", (int)identity.IdentityType);
            identityJObj.Add("GameCamp", identity.GameCamp);
            identityJObj.Add("CurrentAction", (int)identity.CurrentAction);
            switch (identity.IdentityType)
            {
                case GameIdentityType.Defender:
                    {
                        Defender defender = (Defender)identity;
                        identityJObj.Add("LastDefendNo", defender.LastDefendNo);
                    }
                    break;
                case GameIdentityType.Witch:
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

        #region 服务端接口与响应
        private void ReceiveJoinGame(string playerId, PlayerInfo info)
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
            JArray changeArray = new JArray();
            {
                JObject change = new JObject();
                change.Add("JPath", string.Format("PlayerSeatArray[{0}]", seat.SeatNo));
                change.Add("Value", GetPlayerSeatJArrayItem(seat));
                changeArray.Add(change);
            }
            BroadSeatChange(SeatChangeType.Join, changeArray);
        }
        private void ReceiveLeaveGame(string playerId)
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
                JArray changeArray = new JArray();
                {
                    JObject change = new JObject();
                    change.Add("JPath", string.Format("PlayerSeatArray[{0}]", seat.SeatNo));
                    change.Add("Value", GetPlayerSeatJArrayItem(seat));
                    changeArray.Add(change);
                }
                BroadSeatChange(SeatChangeType.Leave, changeArray);
            }
        }
        private void ReceiveSynchronizeState(string playerId)
        {
            ReturnSynchronizeState(playerId);
        }
        private void ReceiveBaseFunction(string playerId, BaseFunctionType functionType, JObject parms)
        {
            PlayerSeat seat = null;
            if (m_playerMapper.TryGetValue(playerId, out seat))
            {
                switch (functionType)
                {
                    case BaseFunctionType.GameReady:
                        {
                            bool isReady = (bool)parms.GetValue("IsReady");
                            seat.isReady = isReady;
                            //车准备好了
                            {
                                JObject resultDetail = new JObject();
                                JArray changeArray = new JArray();
                                {
                                    JObject change = new JObject();
                                    change.Add("JPath", "PlayerProperty.IsReady");
                                    change.Add("Value", isReady);
                                    changeArray.Add(change);
                                }
                                ReturnBaseFunctionResult(playerId, BaseFunctionType.GameReady, resultDetail, changeArray);
                            }
                            //列表也要更新
                            {
                                JArray changeArray = new JArray();
                                {
                                    JObject change = new JObject();
                                    change.Add("JPath", string.Format("PlayerSeatArray[{0}].IsReady", seat.SeatNo));
                                    change.Add("Value", isReady);
                                    changeArray.Add(change);
                                }
                                BroadSeatChange(SeatChangeType.Ready, changeArray);
                            }
;
                        }
                        break;
                    case BaseFunctionType.SetIdentityExpection:
                        {
                            GameIdentityType expectionType = (GameIdentityType)(int)parms.GetValue("IdentityExpection");
                            seat.IdentityExpection = expectionType;
                        }
                        break;
                }
            }
        }
        private void RecevieIdentityFunction(string playerId, IdentityFunctionType functionType, JObject parms)
        {
            PlayerSeat seat = null;
            if (m_playerMapper.TryGetValue(playerId, out seat))
            {
                switch (functionType)
                {
                    default:
                        break;
                }
            }
        }
        private void ReceiveIdentityActionDecide(string playerId, ActionDecisionType decisionType, int targetSeatNo)
        {
            PlayerSeat seat = null;
            if (m_playerMapper.TryGetValue(playerId, out seat))
            {
                switch (decisionType)
                {
                    case ActionDecisionType.Defender_Defend_Excute:
                        {
                            if (seat.Identity.IdentityType != GameIdentityType.Defender)
                                return;
                            if (seat.Identity.CurrentAction != CurrentActionType.Defender_Defend)
                                return;
                            Defender defender = (Defender)seat.Identity;
                            if (defender.LastDefendNo == targetSeatNo)
                            {
                                ReturnGameTip(playerId, GameTipType.CommonLog, "text.wolfman_p8.defender_continuous_defend");
                                return;
                            }
                            PlayerSeat targetSeat = m_playerSeats[targetSeatNo];
                            if (targetSeat.Identity.isDead)
                            {
                                ReturnGameTip(playerId, GameTipType.CommonLog, "text.wolfman_p8.dead_target");
                                return;
                            }
                            //守卫守人
                            defender.CurrentAction = CurrentActionType.Default;
                            defender.Intention.IntentionType = ActionIntentionType.Defend;
                            defender.Intention.TargetSeatNo = targetSeat.SeatNo;
                        }
                        break;
                    case ActionDecisionType.Defender_Defend_Abandon:
                        {
                            if (seat.Identity.IdentityType != GameIdentityType.Defender)
                                return;
                            if (seat.Identity.CurrentAction != CurrentActionType.Defender_Defend)
                                return;
                            //守卫不做动作
                            Defender defender = (Defender)seat.Identity;
                            defender.CurrentAction = CurrentActionType.Default;
                            defender.Intention.IntentionType = ActionIntentionType.Default;
                        }
                        break;
                }
            }
        }


        private void ReturnSynchronizeState(string playerId)
        {
            PlayerSeat seat = null;
            if (m_playerMapper.TryGetValue(playerId, out seat))
            {
                JObject jsonObj = new JObject();
                jsonObj.Add("Action", "SynchronizeState");
                JObject content = new JObject();
                {
                    JArray changeArray = new JArray();
                    {
                        JObject modelViewObj = GetModelView(seat);
                        JObject change1 = new JObject();
                        change1.Add("JPath", "PlayerSeatArray");
                        change1.Add("Value", modelViewObj.GetValue("PlayerSeatArray"));
                        changeArray.Add(change1);
                        JObject change2 = new JObject();
                        change2.Add("JPath", "GameProperty");
                        change2.Add("Value", modelViewObj.GetValue("GameProperty"));
                        changeArray.Add(change2);
                        JObject change3 = new JObject();
                        change3.Add("JPath", "PlayerProperty");
                        change3.Add("Value", modelViewObj.GetValue("PlayerProperty"));
                        changeArray.Add(change3);
                    }
                    content.Add("ModelViewChange", changeArray);
                }
                jsonObj.Add("Content", content);
                SendMessage(playerId, jsonObj.ToString());
            }
        }
        private void BroadSeatChange(SeatChangeType changeType, JArray modelViewChange)
        {
            JObject jsonObj = new JObject();
            jsonObj.Add("Action", "SeatChange");
            JObject content = new JObject();
            {
                content.Add("ChangeType", (int)changeType);
                content.Add("ModelViewChange", modelViewChange);
            }
            jsonObj.Add("Content", content);
            BroadMessage(jsonObj.ToString());
        }
        private void ReturnGameTip(string playerId, GameTipType tipType, string tipText)
        {
            JObject jsonObj = new JObject();
            jsonObj.Add("Action", "GameTip");
            JObject content = new JObject();
            {
                content.Add("TipType", (int)tipType);
                content.Add("TipText", tipText);
            }
            jsonObj.Add("Content", content);
            SendMessage(playerId, jsonObj.ToString());
        }
        private void ReturnJudgeAnnounce(string playerId, JudgeAnnounceType announceType, JObject parms)
        {
            JObject jsonObj = new JObject();
            jsonObj.Add("Action", "JudgeAnnounce");
            JObject content = new JObject();
            {
                content.Add("AnnounceType", (int)announceType);
                content.Add("Params", parms);
            }
            jsonObj.Add("Content", content);
            SendMessage(playerId, jsonObj.ToString());
        }
        private void ReturnBaseFunctionResult(string playerId, BaseFunctionType functionType, JObject resultDetail, JArray modelViewChange)
        {
            JObject jsonObj = new JObject();
            jsonObj.Add("Action", "BaseFunctionResult");
            JObject content = new JObject();
            {
                content.Add("FunctionType", (int)functionType);
                content.Add("ResultDetail", resultDetail);
                content.Add("ModelViewChange", modelViewChange);
            }
            jsonObj.Add("Content", content);
            SendMessage(playerId, jsonObj.ToString());
        }
        private void ReturnIdentityFunctionResult(string playerId, IdentityFunctionType functionType, JObject resultDetail, JArray modelViewChange)
        {
            JObject jsonObj = new JObject();
            jsonObj.Add("Action", "IdentityFunctionResult");
            JObject content = new JObject();
            {
                content.Add("FunctionType", (int)functionType);
                content.Add("ResultDetail", resultDetail);
                content.Add("ModelViewChange", modelViewChange);
            }
            jsonObj.Add("Content", content);
            SendMessage(playerId, jsonObj.ToString());
        }
        private void BroadPublicProcess(PublicProcessState processState, JArray modelViewChange)
        {
            JObject jsonObj = new JObject();
            jsonObj.Add("Action", "PublicProcess");
            JObject content = new JObject();
            {
                content.Add("ProcessState", (int)processState);
                content.Add("ModelViewChange", modelViewChange);
            }
            jsonObj.Add("Content", content);
            BroadMessage(jsonObj.ToString());
        }
        private void ReturnGameloopProcess(string playerId, GameloopProcessState processState, JArray modelViewChange)
        {
            JObject jsonObj = new JObject();
            jsonObj.Add("Action", "GameloopProcess");
            JObject content = new JObject();
            {
                content.Add("ProcessState", (int)processState);
                content.Add("ModelViewChange", modelViewChange);
            }
            jsonObj.Add("Content", content);
            SendMessage(playerId, jsonObj.ToString());
        }
        private void BroadGameloopProcess(GameloopProcessState processState, JArray modelViewChange)
        {
            JObject jsonObj = new JObject();
            jsonObj.Add("Action", "GameloopProcess");
            JObject content = new JObject();
            {
                content.Add("ProcessState", (int)processState);
                content.Add("ModelViewChange", modelViewChange);
            }
            jsonObj.Add("Content", content);
            BroadMessage(jsonObj.ToString());
        }
        private void ReturnIdentityTranslate(string playerId, IdentityTranslateType translateType, JArray modelViewChange)
        {
            JObject jsonObj = new JObject();
            jsonObj.Add("Action", "IdentityTranslate");
            JObject content = new JObject();
            {
                content.Add("TranslateType", (int)translateType);
                content.Add("ModelViewChange", modelViewChange);
            }
            jsonObj.Add("Content", content);
            SendMessage(playerId, jsonObj.ToString());
        }
        #endregion

        #region 具体逻辑
        private void GameReset()
        {
            //1设置变量
            foreach (PlayerSeat seat in m_playerSeats)
            {
                seat.isReady = false;
                seat.isSpeaking = false;
                seat.Identity = null;
                seat.IdentityExpection = GameIdentityType.Default;
            }
            m_isPlaying = false;
            m_publicProcess = PublicProcessState.PlayerReady;
            //2广播重置信息
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
            BroadPublicProcess(PublicProcessState.PlayerReady, changeArray);
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
        private void GameStartPrepare()
        {
            //1设置变量
            foreach (PlayerSeat seat in m_playerSeats)
            {
                seat.isReady = false;
                seat.isSpeaking = false;
            }
            m_isPlaying = true;
            m_publicProcess = PublicProcessState.StartPrepare;
            //2发送信息
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
            BroadPublicProcess(PublicProcessState.StartPrepare, changeArray);
        }
        private void GameStart()
        {
            //1设置变量
            m_publicProcess = PublicProcessState.GameLoop;
            //2发送消息
            JArray changeArray = new JArray();
            {
                JObject gameProperty = GetGamePropertyJObject();
                JObject change1 = new JObject();
                change1.Add("JPath", "GameProperty");
                change1.Add("Value", gameProperty);
                changeArray.Add(change1);
            }
            BroadPublicProcess(PublicProcessState.GameLoop, changeArray);
        }
        private void DistributeIdentity()
        {
            Random random = new Random();
            //1定义身份池
            List<GameIdentityType> identityPool = new List<GameIdentityType>{
                GameIdentityType.Villager, GameIdentityType.Villager,
                GameIdentityType.Wolfman, GameIdentityType.Wolfman,
                GameIdentityType.Prophet,
                GameIdentityType.Hunter,
                GameIdentityType.Defender,
                GameIdentityType.Witch
            };
            //2汇总各个身份的玩家期望
            IDictionary<GameIdentityType, IList<int>> expectionGroup = new Dictionary<GameIdentityType, IList<int>>();
            foreach (PlayerSeat seat in m_playerSeats)
            {
                GameIdentityType expection = seat.IdentityExpection;
                if (!expectionGroup.ContainsKey(expection))
                    expectionGroup[expection] = new List<int>();
                expectionGroup[expection].Add(seat.SeatNo);
            }
            //3分配身份
            IList<int> randomSeatNoList = new List<int>();
            IDictionary<int, GameIdentityType> seatNoIdentityMapper = new Dictionary<int, GameIdentityType>();
            //3.1优先分配
            foreach (KeyValuePair<GameIdentityType, IList<int>> kv in expectionGroup)
            {
                GameIdentityType identity = kv.Key;
                IList<int> seatNos = new List<int>(kv.Value);
                while (seatNos.Count > 0)
                {

                    int seatNoIdx = (int)(random.NextDouble() * seatNos.Count);
                    int seatNo = seatNos[seatNoIdx];
                    seatNos.RemoveAt(seatNoIdx);
                    int identityIdx = identityPool.FindIndex(a => a == identity);
                    if (identityIdx >= 0)
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
            foreach (KeyValuePair<int, GameIdentityType> kv in seatNoIdentityMapper)
            {
                int seatNo = kv.Key;
                GameIdentityType identity = kv.Value;
                GameIdentity identityObj = null;
                switch (identity)
                {
                    case GameIdentityType.Villager:
                        identityObj = new Villager();
                        break;
                    case GameIdentityType.Wolfman:
                        identityObj = new Wolfman();
                        break;
                    case GameIdentityType.Prophet:
                        identityObj = new Prophet();
                        break;
                    case GameIdentityType.Hunter:
                        identityObj = new Hunter();
                        break;
                    case GameIdentityType.Defender:
                        identityObj = new Defender();
                        break;
                    case GameIdentityType.Witch:
                        identityObj = new Witch();
                        break;
                }
                m_playerSeats[seatNo].Identity = identityObj;
            }
            //5更新状态变量
            m_gameloopProcess = GameloopProcessState.CheckIdentity;
            foreach (PlayerSeat seat in m_playerSeats)
            {
                long waitTimeStamp = DateTime.UtcNow.AddSeconds(10).Ticks;
                seat.WaitTimestamp = waitTimeStamp;
            }
            //6客户端同步更新消息
            foreach (PlayerSeat seat in m_playerSeats)
            {
                JArray changeArray = new JArray();
                {
                    JObject change1 = new JObject();
                    change1.Add("JPath", "GameProperty");
                    change1.Add("Value", GetGamePropertyJObject());
                    changeArray.Add(change1);
                    JObject change2 = new JObject();
                    change2.Add("JPath", "PlayerProperty");
                    change2.Add("Value", GetPlayerPropertyJObject(seat));
                    changeArray.Add(change2);
                }
                ReturnGameloopProcess(seat.PlayerId, GameloopProcessState.CheckIdentity, changeArray);
            }
        }
        private void NightCloseEye()
        {
            //1设置变量
            m_gameloopProcess = GameloopProcessState.NightCloseEye;
            //2发送消息
            JArray changeArray = new JArray();
            {
                JObject gameProperty = GetGamePropertyJObject();
                JObject change1 = new JObject();
                change1.Add("JPath", "GameProperty");
                change1.Add("Value", gameProperty);
                changeArray.Add(change1);
            }
            BroadGameloopProcess(GameloopProcessState.NightCloseEye, changeArray);
        }
        private void DefenderDefend_Choose()
        {
            //1列举出守卫玩家
            IList<PlayerSeat> defenderSeats = new List<PlayerSeat>(m_playerSeats.Where((seat) =>
            {
                //排除空对象
                if (seat.Identity == null)
                    return false;
                //排除掉死亡的玩家
                if (seat.Identity.isDead)
                    return false;
                //排除掉不是守卫的玩家
                if (seat.Identity.IdentityType != GameIdentityType.Defender)
                    return false;

                return true;
            }));
            //2切换当前动作-》守卫抉择
            foreach (PlayerSeat defenderSeat in defenderSeats)
            {
                long waitTimeStamp = DateTime.UtcNow.AddSeconds(10).Ticks;
                defenderSeat.WaitTimestamp = waitTimeStamp;
                Defender defender = (Defender)defenderSeat.Identity;
                defender.CurrentAction = CurrentActionType.Defender_Defend;
            }
            //3客户端同步更新消息
            foreach (PlayerSeat defenderSeat in defenderSeats)
            {
                JArray changeArray = new JArray();
                {
                    JObject change1 = new JObject();
                    change1.Add("JPath", "PlayerProperty.WaitTimestamp");
                    change1.Add("Value", defenderSeat.WaitTimestamp);
                    changeArray.Add(change1);
                    JObject change2 = new JObject();
                    change2.Add("JPath", "PlayerProperty.Identity.CurrentAction");
                    change2.Add("Value", (int)defenderSeat.Identity.CurrentAction);
                    changeArray.Add(change2);
                }
                ReturnIdentityTranslate(defenderSeat.PlayerId, IdentityTranslateType.Defender_Defend_Begin, changeArray);
            }
        }
        private bool DefenderDefend_Wait()
        {
            //等待所有守卫做出选择 或者时间结束
            int count = 0;
            int okCount = 0;
            foreach (PlayerSeat seat in m_playerSeats)
            {
                if (seat.Identity != null)
                {
                    if (seat.Identity.IdentityType == GameIdentityType.Defender)
                    {
                        Defender defender = (Defender)seat.Identity;
                        count++;
                        if (defender.CurrentAction == CurrentActionType.Default)
                            okCount++;
                    }
                }
            }

            if (count == okCount)
                return true;
            return false;
        }
        private void DefenderDefend_Result()
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
                    DefenderDefend_Choose();
                    stopwatch.Restart();
                    while (stopwatch.ElapsedMilliseconds < 10000 && !DefenderDefend_Wait())
                        yield return 0;
                    DefenderDefend_Result();
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
        #endregion

    }
}
