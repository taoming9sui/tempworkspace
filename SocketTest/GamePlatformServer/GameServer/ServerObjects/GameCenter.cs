using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;
using Newtonsoft.Json.Linq;
using GamePlatformServer.Utils;
using System.Data.SQLite;
using GamePlatformServer.Exceptions;
using System.Text.RegularExpressions;
using System.Data;

namespace GamePlatformServer.GameServer.ServerObjects
{
    public class GameCenter
    {
        public class QueueEventArgs : EventArgs
        {
            public enum MessageType { None, Client_Center, Client_Hall, Client_Room };

            public MessageType Type { get; set; }
            public string Data { get; set; }
            public Object Param1;
            public Object Param2;
        }
        private ConcurrentQueue<QueueEventArgs> m_eventQueue;
        private Thread m_loopThread;
        private bool m_loopThreadExit = false;

        private GameServerContainer m_serverContainer;
        private IDictionary<string, bool> m_socketIdSet;
        private IDictionary<string, CenterPlayer> m_playerSet;
        private IDictionary<string, CenterRoom> m_roomSet;
        private IDictionary<string, string> m_mapperSocketIdtoPlayerId;
        private IDictionary<string, int> m_updateTimerSet;
        private string m_roomListJsonData;

        public GameCenter(GameServerContainer container)
        {
            //服务容器引用
            m_serverContainer = container;
            //客户端会话id集
            m_socketIdSet = new Dictionary<string, bool>();
            //玩家对象集
            m_playerSet = new Dictionary<string, CenterPlayer>();
            //房间集
            m_roomSet = new Dictionary<string, CenterRoom>();
            //客户端会话id到玩家id的关系集
            m_mapperSocketIdtoPlayerId = new Dictionary<string, string>();
            //计时器
            InitTimer();
        }
        public void Start()
        {
            if (m_loopThread != null)
            {
                if (m_loopThread.IsAlive)
                {
                    return;
                }
            }

            m_eventQueue = new ConcurrentQueue<QueueEventArgs>();
            m_loopThread = new Thread(Run);
            m_loopThreadExit = false;
            m_loopThread.Start();
        }
        public void Stop()
        {
            m_loopThreadExit = true;
        }

        #region 消息队列循环
        public void PushMessage(QueueEventArgs eventArgs)
        {
            if (m_eventQueue != null)
                m_eventQueue.Enqueue(eventArgs);
        }
        private void Begin()
        {
            try
            {
            }
            catch (Exception ex) { LogHelper.LogError(ex.Message + "|" + ex.StackTrace); }
        }
        private void Finish()
        {
            try
            {
                StopHallRooms();
            }
            catch (Exception ex) { LogHelper.LogError(ex.Message + "|" + ex.StackTrace); }         
        }
        private void Run()
        {
            Begin();
            while (!m_loopThreadExit)
            {
                QueueEventArgs eventArgs;
                while (m_eventQueue.TryDequeue(out eventArgs))
                {
                    switch (eventArgs.Type)
                    {
                        case QueueEventArgs.MessageType.Client_Center:
                            this.OnClient_Center(eventArgs.Data, (string)eventArgs.Param1);
                            break;
                        case QueueEventArgs.MessageType.Client_Hall:
                            this.OnClient_Hall(eventArgs.Data, (string)eventArgs.Param1);
                            break;
                        case QueueEventArgs.MessageType.Client_Room:
                            this.OnClient_Room(eventArgs.Data, (string)eventArgs.Param1);
                            break;
                    }
                }
                LogicUpdate();
                Thread.Sleep(1);
            }
            Finish();
        }
        #endregion


        #region 计时器
        private void InitTimer()
        {
            m_updateTimerSet = new Dictionary<string, int>();
            m_updateTimerSet.Add("RoomListUpdate", 0);
        }
        private void LogicUpdate()
        {
            string[] keys = m_updateTimerSet.Keys.ToArray();
            foreach (string key in keys)
                m_updateTimerSet[key]++;

            //间隔一秒更新房间列表
            {
                int t = m_updateTimerSet["RoomListUpdate"];
                if (t > 1000)
                {
                    m_updateTimerSet["RoomListUpdate"] = 0;
                    JArray jsonArray = new JArray();
                    foreach (CenterRoom room in m_roomSet.Values)
                    {
                        JObject jobj = new JObject();
                        jobj.Add("RoomId", room.RoomId);
                        jobj.Add("RoomTitle", room.RoomTitle);
                        jobj.Add("GameId", room.GameId);
                        jobj.Add("RoomStatus", (int)room.RoomStatus);
                        jobj.Add("PlayerCount", room.PlayerCount);
                        jobj.Add("MaxPlayerCount", room.MaxPlayerCount);
                        jobj.Add("HasPassword", !String.IsNullOrEmpty(room.RoomPassword));
                        jsonArray.Add(jobj);
                    }
                    m_roomListJsonData = jsonArray.ToString();
                }
            }

        }
        #endregion


