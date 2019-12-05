using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSocket.GameServer.GameModuels;
using WebSocket.Exceptions;

namespace WebSocket.GameServer.ServerObjects
{
    public class ServerRoom
    {
        public enum Status { Default, Vacant , Full, Playing };

        private GameClientAgent m_clientAgent;
        private IDictionary<string, PlayerInfo> m_playerSet;
        private GameModuel m_game;
        private string m_roomId;
        private string m_title;
        private string m_password;

        #region 面向大厅调用
        public int MaxPlayerCount
        {
            get
            {
                return m_game.MaxPlayerCount;
            }
        }
        public int PlayerCount
        {
            get
            {
                return m_playerSet.Count;
            }
        }
        public Status RoomStatus
        {
            get
            {
                if (!m_game.isOpened)
                    return Status.Playing;
                if (m_playerSet.Count >= m_game.MaxPlayerCount)
                    return Status.Full;
                if (m_playerSet.Count < m_game.MaxPlayerCount)
                    return Status.Vacant;

                return Status.Default;
            }
        }
        public string RoomId
        {
            get
            {
                return m_roomId;
            }
        }
        public string RoomTitle
        {
            get
            {
                return m_title;
            }
        }
        public string RoomPassword
        {
            get
            {
                return m_password;
            }
        }
        public void PlayerJoin(string playerId, PlayerInfo playerInfo)
        {
            GameModuel.QueueEventArgs eventArgs = new GameModuel.QueueEventArgs();
            eventArgs.Type = GameModuel.QueueEventArgs.MessageType.Join;
            eventArgs.Param1 = playerId;
            eventArgs.Param2 = playerInfo;
            m_game.PushMessage(eventArgs);

            if (!m_playerSet.ContainsKey(playerId))
                m_playerSet[playerId] = playerInfo;
        }
        public void PlayerLeave(string playerId)
        {
            GameModuel.QueueEventArgs eventArgs = new GameModuel.QueueEventArgs();
            eventArgs.Type = GameModuel.QueueEventArgs.MessageType.Leave;
            eventArgs.Param1 = playerId;
            m_game.PushMessage(eventArgs);

            if (m_playerSet.ContainsKey(playerId))
                m_playerSet.Remove(playerId);
        }
        public void PlayerDisconnect(string playerId)
        {
        }
        public void PlayerReConnect(string playerId)
        {
        }
        public void GameMessageReceive(string playerId, string data)
        {
        }
        #endregion

        #region 面向游戏调用
        protected void GameMessageResponse(string playerId, string data)
        {

        }
        protected void GameMessageBroad(string data)
        {

        }
        #endregion

    }
}