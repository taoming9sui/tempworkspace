using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSocket.GameServer.GameModuels;
using WebSocket.Exceptions;

namespace WebSocket.GameServer.ServerObjects
{
    public class CenterRoom
    {
        public enum Status { Default, Vacant , Full, Playing };

        private IDictionary<string, PlayerInfo> m_playerSet;
        private GameCenter m_center;
        private GameModuel m_game;
        private string m_roomId;
        private string m_title;
        private string m_password;

        public CenterRoom(GameCenter center, string roomId, string gameId, string title, string password = null)
        {
            m_center = center;
            m_roomId = roomId;
            m_game = GameModuelLoader.GetGameInstance(gameId, this);
            m_title = title;
            m_password = password;

            m_playerSet = new Dictionary<string, PlayerInfo>();
        }

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
                if (!m_game.IsOpened)
                    return Status.Playing;
                if (m_playerSet.Count >= m_game.MaxPlayerCount)
                    return Status.Full;
                if (m_playerSet.Count < m_game.MaxPlayerCount)
                    return Status.Vacant;

                return Status.Default;
            }
        }
        public string GameId
        {
            get
            {
                return this.m_game.GameId;
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
        public void StartGame()
        {
            m_game.Start();
        }
        public void StopGame()
        {
            m_game.Stop();
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
        public void PlayerOffline(string playerId)
        {
            GameModuel.QueueEventArgs eventArgs = new GameModuel.QueueEventArgs();
            eventArgs.Type = GameModuel.QueueEventArgs.MessageType.Disconnect;
            eventArgs.Param1 = playerId;
            m_game.PushMessage(eventArgs);
        }
        public void PlayerReConnect(string playerId)
        {
            GameModuel.QueueEventArgs eventArgs = new GameModuel.QueueEventArgs();
            eventArgs.Type = GameModuel.QueueEventArgs.MessageType.Connect;
            eventArgs.Param1 = playerId;
            m_game.PushMessage(eventArgs);
        }
        public void GameMessageReceive(string playerId, string data)
        {
            GameModuel.QueueEventArgs eventArgs = new GameModuel.QueueEventArgs();
            eventArgs.Type = GameModuel.QueueEventArgs.MessageType.Message;
            eventArgs.Data = data;
            eventArgs.Param1 = playerId;
            m_game.PushMessage(eventArgs);
        }
        #endregion

        #region 面向游戏调用
        public void GameMessageResponse(string playerId, string data)
        {
            m_center.RoomResponse(playerId, data);
        }
        public void GameMessageBroad(string data)
        {
            foreach(string playerId in m_playerSet.Keys)
            {
                m_center.RoomResponse(playerId, data);
            }
        }
        #endregion

    }
}