        #region 游戏平台工作
        private void OnClient_Center(string data, string socketId)
        {
            try
            {
                JObject jsonObj = JObject.Parse(data);
                string action = jsonObj.GetValue("Action").ToString();
                switch (action)
                {
                    case "Register":
                        ClientRegister(jsonObj.GetValue("PlayerId").ToString(), jsonObj.GetValue("Password").ToString(), socketId);
                        break;
                    case "Login":
                        ClientLogin(jsonObj.GetValue("PlayerId").ToString(), jsonObj.GetValue("Password").ToString(), socketId);
                        break;
                    case "Logout":
                        ClientLogout(socketId);
                        break;
                    case "Connect":
                        ClientConnect(socketId);
                        break;
                    case "Disconnect":
                        ClientDisconnect(socketId);
                        break;
                }
            }
            catch (Exception ex) { LogHelper.LogError(ex.Message + "|" + ex.StackTrace); }
        }
        private void ClientRegister(string playerId, string password, string socketId)
        {
            bool flag = false;

            try
            {
                m_serverContainer.PlayerDBAgent.PlayerRegister(playerId, password);
                flag = true;
            }
            catch (InfoException ex)
            {
                CenterUserTip(socketId, ex.Message);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(ex.Message + "|" + ex.StackTrace);
            }

            if (flag)
            {
                //2客户端返回注册成功
                JObject jsonObj = new JObject();
                jsonObj.Add("Action", "RegisterSuccess");
                CenterResponse(socketId, jsonObj.ToString());
            }
        }
        private void ClientLogin(string playerId, string password, string socketId)
        {
            bool flag = false;

            try
            {
                m_serverContainer.PlayerDBAgent.PlayerLogin(playerId, password);
                flag = true;
            }
            catch (InfoException ex)
            {
                CenterUserTip(socketId, ex.Message);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(ex.Message + "|" + ex.StackTrace);
            }

            if (flag)
            {
                //1管理玩家对象
                if (m_playerSet.ContainsKey(playerId))
                {
                    //挂起玩家重连
                    m_mapperSocketIdtoPlayerId[socketId] = playerId;
                    CenterPlayer player = m_playerSet[playerId];
                    player.SocketId = socketId;
                    //通知大厅有玩家重连
                    PlayerReconnect(player);
                }
                else
                {
                    //新玩家上线
                    m_mapperSocketIdtoPlayerId[socketId] = playerId;
                    CenterPlayer player = new CenterPlayer();
                    player.PlayerId = playerId;
                    player.InRoomId = null;
                    player.SocketId = socketId;
                    PlayerInfo info;
                    info.Name = playerId;
                    player.Info = info;
                    m_playerSet[playerId] = player;
                    //通知大厅有玩家上线
                    PlayerOnline(player);
                }
                //2客户端返回登录成功
                JObject jsonObj = new JObject();
                jsonObj.Add("Action", "LoginSuccess");
                CenterResponse(socketId, jsonObj.ToString());
            }
        }
        private void ClientLogout(string socketId)
        {
            //1管理玩家对象
            string playerId = null;
            m_mapperSocketIdtoPlayerId.TryGetValue(socketId, out playerId);
            if (playerId != null)
            {
                CenterPlayer player = null;
                m_playerSet.TryGetValue(playerId, out player);
                if (player != null)
                {
                    PlayerLeaveRoom(player);
                    m_playerSet.Remove(playerId);
                }
            }
            m_mapperSocketIdtoPlayerId.Remove(socketId);
            //2客户端返回登出成功
            JObject jsonObj = new JObject();
            jsonObj.Add("Action", "LogoutSuccess");
            CenterResponse(socketId, jsonObj.ToString());
        }
        private void ClientConnect(string socketId)
        {
            m_socketIdSet[socketId] = true;
        }
        private void ClientDisconnect(string socketId)
        {
            string playerId = null;
            m_mapperSocketIdtoPlayerId.TryGetValue(socketId, out playerId);
            if (playerId != null)
            {
                CenterPlayer player = null;
                m_playerSet.TryGetValue(playerId, out player);
                if (player != null)
                {
                    PlayerOffline(player);
                    player.SocketId = null;
                }
            }
            m_mapperSocketIdtoPlayerId.Remove(socketId);
            m_socketIdSet.Remove(socketId);
        }
        private void CenterUserTip(string socketId, string tip)
        {
            JObject jsonObj = new JObject();
            jsonObj.Add("Action", "Tip");
            jsonObj.Add("Content", tip);
            CenterResponse(socketId, jsonObj.ToString());
        }
        private void CenterResponse(string socketId, string data)
        {
            GameClientAgent.QueueEventArgs eventArgs = new GameClientAgent.QueueEventArgs();
            JObject jsonObj = new JObject();
            jsonObj.Add("Type", "Server_Center");
            jsonObj.Add("Data", JObject.Parse(data));
            eventArgs.Data = jsonObj.ToString();
            eventArgs.Param1 = socketId;
            m_serverContainer.ClientAgent.PushMessage(eventArgs);
        }
        private void CenterBroadcast(string data)
        {
            JObject jsonObj = new JObject();
            jsonObj.Add("Type", "Server_Center");
            jsonObj.Add("Data", JObject.Parse(data));
            string jsonStr = jsonObj.ToString();
            foreach (string socketId in m_socketIdSet.Keys)
            {
                GameClientAgent.QueueEventArgs eventArgs = new GameClientAgent.QueueEventArgs();
                eventArgs.Data = jsonStr;
                eventArgs.Param1 = socketId;
                m_serverContainer.ClientAgent.PushMessage(eventArgs);
            }
        }
        #endregion


