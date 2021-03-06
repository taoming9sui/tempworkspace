﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GamePlatformServer.GameServer.GameModuels;
using GamePlatformServer.Exceptions;
using System.Data;
using Newtonsoft.Json.Linq;

namespace GamePlatformServer.GameServer.ServerObjects
{
    public class CenterRoom
    {
        public enum Status { Default, Vacant , Full, Playing };

        private GameServerContainer m_serverContainer;
        private GameModuel m_game;
        private DataTable m_playerSet;
        private string m_roomId;
        private string m_caption;
        private string m_password;

        public CenterRoom(GameServerContainer container, string roomId, string gameId, string caption, string password = null)
        {
            m_serverContainer = container;
            m_roomId = roomId;
            m_game = GameModuelLoader.GetGameInstance(gameId, container);
            m_caption = caption;
            m_password = password;

            m_playerSet = new DataTable();
            DataColumn priKey = m_playerSet.Columns.Add("PlayerId", typeof(string));
            m_playerSet.Columns.Add("SocketId", typeof(string));
            m_playerSet.Columns.Add("PlayerInfo", typeof(PlayerInfo));
            m_playerSet.PrimaryKey = new DataColumn[] { priKey };
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
                return m_playerSet.Rows.Count;
            }
        }
        public Status RoomStatus
        {
            get
            {
                if (!m_game.IsOpened)
                    return Status.Playing;
                if (m_playerSet.Rows.Count >= m_game.MaxPlayerCount)
                    return Status.Full;
                if (m_playerSet.Rows.Count < m_game.MaxPlayerCount)
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
        public string RoomCaption
        {
            get
            {
                return m_caption;
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
        public void PlayerJoin(string playerId, string socketId, PlayerInfo playerInfo)
        {
            GameModuel.QueueEventArgs eventArgs = new GameModuel.QueueEventArgs();
            eventArgs.Type = GameModuel.QueueEventArgs.MessageType.Join;
            eventArgs.Data = socketId;
            eventArgs.Param1 = playerId;
            eventArgs.Param2 = playerInfo;
            m_game.PushMessage(eventArgs);

            DataRow playerRow = m_playerSet.Rows.Find(playerId);
            if (playerRow == null)
            {
                playerRow = m_playerSet.NewRow();
                playerRow["PlayerId"] = playerId;
                playerRow["SocketId"] = socketId;
                playerRow["PlayerInfo"] = playerInfo;
                m_playerSet.Rows.Add(playerRow);
            }
                
        }
        public void PlayerLeave(string playerId)
        {
            GameModuel.QueueEventArgs eventArgs = new GameModuel.QueueEventArgs();
            eventArgs.Type = GameModuel.QueueEventArgs.MessageType.Leave;
            eventArgs.Param1 = playerId;
            m_game.PushMessage(eventArgs);

            DataRow playerRow = m_playerSet.Rows.Find(playerId);
            if (playerRow != null)
                m_playerSet.Rows.Remove(playerRow);
        }
        public void PlayerOffline(string playerId)
        {
            GameModuel.QueueEventArgs eventArgs = new GameModuel.QueueEventArgs();
            eventArgs.Type = GameModuel.QueueEventArgs.MessageType.Disconnect;
            eventArgs.Param1 = playerId;
            m_game.PushMessage(eventArgs);

            DataRow playerRow = m_playerSet.Rows.Find(playerId);
            if (playerRow != null)
                playerRow["SocketId"] = null;
        }
        public void PlayerReconnect(string playerId, string socketId)
        {
            GameModuel.QueueEventArgs eventArgs = new GameModuel.QueueEventArgs();
            eventArgs.Type = GameModuel.QueueEventArgs.MessageType.Connect;
            eventArgs.Data = socketId;
            eventArgs.Param1 = playerId;
            m_game.PushMessage(eventArgs);

            DataRow playerRow = m_playerSet.Rows.Find(playerId);
            if (playerRow != null)
                playerRow["SocketId"] = socketId;
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

    }
}