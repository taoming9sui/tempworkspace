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
            ReceiveJoinGame(playerId, info);
        }

        protected override void OnPlayerLeave(string playerId)
        {
            ReceiveLeaveGame(playerId);
        }

        protected override void OnPlayerMessage(string playerId, string msgData)
        {
            JObject jsonObj = JObject.Parse(msgData);
            string action = jsonObj.GetValue("Action").ToString();
            switch (action)
            {
                case "SynchronizeState":
                    ReceiveSynchronizeState(playerId);
                    break;
                case "BaseFunction":
                    ReceiveBaseFunction(playerId,
                        (BaseFunctionType)(int)jsonObj.SelectToken("Content.FunctionType"),
                        (JObject)jsonObj.SelectToken("Content.Params"));
                    break;
                case "IdentityActionDecide":
                    ReceiveIdentityActionDecide(playerId, 
                        (ActionDecisionType)(int)jsonObj.SelectToken("Content.DecisionType"),
                        (int)jsonObj.SelectToken("Content.TargetSeatNo"));
                    break;
            }
        }

        protected override void LogicUpdate(long milliseconds)
        {
            //主循环
            m_gameFlowLoop.MoveNext();
        }
        #endregion

    }
}