        #region 大厅工作
        private void OnClient_Hall(string data, string socketId)
        {
            try
            {
                CenterPlayer player = null;
                try
                {
                    string playerId = m_mapperSocketIdtoPlayerId[socketId];
                    player = m_playerSet[playerId];
                }
                catch { }

                JObject jsonObj = JObject.Parse(data);
                string action = jsonObj.GetValue("Action").ToString();
                switch (action)
                {
                    case "CreateRoom":
                        PlayerCreateRoom(player, jsonObj.GetValue("GameId").ToString(), jsonObj.GetValue("RoomTitle").ToString(), jsonObj.GetValue("RoomPassword").ToString());
                        break;
                    case "JoinRoom":
                        PlayerJoinRoom(player, jsonObj.GetValue("RoomId").ToString(), jsonObj.GetValue("Password").ToString());
                        break;
                    case "LeaveRoom":
                        PlayerLeaveRoom(player);
                        break;
                    case "RequestRoomList":
                        PlayerRequestRoomList(player);
                        break;
                }

            }
            catch (Exception ex) { LogHelper.LogError(ex.Message + "|" + ex.StackTrace); }
        }
        private void StopHallRooms()
        {
            foreach (CenterRoom room in m_roomSet.Values)
                room.StopGame();
        }
        private void PlayerOnline(CenterPlayer player)
        {
        }
        private void PlayerOffline(CenterPlayer player)
        {
            if (player.InRoomId != null)
            {
                CenterRoom room = null;
                m_roomSet.TryGetValue(player.InRoomId, out room);
                if (room != null)
                {
                    room.PlayerOffline(player.PlayerId);
                }
            }
        }
        private void PlayerReconnect(CenterPlayer player)
        {
            if (player.InRoomId != null)
            {
                CenterRoom room = null;
                m_roomSet.TryGetValue(player.InRoomId, out room);
                if (room != null)
                {
                    room.PlayerReConnect(player.PlayerId, player.SocketId);
                }
            }
        }
        private void PlayerCreateRoom(CenterPlayer player, string gameId, string roomTitle, string roomPassword)
        {
            if (player.InRoomId != null)
            {
                HallUserTip(player, "已经在一个房间中");
                return;
            }
            {
                Regex roomTitle_reg = new Regex(@"^[\u4E00-\u9FA5A-Za-z0-9_]{3,16}$");  //3-16位汉字数字字母
                if (!roomTitle_reg.IsMatch(roomTitle))
                {
                    HallUserTip(player, "房间名格式错误");
                    return;
                }
            }
            {
                Regex roomPassword_reg = new Regex(@"^.{0,16}$");  //3-16位
                if (!roomPassword_reg.IsMatch(roomPassword))
                {
                    HallUserTip(player, "房间密码格式错误");
                    return;
                }
            }

            {
                string roomId = SecurityHelper.CreateGuid();
                while (m_roomSet.ContainsKey(roomId))
                    roomId = SecurityHelper.CreateGuid();
                CenterRoom room = new CenterRoom(m_serverContainer, roomId, gameId, roomTitle, roomPassword);
                room.StartGame();
                room.PlayerJoin(player.PlayerId, player.SocketId, player.Info);
                player.InRoomId = room.RoomId;
                //向客户端返回信息
                JObject jsonObj = new JObject();
                jsonObj.Add("Action", "InRoom");
                JObject content = new JObject();
                content.Add("RoomId", room.RoomId);
                jsonObj.Add("Content", content);
                HallResponse(player, jsonObj.ToString());
            }
        }
        private void PlayerJoinRoom(CenterPlayer player, string roomId, string password)
        {
            if (player.InRoomId != null)
            {
                HallUserTip(player, "已经在一个房间中");
                return;
            }
            CenterRoom room = null;
            m_roomSet.TryGetValue(player.InRoomId, out room);
            if (room == null)
            {
                HallUserTip(player, "意料之外的房间");
                return;
            }
            if (room.RoomStatus == CenterRoom.Status.Full)
            {
                HallUserTip(player, "房间已满员");
                return;
            }
            if (room.RoomStatus == CenterRoom.Status.Playing)
            {
                HallUserTip(player, "该房间正在进行中");
                return;
            }
            if (room.RoomPassword != null && !room.RoomPassword.Equals(password))
            {
                HallUserTip(player, "房间密码错误");
                return;
            }

            {
                room.PlayerJoin(player.PlayerId, player.SocketId, player.Info);
                player.InRoomId = room.RoomId;
                //向客户端返回信息
                JObject jsonObj = new JObject();
                jsonObj.Add("Action", "InRoom");
                JObject content = new JObject();
                content.Add("RoomId", room.RoomId);
                jsonObj.Add("Content", content);
                HallResponse(player, jsonObj.ToString());
            }
        }
        private void PlayerLeaveRoom(CenterPlayer player)
        {
            if (player.InRoomId == null)
                return;

            CenterRoom room = null;
            m_roomSet.TryGetValue(player.InRoomId, out room);
            if (room != null)
            {
                room.PlayerLeave(player.PlayerId);
                if (room.PlayerCount == 0)
                {
                    room.StopGame();
                    m_roomSet.Remove(room.RoomId);
                }
            }
            player.InRoomId = null;
        }
        private void PlayerHallChat(CenterPlayer player, string chat)
        {
            JObject jsonObj = new JObject();
            jsonObj.Add("Action", "Chat");
            JObject content = new JObject();
            content.Add("Chat", chat);
            content.Add("Sender", player.Info.Name);
            jsonObj.Add("Content", content);
            HallBroadcast(jsonObj.ToString());
        }
        private void PlayerRequestRoomList(CenterPlayer player)
        {
            JObject jsonObj = new JObject();
            jsonObj.Add("Action", "ResponseRoomList");
            JObject content = new JObject();
            content.Add("RoomList", m_roomListJsonData);
            jsonObj.Add("Content", content);
            HallResponse(player, jsonObj.ToString());
        }
        private void HallNotice(string notice, int level)
        {
            JObject jsonObj = new JObject();
            jsonObj.Add("Action", "Notice");
            JObject content = new JObject();
            content.Add("Notice", notice);
            content.Add("Level", level.ToString());
            jsonObj.Add("Content", content);

        }
        private void HallUserTip(CenterPlayer player, string tip)
        {
            JObject jsonObj = new JObject();
            jsonObj.Add("Action", "Tip");
            jsonObj.Add("Content", tip);
            HallResponse(player, jsonObj.ToString());
        }
        private void HallResponse(CenterPlayer player, string data)
        {
            if (player.SocketId == null)
                return;

            GameClientAgent.QueueEventArgs eventArgs = new GameClientAgent.QueueEventArgs();
            JObject jsonObj = new JObject();
            jsonObj.Add("Type", "Server_Hall");
            jsonObj.Add("Data", JObject.Parse(data));
            eventArgs.Data = jsonObj.ToString();
            eventArgs.Param1 = player.SocketId;
            m_serverContainer.ClientAgent.PushMessage(eventArgs);
        }
        private void HallBroadcast(string data)
        {
            JObject jsonObj = new JObject();
            jsonObj.Add("Type", "Server_Hall");
            jsonObj.Add("Data", JObject.Parse(data));
            string jsonStr = jsonObj.ToString();
            foreach (CenterPlayer player in m_playerSet.Values)
            {
                if (player.SocketId == null)
                    continue;

                GameClientAgent.QueueEventArgs eventArgs = new GameClientAgent.QueueEventArgs(); ;
                eventArgs.Data = jsonStr;
                eventArgs.Param1 = player.SocketId;
                m_serverContainer.ClientAgent.PushMessage(eventArgs);
            }
        }
        #endregion


        #region 游戏房间工作
        private void OnClient_Room(string data, string socketId)
        {
            try
            {
                CenterPlayer player = null;
                CenterRoom room = null;

                {
                    string playerId = null;
                    m_mapperSocketIdtoPlayerId.TryGetValue(socketId, out playerId);
                    if (playerId != null)
                        m_playerSet.TryGetValue(playerId, out player);
                    if (player != null)
                        if (player.InRoomId != null)
                            m_roomSet.TryGetValue(player.InRoomId, out room);
                }

                if (player != null && room != null)
                {
                    RoomReceive(player, room, data);
                }
            }
            catch (Exception ex) { LogHelper.LogError(ex.Message + "|" + ex.StackTrace); }
        }
        private void RoomReceive(CenterPlayer player, CenterRoom room, string data)
        {
            room.GameMessageReceive(player.PlayerId, data);
        }
        #endregion

    }
}
