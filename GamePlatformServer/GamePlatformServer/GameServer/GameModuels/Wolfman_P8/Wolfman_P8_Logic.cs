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
            Default, Defender_Defend, Wolfman_Kill, Hunter_Revenge, Witch_Magic, Prophet_Foresee, Square_Speak, Square_Vote, LastWord
        }
        private enum ActionDecisionType
        {
            Default,
            Defender_Defend_Excute, Defender_Defend_Abandon,
            Wolfman_Kill_Excute, Wolfman_Kill_Abandon,
            Prophet_Foresee_Excute, Prophet_Foresee_Abandon,
            Witch_Magic_Poison, Witch_Magic_Save, Witch_Magic_Abandon,
            Hunter_Revenge_Excute, Hunter_Revenge_Abandon,
            Square_Speak_Begin, Square_Speak_End, Square_Speak_Abandon,
            Square_Vote_Excute, Square_Vote_Abandon,
            LastWord_Begin, LastWord_End, LastWord_Abandon,
        }
        private enum ActionIntentionType
        {
            Default, Abandon, Defend, Foresee, Kill, Poison, Save, Vote, Revenge
        }
        private enum GameIdentityType
        {
            Default, Villager, Wolfman, Prophet, Hunter, Defender, Witch
        }
        private enum OperationFlagType
        {
            Default, Poison, Save, Defend, Kill, BanishTicket
        }
        private enum OperationResultType
        {
            Default, Saved, Defended, Poisoned, Killed, Banned, Revenged
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
            Defender_Defend_Begin, Defender_Defend_End,
            Prophet_Foresee_Begin, Prophet_Foresee_End,
            Wolfman_Kill_Begin, Wolfman_Kill_End,
            Witch_Magic_Begin, Witch_Magic_End,
        }
        private enum IdentityFunctionType
        {
            Default,
            Prophet_ForeseeLog,
            Wolfman_BodyLanguage,
            Wolfman_Whisper
        }
        private enum GameTipType
        {
            Default, CommonLog, CommonModel, PlayerChangeLog
        }
        private enum JudgeAnnounceType
        {
            Default,
            DistributeIdentity_Result,
            Defender_Defend_Excute, Defender_Defend_Abandon,
            Wolfman_Kill_Excute, Wolfman_Kill_Abandon,
            Prophet_Foresee_Excute, Prophet_Foresee_Abandon,
            Witch_Magic_Poison, Witch_Magic_Save, Witch_Magic_Abandon,
            Hunter_Revenge_Excute, Hunter_Revenge_Abandon,
            Foresee_Result,
            Revenge_Report,
            Speak_Begin, Speak_Abandon, Speak_End,
            LastWord_Begin, LastWord_Abandon, LastWord_End,
            Square_Vote_Report, LastNight_Report, GameOverLog_Report
        }
        private enum BodyLanguageType
        {
            Default,
            Nod, LOL
        }
        private enum GameOverLogType
        {
            Default,
            Start, DistributeIdentity, NightCloseEye, DayOpenEye, End,
            Defender_Defend,
            Wolfman_Kill,
            Prophet_Foresee,
            Witch_Magic,
            Hunter_Revenge,
            Square_Vote,
            LastNight
        }
        private enum DayTime { Default, Night, Day };
        private enum IdentityCamp { Default, Villager, Wolfman }
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
            public GameIdentity Identity = new GameIdentity();
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
            public IdentityCamp GameCamp =  IdentityCamp.Default;
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
                GameCamp = IdentityCamp.Villager;
            }
        }
        private class Wolfman : GameIdentity
        {
            public int WhisperTime = 0;

            public Wolfman()
            {
                IdentityType = GameIdentityType.Wolfman;
                GameCamp = IdentityCamp.Wolfman;
            }
        }
        private class Prophet : GameIdentity
        {
            public IDictionary<int, GameIdentityType> ForeseeLog = new Dictionary<int, GameIdentityType>();
            public Prophet()
            {
                IdentityType = GameIdentityType.Prophet;
                GameCamp = IdentityCamp.Villager;
            }
        }
        private class Hunter : GameIdentity
        {
            public Hunter()
            {
                IdentityType = GameIdentityType.Hunter;
                GameCamp = IdentityCamp.Villager;
            }
        }
        private class Defender : GameIdentity
        {
            public int LastDefendNo = -1;
            public Defender()
            {
                IdentityType = GameIdentityType.Defender;
                GameCamp = IdentityCamp.Villager;
            }
        }
        private class Witch : GameIdentity
        {
            public bool Poison = true;
            public bool Antidote = true;
            public int PremonitionNo = -1;
            public Witch()
            {
                IdentityType = GameIdentityType.Witch;
                GameCamp = IdentityCamp.Villager;
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
            jobj.Add("IsDead", playerSeat.Identity.isDead);

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
                jobj.Add("Identity", GetIdentityJObject(playerSeat.Identity));
            }
            return jobj;
        }
        private JObject GetIdentityJObject(GameIdentity identity)
        {

            JObject identityJObj = new JObject();
            identityJObj.Add("IsDead", identity.isDead);
            identityJObj.Add("IdentityType", (int)identity.IdentityType);
            identityJObj.Add("GameCamp", (int)identity.GameCamp);
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
                        identityJObj.Add("PremonitionNo", witch.PremonitionNo);
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
                //2如果是中途退出 公布消息
                if (m_isPlaying)
                {
                    JObject parms = new JObject();
                    {
                        parms.Add("Template", "template.wolfman_p8.playerleave_logtip");
                        parms.Add("SeatNo", seat.SeatNo);
                    }
                    BroadGameTip(GameTipType.PlayerChangeLog, parms);
                }
                //3客户端座位列表更新
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
        private void ReceiveDisconnect(string playerId)
        {
            PlayerSeat seat = null;
            if (m_playerMapper.TryGetValue(playerId, out seat))
            {
                //1设置座位状态
                seat.Connected = false;
                //2公布消息
                JObject parms = new JObject();
                {
                    parms.Add("Template", "template.wolfman_p8.playerdisconnect_logtip");
                    parms.Add("SeatNo", seat.SeatNo);
                }
                BroadGameTip(GameTipType.PlayerChangeLog, parms);
                //3客户端座位列表更新
                JArray changeArray = new JArray();
                {
                    JObject change = new JObject();
                    change.Add("JPath", string.Format("PlayerSeatArray[{0}]", seat.SeatNo));
                    change.Add("Value", GetPlayerSeatJArrayItem(seat));
                    changeArray.Add(change);
                }
                BroadSeatChange(SeatChangeType.Disconnect, changeArray);
            }
        }
        private void ReceiveReconnect(string playerId)
        {
            PlayerSeat seat = null;
            if (m_playerMapper.TryGetValue(playerId, out seat))
            {
                //1设置座位状态
                seat.Connected = true;
                //2公布消息
                JObject parms = new JObject();
                {
                    parms.Add("Template", "template.wolfman_p8.playerreconnect_logtip");
                    parms.Add("SeatNo", seat.SeatNo);
                }
                BroadGameTip(GameTipType.PlayerChangeLog, parms);
                //3客户端座位列表更新
                JArray changeArray = new JArray();
                {
                    JObject change = new JObject();
                    change.Add("JPath", string.Format("PlayerSeatArray[{0}]", seat.SeatNo));
                    change.Add("Value", GetPlayerSeatJArrayItem(seat));
                    changeArray.Add(change);
                }
                BroadSeatChange(SeatChangeType.Reconnect, changeArray);
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
                            //读JSON
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
                            //读JSON
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
                    case IdentityFunctionType.Prophet_ForeseeLog:
                        {
                            if(seat.Identity.IdentityType == GameIdentityType.Prophet)
                            {
                                Prophet prophet = (Prophet)seat.Identity;
                                JObject details = new JObject();
                                {
                                    JArray logs = new JArray();
                                    foreach (KeyValuePair<int, GameIdentityType> kv in prophet.ForeseeLog)
                                    {
                                        JObject logItem = new JObject();
                                        logItem.Add("SeatNo", kv.Key);
                                        logItem.Add("IdentityType", (int)kv.Value);
                                        logs.Add(logItem);
                                    }
                                    details.Add("ForeseeLogs", logs);
                                }
                                ReturnIdentityFunctionResult(
                                    seat.PlayerId, IdentityFunctionType.Prophet_ForeseeLog, details, new JArray());
                            }
                        }
                        break;
                    case IdentityFunctionType.Wolfman_Whisper:
                        {
                            if(seat.Identity.IdentityType == GameIdentityType.Wolfman)
                            {
                                //读JSON
                                string whisper = (string)parms.GetValue("Whisper");
                                //0筛选条件
                                Wolfman wolfman = (Wolfman)seat.Identity;
                                if (wolfman.CurrentAction != CurrentActionType.Wolfman_Kill)
                                    return;
                                //1密语次数减1
                                if (wolfman.WhisperTime <= 0) 
                                    return;
                                wolfman.WhisperTime--;
                                //2列举出狼人玩家
                                IList<PlayerSeat> wolfmanSeats = new List<PlayerSeat>(m_playerSeats.Where((s) =>
                                {
                                    //排除掉死亡的玩家
                                    if (s.Identity.isDead)
                                        return false;
                                    //排除掉不是狼人的玩家
                                    if (s.Identity.IdentityType != GameIdentityType.Wolfman)
                                        return false;
                                    return true;
                                }));
                                //3返回悄悄话
                                if (whisper.Length > 2)
                                    whisper = whisper.Substring(0, 2);
                                foreach (PlayerSeat wolfmanSeat in wolfmanSeats)
                                {
                                    JObject details = new JObject();
                                    {
                                        details.Add("SeatNo", seat.SeatNo);
                                        details.Add("Whisper", whisper);
                                    }
                                    ReturnIdentityFunctionResult(
                                        wolfmanSeat.PlayerId, IdentityFunctionType.Wolfman_Whisper, details, new JArray());
                                }
                            }         
                        }
                        break;
                    case IdentityFunctionType.Wolfman_BodyLanguage:
                        {
                            if (seat.Identity.IdentityType == GameIdentityType.Wolfman)
                            {
                                //读JSON
                                int bodyLanguageType = (int)parms.GetValue("BodyLanguageType");
                                //0筛选条件
                                Wolfman wolfman = (Wolfman)seat.Identity;
                                if (wolfman.CurrentAction != CurrentActionType.Wolfman_Kill)
                                    return;
                                //1列举出狼人玩家
                                IList<PlayerSeat> wolfmanSeats = new List<PlayerSeat>(m_playerSeats.Where((s) =>
                                {
                                    //排除掉死亡的玩家
                                    if (s.Identity.isDead)
                                        return false;
                                    //排除掉不是狼人的玩家
                                    if (s.Identity.IdentityType != GameIdentityType.Wolfman)
                                        return false;
                                    return true;
                                }));
                                //2返回悄悄话
                                foreach (PlayerSeat wolfmanSeat in wolfmanSeats)
                                {
                                    JObject details = new JObject();
                                    {
                                        details.Add("SeatNo", seat.SeatNo);
                                        details.Add("BodyLanguageType", bodyLanguageType);
                                    }
                                    ReturnIdentityFunctionResult(
                                        wolfmanSeat.PlayerId, IdentityFunctionType.Wolfman_BodyLanguage, details, new JArray());
                                }
                            }
                        }
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
                            if (seat.Identity.Intention.IntentionType != ActionIntentionType.Default)
                                return;
                            Defender defender = (Defender)seat.Identity;
                            //条件:不能连续守同一个人
                            if (defender.LastDefendNo == targetSeatNo)
                            {
                                JObject parms = new JObject();
                                parms.Add("Text", "text.wolfman_p8.actiondecide_defend_continuous_modeltip");
                                ReturnGameTip(playerId, GameTipType.CommonModel, parms);
                                return;
                            }
                            //条件:不能守死人
                            PlayerSeat targetSeat = m_playerSeats[targetSeatNo];
                            if (targetSeat.Identity.isDead)
                            {
                                JObject parms = new JObject();
                                parms.Add("Text", "text.wolfman_p8.actiondecide_deadtarget_modeltip");
                                ReturnGameTip(playerId, GameTipType.CommonModel, parms);
                                return;
                            }
                            //抉择结束 确定守卫意图
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
                            if (seat.Identity.Intention.IntentionType != ActionIntentionType.Default)
                                return;
                            //守卫不做动作
                            Defender defender = (Defender)seat.Identity;
                            defender.Intention.IntentionType = ActionIntentionType.Abandon;
                        }
                        break;
                    case ActionDecisionType.Prophet_Foresee_Excute:
                        {
                            if (seat.Identity.IdentityType != GameIdentityType.Prophet)
                                return;
                            if (seat.Identity.CurrentAction != CurrentActionType.Prophet_Foresee)
                                return;
                            if (seat.Identity.Intention.IntentionType != ActionIntentionType.Default)
                                return;
                            Prophet prophet  = (Prophet)seat.Identity;
                            PlayerSeat targetSeat = m_playerSeats[targetSeatNo];
                            //条件:不能验死人
                            if (targetSeat.Identity.isDead)
                            {
                                JObject parms = new JObject();
                                parms.Add("Text", "text.wolfman_p8.actiondecide_deadtarget_modeltip");
                                ReturnGameTip(playerId, GameTipType.CommonModel, parms);
                                return;
                            }
                            //条件:不能验自己
                            if(targetSeat.SeatNo == seat.SeatNo)
                            {
                                JObject parms = new JObject();
                                parms.Add("Text", "text.wolfman_p8.actiondecide_foresee_self_modeltip");
                                ReturnGameTip(playerId, GameTipType.CommonModel, parms);
                                return;
                            }
                            //条件:不能重复验
                            if (prophet.ForeseeLog.ContainsKey(targetSeatNo))
                            {
                                JObject parms = new JObject();
                                parms.Add("Text", "text.wolfman_p8.actiondecide_foresee_repeat_modeltip");
                                ReturnGameTip(playerId, GameTipType.CommonModel, parms);
                                return;
                            }
                            //抉择结束 确定先知意图
                            prophet.Intention.IntentionType = ActionIntentionType.Foresee;
                            prophet.Intention.TargetSeatNo = targetSeat.SeatNo;
                        }
                        break;
                    case ActionDecisionType.Prophet_Foresee_Abandon:
                        {
                            if (seat.Identity.IdentityType != GameIdentityType.Prophet)
                                return;
                            if (seat.Identity.CurrentAction != CurrentActionType.Prophet_Foresee)
                                return;
                            if (seat.Identity.Intention.IntentionType != ActionIntentionType.Default)
                                return;
                            //先知不做动作
                            Prophet prophet = (Prophet)seat.Identity;
                            prophet.Intention.IntentionType = ActionIntentionType.Abandon;
                        }
                        break;
                    case ActionDecisionType.Wolfman_Kill_Excute:
                        {
                            if (seat.Identity.IdentityType != GameIdentityType.Wolfman)
                                return;
                            if (seat.Identity.CurrentAction != CurrentActionType.Wolfman_Kill)
                                return;
                            if (seat.Identity.Intention.IntentionType != ActionIntentionType.Default)
                                return;
                            Wolfman wolfman = (Wolfman)seat.Identity;
                            PlayerSeat targetSeat = m_playerSeats[targetSeatNo];
                            //条件:不能刀死人
                            if (targetSeat.Identity.isDead)
                            {
                                JObject parms = new JObject();
                                parms.Add("Text", "text.wolfman_p8.actiondecide_deadtarget_modeltip");
                                ReturnGameTip(playerId, GameTipType.CommonModel, parms);
                                return;
                            }
                            //裁判通知玩家
                            {
                                JObject parms = new JObject();
                                {
                                    parms.Add("SeatNo", targetSeat.SeatNo);
                                }
                                ReturnJudgeAnnounce(seat.PlayerId, JudgeAnnounceType.Wolfman_Kill_Excute, parms);
                            }
                            //抉择结束 确定狼人意图
                            wolfman.Intention.IntentionType = ActionIntentionType.Kill;
                            wolfman.Intention.TargetSeatNo = targetSeat.SeatNo;
                        }
                        break;
                    case ActionDecisionType.Witch_Magic_Save:
                        {
                            if (seat.Identity.IdentityType != GameIdentityType.Witch)
                                return;
                            if (seat.Identity.CurrentAction != CurrentActionType.Witch_Magic)
                                return;
                            if (seat.Identity.Intention.IntentionType != ActionIntentionType.Default)
                                return;
                            Witch witch = (Witch)seat.Identity;
                            if (!witch.Antidote)
                                return;
                            if (witch.PremonitionNo == -1)
                                return;
                            
                            PlayerSeat targetSeat = m_playerSeats[witch.PremonitionNo];
                            //条件:不能自救
                            if (targetSeat.SeatNo == seat.SeatNo)
                            {
                                JObject parms1 = new JObject();
                                parms1.Add("Text", "text.wolfman_p8.actiondecide_save_self_modeltip");
                                ReturnGameTip(playerId, GameTipType.CommonModel, parms1);
                                return;
                            }
                            //条件:不能救死人
                            if (targetSeat.Identity.isDead)
                            {
                                JObject parms1 = new JObject();
                                parms1.Add("Text", "text.wolfman_p8.actiondecide_deadtarget_modeltip");
                                ReturnGameTip(playerId, GameTipType.CommonModel, parms1);
                                return;
                            }
                            //抉择结束 确定女巫救人意图
                            witch.Intention.IntentionType = ActionIntentionType.Save;
                            witch.Intention.TargetSeatNo = targetSeat.SeatNo;
                        }
                        break;
                    case ActionDecisionType.Witch_Magic_Poison:
                        {
                            if (seat.Identity.IdentityType != GameIdentityType.Witch)
                                return;
                            if (seat.Identity.CurrentAction != CurrentActionType.Witch_Magic)
                                return;
                            if (seat.Identity.Intention.IntentionType != ActionIntentionType.Default)
                                return;
                            Witch witch = (Witch)seat.Identity;
                            if (!witch.Poison)
                                return;

                            PlayerSeat targetSeat = m_playerSeats[targetSeatNo];
                            //条件:不能毒死人
                            if (targetSeat.Identity.isDead)
                            {
                                JObject parms1 = new JObject();
                                parms1.Add("Text", "text.wolfman_p8.actiondecide_deadtarget_modeltip");
                                ReturnGameTip(playerId, GameTipType.CommonModel, parms1);
                                return;
                            }
                            //抉择结束 确定女巫毒人意图
                            witch.Intention.IntentionType = ActionIntentionType.Poison;
                            witch.Intention.TargetSeatNo = targetSeat.SeatNo;
                        }
                        break;
                    case ActionDecisionType.Witch_Magic_Abandon:
                        {
                            if (seat.Identity.IdentityType != GameIdentityType.Witch)
                                return;
                            if (seat.Identity.CurrentAction != CurrentActionType.Witch_Magic)
                                return;
                            if (seat.Identity.Intention.IntentionType != ActionIntentionType.Default)
                                return;
                            //女巫不做动作
                            Witch witch = (Witch)seat.Identity;
                            witch.Intention.IntentionType = ActionIntentionType.Abandon;
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
        private void ReturnGameTip(string playerId, GameTipType tipType, JObject parms)
        {
            JObject jsonObj = new JObject();
            jsonObj.Add("Action", "GameTip");
            JObject content = new JObject();
            {
                content.Add("TipType", (int)tipType);
                content.Add("Params", parms);
            }
            jsonObj.Add("Content", content);
            SendMessage(playerId, jsonObj.ToString());
        }
        private void BroadGameTip(GameTipType tipType, JObject parms)
        {
            JObject jsonObj = new JObject();
            jsonObj.Add("Action", "GameTip");
            JObject content = new JObject();
            {
                content.Add("TipType", (int)tipType);
                content.Add("Params", parms);
            }
            jsonObj.Add("Content", content);
            BroadMessage(jsonObj.ToString());
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
                seat.Identity = new GameIdentity();
                seat.IdentityExpection = GameIdentityType.Default;
            }
            m_isPlaying = false;
            m_publicProcess = PublicProcessState.PlayerReady;
            //2广播重置信息
            {
                JArray playerSeatArray = GetPlayerSeatJArray();
                JObject gameProperty = GetGamePropertyJObject();
                JObject playerProperty = GetPlayerPropertyJObject(m_playerSeats[0]);

                JArray changeArray = new JArray();
                JObject change1 = new JObject();
                change1.Add("JPath", "PlayerSeatArray");
                change1.Add("Value", playerSeatArray);
                changeArray.Add(change1);

                JObject change2 = new JObject();
                change2.Add("JPath", "GameProperty");
                change2.Add("Value", gameProperty);
                changeArray.Add(change2);

                JObject change3 = new JObject();
                change3.Add("JPath", "PlayerProperty");
                change3.Add("Value", playerProperty);
                changeArray.Add(change3);

                BroadPublicProcess(PublicProcessState.PlayerReady, changeArray);
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
        private void DefenderDefend_Begin(int seconds)
        {
            //1列举出守卫玩家
            IList<PlayerSeat> defenderSeats = new List<PlayerSeat>(m_playerSeats.Where((seat) =>
            {
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
                long waitTimeStamp = DateTime.UtcNow.AddSeconds(seconds).Ticks;
                defenderSeat.WaitTimestamp = waitTimeStamp;

                Defender defender = (Defender)defenderSeat.Identity;
                defender.CurrentAction = CurrentActionType.Defender_Defend;
                defender.Intention.IntentionType = ActionIntentionType.Default;
                defender.Intention.TargetSeatNo = -1;
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
                if (seat.Identity.IdentityType == GameIdentityType.Defender)
                {
                    Defender defender = (Defender)seat.Identity;
                    count++;
                    if (defender.Intention.IntentionType != ActionIntentionType.Default)
                        okCount++;
                }
            }

            if (count == okCount)
                return true;
            return false;
        }
        private void DefenderDefend_End()
        {
            //1列举出守卫玩家
            IList<PlayerSeat> defenderSeats = new List<PlayerSeat>(m_playerSeats.Where((seat) =>
            {
                //排除掉死亡的玩家
                if (seat.Identity.isDead)
                    return false;
                //排除掉不是守卫的玩家
                if (seat.Identity.IdentityType != GameIdentityType.Defender)
                    return false;

                return true;
            }));
            //2读取守卫所作意图 增加flag 返回响应
            foreach (PlayerSeat defenderSeat in defenderSeats)
            {
                Defender defender = (Defender)defenderSeat.Identity;
                ActionIntention intention = defender.Intention;
                if(intention.IntentionType == ActionIntentionType.Defend)
                {
                    int seatNo = intention.TargetSeatNo;
                    //
                    defender.LastDefendNo = seatNo;
                    //
                    PlayerSeat targetSeat = m_playerSeats[seatNo];
                    OperationFlag flag = new OperationFlag();
                    flag.FlagType = OperationFlagType.Defend;
                    flag.SourceSeatNo = defenderSeat.SeatNo;
                    targetSeat.Identity.FlagList.Add(flag);
                    //
                    JObject parms = new JObject();
                    {
                        parms.Add("SeatNo", seatNo);
                    }
                    ReturnJudgeAnnounce(defenderSeat.PlayerId, JudgeAnnounceType.Defender_Defend_Excute, parms);
                }
                else
                {
                    //
                    defender.LastDefendNo = -1;
                    //
                    JObject parms = new JObject();
                    ReturnJudgeAnnounce(defenderSeat.PlayerId, JudgeAnnounceType.Defender_Defend_Abandon, parms);
                }
                //重置意图状态
                defender.CurrentAction = CurrentActionType.Default;
                defender.Intention.IntentionType = ActionIntentionType.Default;
                defender.Intention.TargetSeatNo = -1;
            }
            //3客户端同步更新消息
            foreach (PlayerSeat defenderSeat in defenderSeats)
            {
                JArray changeArray = new JArray();
                {
                    JObject change1 = new JObject();
                    change1.Add("JPath", "PlayerProperty.Identity.CurrentAction");
                    change1.Add("Value", (int)defenderSeat.Identity.CurrentAction);
                    changeArray.Add(change1);
                }
                ReturnIdentityTranslate(defenderSeat.PlayerId, IdentityTranslateType.Defender_Defend_End, changeArray);
            }
        }
        private void ProphetForesee_Begin(int seconds)
        {
            //1列举出先知玩家
            IList<PlayerSeat> prophetSeats = new List<PlayerSeat>(m_playerSeats.Where((seat) =>
            {
                //排除掉死亡的玩家
                if (seat.Identity.isDead)
                    return false;
                //排除掉不是先知的玩家
                if (seat.Identity.IdentityType != GameIdentityType.Prophet)
                    return false;

                return true;
            }));
            //2切换当前动作-》先知验人
            foreach (PlayerSeat prophetSeat in prophetSeats)
            {
                long waitTimeStamp = DateTime.UtcNow.AddSeconds(seconds).Ticks;
                prophetSeat.WaitTimestamp = waitTimeStamp;
                Prophet prophet = (Prophet)prophetSeat.Identity;
                prophet.CurrentAction = CurrentActionType.Prophet_Foresee;
                prophet.Intention.IntentionType = ActionIntentionType.Default;
                prophet.Intention.TargetSeatNo = -1;
            }
            //3客户端同步更新消息
            foreach (PlayerSeat prophetSeat in prophetSeats)
            {
                JArray changeArray = new JArray();
                {
                    JObject change1 = new JObject();
                    change1.Add("JPath", "PlayerProperty.WaitTimestamp");
                    change1.Add("Value", prophetSeat.WaitTimestamp);
                    changeArray.Add(change1);
                    JObject change2 = new JObject();
                    change2.Add("JPath", "PlayerProperty.Identity.CurrentAction");
                    change2.Add("Value", (int)prophetSeat.Identity.CurrentAction);
                    changeArray.Add(change2);
                }
                ReturnIdentityTranslate(prophetSeat.PlayerId, IdentityTranslateType.Prophet_Foresee_Begin, changeArray);
            }
        }
        private bool ProphetForesee_Wait()
        {
            //等待所有先知做出选择 或者时间结束
            int count = 0;
            int okCount = 0;
            foreach (PlayerSeat seat in m_playerSeats)
            {
                if (seat.Identity.IdentityType == GameIdentityType.Prophet)
                {
                    Prophet prophet = (Prophet)seat.Identity;
                    count++;
                    if (prophet.Intention.IntentionType != ActionIntentionType.Default)
                        okCount++;
                }
            }

            if (count == okCount)
                return true;
            return false;
        }
        private void ProphetForesee_End()
        {
            //1列举出先知玩家
            IList<PlayerSeat> prophetSeats = new List<PlayerSeat>(m_playerSeats.Where((seat) =>
            {
                //排除掉死亡的玩家
                if (seat.Identity.isDead)
                    return false;
                //排除掉不是先知的玩家
                if (seat.Identity.IdentityType != GameIdentityType.Prophet)
                    return false;

                return true;
            }));
            //2读取先知的验人目标 返回查验信息
            foreach (PlayerSeat prophetSeat in prophetSeats)
            {
                Prophet prophet = (Prophet)prophetSeat.Identity;
                ActionIntention intention = prophet.Intention;
                if (intention.IntentionType == ActionIntentionType.Foresee)
                {
                    int seatNo = intention.TargetSeatNo;
                    //
                    JObject parms1 = new JObject();
                    {
                        parms1.Add("SeatNo", seatNo);
                    }
                    ReturnJudgeAnnounce(prophetSeat.PlayerId, JudgeAnnounceType.Prophet_Foresee_Excute, parms1);
                    //
                    PlayerSeat targetSeat = m_playerSeats[seatNo];
                    GameIdentityType identityType = targetSeat.Identity.IdentityType;
                    JObject parms2 = new JObject();
                    {
                        parms2.Add("SeatNo", seatNo);
                        parms2.Add("IdentityType", (int)identityType);
                    }
                    prophet.ForeseeLog[seatNo] = identityType;
                    ReturnJudgeAnnounce(prophetSeat.PlayerId, JudgeAnnounceType.Foresee_Result, parms2);
                }
                else
                {
                    JObject parms = new JObject();
                    ReturnJudgeAnnounce(prophetSeat.PlayerId, JudgeAnnounceType.Prophet_Foresee_Abandon, parms);
                }
                //重置意图
                prophet.CurrentAction = CurrentActionType.Default;
                prophet.Intention.IntentionType = ActionIntentionType.Default;
                prophet.Intention.TargetSeatNo = -1;
            }
            //3客户端同步更新消息
            foreach (PlayerSeat prophetSeat in prophetSeats)
            {
                JArray changeArray = new JArray();
                {
                    JObject change1 = new JObject();
                    change1.Add("JPath", "PlayerProperty.Identity.CurrentAction");
                    change1.Add("Value", (int)prophetSeat.Identity.CurrentAction);
                    changeArray.Add(change1);
                }
                ReturnIdentityTranslate(prophetSeat.PlayerId, IdentityTranslateType.Prophet_Foresee_End, changeArray);
            }
        }
        private void WolfmanKill_Begin(int seconds)
        {
            //1列举出狼人玩家
            IList<PlayerSeat> wolfmanSeats = new List<PlayerSeat>(m_playerSeats.Where((seat) =>
            {
                //排除掉死亡的玩家
                if (seat.Identity.isDead)
                    return false;
                //排除掉不是狼人的玩家
                if (seat.Identity.IdentityType != GameIdentityType.Wolfman)
                    return false;

                return true;
            }));
            //2切换当前动作-》狼人谋杀
            foreach (PlayerSeat wolfmanSeat in wolfmanSeats)
            {
                long waitTimeStamp = DateTime.UtcNow.AddSeconds(seconds).Ticks;
                wolfmanSeat.WaitTimestamp = waitTimeStamp;
                Wolfman wolfman = (Wolfman)wolfmanSeat.Identity;
                wolfman.CurrentAction = CurrentActionType.Wolfman_Kill;
                wolfman.Intention.IntentionType = ActionIntentionType.Default;
                wolfman.Intention.TargetSeatNo = -1;
                //HARDCODE 三次密语机会
                wolfman.WhisperTime = 3;
            }
            //3客户端同步更新消息
            foreach (PlayerSeat wolfmanSeat in wolfmanSeats)
            {
                JArray changeArray = new JArray();
                {
                    JObject change1 = new JObject();
                    change1.Add("JPath", "PlayerProperty.WaitTimestamp");
                    change1.Add("Value", wolfmanSeat.WaitTimestamp);
                    changeArray.Add(change1);
                    JObject change2 = new JObject();
                    change2.Add("JPath", "PlayerProperty.Identity.CurrentAction");
                    change2.Add("Value", (int)wolfmanSeat.Identity.CurrentAction);
                    changeArray.Add(change2);
                }
                ReturnIdentityTranslate(wolfmanSeat.PlayerId, IdentityTranslateType.Wolfman_Kill_Begin, changeArray);
            }
        }
        private bool WolfmanKill_Wait()
        {
            return false;
        }
        private void WolfmanKill_End(out int wolfmanKillSeatNo)
        {
            //1列举出狼人玩家
            IList<PlayerSeat> wolfmanSeats = new List<PlayerSeat>(m_playerSeats.Where((seat) =>
            {
                //排除掉死亡的玩家
                if (seat.Identity.isDead)
                    return false;
                //排除掉不是狼人的玩家
                if (seat.Identity.IdentityType != GameIdentityType.Wolfman)
                    return false;

                return true;
            }));
            //2统计狼人所作投票 重置意图
            IDictionary<int, int> m_wolfmanVoteCount = new Dictionary<int, int>();
            foreach (PlayerSeat wolfmanSeat in wolfmanSeats)
            {
                Wolfman wolfman = (Wolfman)wolfmanSeat.Identity;
                ActionIntention intention = wolfman.Intention;
                if (intention.IntentionType == ActionIntentionType.Kill)
                {
                    int seatNo = intention.TargetSeatNo;
                    //
                    PlayerSeat targetSeat = m_playerSeats[seatNo];
                    if (!m_wolfmanVoteCount.ContainsKey(seatNo))
                        m_wolfmanVoteCount[seatNo] = 0;
                    m_wolfmanVoteCount[seatNo]++;
                }
                else
                {
                    JObject parms = new JObject();
                    ReturnJudgeAnnounce(wolfmanSeat.PlayerId, JudgeAnnounceType.Wolfman_Kill_Abandon, parms);
                }
                //重置意图
                wolfman.CurrentAction = CurrentActionType.Default;
                wolfman.Intention.IntentionType = ActionIntentionType.Default;
                wolfman.Intention.TargetSeatNo = -1;
            }
            //3选出最终被杀的玩家 增加flag
            Random random = new Random();
            IList<int> killNoList = new List<int>();
            int maxVoteCount = 0;
            foreach(KeyValuePair<int, int> kv in m_wolfmanVoteCount)
            {
                int seatNo = kv.Key;
                int voteCount = kv.Value;

                if (voteCount > maxVoteCount)
                {
                    maxVoteCount = voteCount;
                    killNoList.Clear();
                    killNoList.Add(seatNo);
                }
                else if(voteCount == maxVoteCount)
                {
                    killNoList.Add(seatNo);
                }
            }
            int killNo = -1;
            if(killNoList.Count > 0)
            {
                killNo = killNoList[(int)(killNoList.Count * random.NextDouble())];
                //上flag
                PlayerSeat killedSeat = m_playerSeats[killNo];
                OperationFlag flag = new OperationFlag();
                flag.FlagType = OperationFlagType.Kill;
                killedSeat.Identity.FlagList.Add(flag);
            }
            wolfmanKillSeatNo = killNo;
            //4客户端同步更新消息
            foreach (PlayerSeat wolfmanSeat in wolfmanSeats)
            {
                JArray changeArray = new JArray();
                {
                    JObject change1 = new JObject();
                    change1.Add("JPath", "PlayerProperty.Identity.CurrentAction");
                    change1.Add("Value", (int)wolfmanSeat.Identity.CurrentAction);
                    changeArray.Add(change1);
                }
                ReturnIdentityTranslate(wolfmanSeat.PlayerId, IdentityTranslateType.Wolfman_Kill_End, changeArray);
            }
        }
        private void WitchMagic_Begin(int seconds, int premonitionNo)
        {
            //1列举出女巫玩家
            IList<PlayerSeat> witchSeats = new List<PlayerSeat>(m_playerSeats.Where((seat) =>
            {
                //排除掉死亡的玩家
                if (seat.Identity.isDead)
                    return false;
                //排除掉不是女巫的玩家
                if (seat.Identity.IdentityType != GameIdentityType.Witch)
                    return false;

                return true;
            }));
            //2切换当前动作-》女巫魔法
            foreach (PlayerSeat witchSeat in witchSeats)
            {
                long waitTimeStamp = DateTime.UtcNow.AddSeconds(seconds).Ticks;
                witchSeat.WaitTimestamp = waitTimeStamp;
                Witch witch = (Witch)witchSeat.Identity;

                witch.CurrentAction = CurrentActionType.Witch_Magic;
                witch.Intention.IntentionType = ActionIntentionType.Default;
                witch.Intention.TargetSeatNo = -1;
                //如果女巫有解药 则可以获取到被害消息
                if (witch.Antidote)
                {
                    witch.PremonitionNo = premonitionNo;
                }
                else
                {
                    witch.PremonitionNo = -1;
                }
            }
            //3客户端同步更新消息
            foreach (PlayerSeat witchSeat in witchSeats)
            {
                JArray changeArray = new JArray();
                {
                    JObject change1 = new JObject();
                    change1.Add("JPath", "PlayerProperty.WaitTimestamp");
                    change1.Add("Value", witchSeat.WaitTimestamp);
                    changeArray.Add(change1);
                    JObject change2 = new JObject();
                    change2.Add("JPath", "PlayerProperty.Identity");
                    change2.Add("Value", GetIdentityJObject(witchSeat.Identity));
                    changeArray.Add(change2);
                }
                ReturnIdentityTranslate(witchSeat.PlayerId, IdentityTranslateType.Witch_Magic_Begin, changeArray);
            }
        }
        private bool WitchMagic_Wait()
        {
            //等待所有女巫做出选择 或者时间结束
            int count = 0;
            int okCount = 0;
            foreach (PlayerSeat seat in m_playerSeats)
            {
                if (seat.Identity.IdentityType == GameIdentityType.Witch)
                {
                    Witch witch = (Witch)seat.Identity;
                    count++;
                    if (witch.Intention.IntentionType != ActionIntentionType.Default)
                        okCount++;
                }
            }

            if (count == okCount)
                return true;
            return false;
        }
        private void WitchMagic_End()
        {
            //1列举出女巫玩家
            IList<PlayerSeat> witchSeats = new List<PlayerSeat>(m_playerSeats.Where((seat) =>
            {
                //排除掉死亡的玩家
                if (seat.Identity.isDead)
                    return false;
                //排除掉不是女巫的玩家
                if (seat.Identity.IdentityType != GameIdentityType.Witch)
                    return false;

                return true;
            }));
            //2读取女巫所作意图 增加flag 返回响应
            foreach (PlayerSeat witchSeat in witchSeats)
            {
                Witch witch = (Witch)witchSeat.Identity;
                ActionIntention intention = witch.Intention;
                if (intention.IntentionType == ActionIntentionType.Save)
                {
                    int seatNo = intention.TargetSeatNo;
                    //消耗解药
                    witch.Antidote = false;
                    //
                    PlayerSeat targetSeat = m_playerSeats[seatNo];
                    OperationFlag flag = new OperationFlag();
                    flag.FlagType = OperationFlagType.Save;
                    flag.SourceSeatNo = witchSeat.SeatNo;
                    targetSeat.Identity.FlagList.Add(flag);

                    JObject parms = new JObject();
                    {
                        parms.Add("SeatNo", seatNo);
                    }
                    ReturnJudgeAnnounce(witchSeat.PlayerId, JudgeAnnounceType.Witch_Magic_Save, parms);
                }
                else if(intention.IntentionType == ActionIntentionType.Poison)
                {
                    int seatNo = intention.TargetSeatNo;
                    //消耗毒药
                    witch.Poison = false;
                    //
                    PlayerSeat targetSeat = m_playerSeats[seatNo];
                    OperationFlag flag = new OperationFlag();
                    flag.FlagType = OperationFlagType.Poison;
                    flag.SourceSeatNo = witchSeat.SeatNo;
                    targetSeat.Identity.FlagList.Add(flag);

                    JObject parms = new JObject();
                    {
                        parms.Add("SeatNo", seatNo);
                    }
                    ReturnJudgeAnnounce(witchSeat.PlayerId, JudgeAnnounceType.Witch_Magic_Poison, parms);
                }
                else
                {
                    JObject parms = new JObject();
                    ReturnJudgeAnnounce(witchSeat.PlayerId, JudgeAnnounceType.Witch_Magic_Abandon, parms);
                }
                //重置意图状态
                witch.CurrentAction = CurrentActionType.Default;
                witch.Intention.IntentionType = ActionIntentionType.Default;
                witch.Intention.TargetSeatNo = -1;
            }
            //3客户端同步更新消息
            foreach (PlayerSeat witchSeat in witchSeats)
            {
                JArray changeArray = new JArray();
                {
                    JObject change1 = new JObject();
                    change1.Add("JPath", "PlayerProperty.Identity");
                    change1.Add("Value", GetIdentityJObject(witchSeat.Identity));
                    changeArray.Add(change1);
                }
                ReturnIdentityTranslate(witchSeat.PlayerId, IdentityTranslateType.Witch_Magic_End, changeArray);
            }
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
                //HARDCODE 3秒
                GameStartPrepare();
                stopwatch.Restart();
                while (stopwatch.ElapsedMilliseconds < 3000)
                    yield return 0;
                //游戏正式开始
                GameStart();
                //分配身份 10秒钟确认时间
                //HARDCODE 10秒
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
                    //HARDCODE 20秒
                    DefenderDefend_Begin(20);
                    stopwatch.Restart();
                    while (stopwatch.ElapsedMilliseconds < 20 * 1000 && !DefenderDefend_Wait())
                        yield return 0;
                    DefenderDefend_End();
                    //预言家行动
                    //HARDCODE 20秒
                    ProphetForesee_Begin(20);
                    stopwatch.Restart();
                    while (stopwatch.ElapsedMilliseconds < 20 * 1000 && !ProphetForesee_Wait())
                        yield return 0;
                    ProphetForesee_End();
                    //狼人行动
                    //HARDCODE 30秒
                    int wolfmanKillSeatNo = -1;
                    WolfmanKill_Begin(30);
                    stopwatch.Restart();
                    while (stopwatch.ElapsedMilliseconds < 30 * 1000 && !WolfmanKill_Wait())
                        yield return 0;
                    WolfmanKill_End(out wolfmanKillSeatNo);
                    //女巫行动
                    //HARDCODE 20秒
                    WitchMagic_Begin(20, wolfmanKillSeatNo);
                    stopwatch.Restart();
                    while (stopwatch.ElapsedMilliseconds < 20 * 1000 && !WitchMagic_Wait())
                        yield return 0;
                    WitchMagic_End();
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
