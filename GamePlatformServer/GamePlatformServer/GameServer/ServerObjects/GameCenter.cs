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
using System.Diagnostics;

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
        private GameCenterDBAgent m_centerDBAgent;
        private IDictionary<string, bool> m_socketIdSet;
        private IDictionary<string, CenterPlayer> m_playerSet;
        private IDictionary<string, CenterRoom> m_roomSet;
        private IDictionary<string, string> m_mapperSocketIdtoPlayerId;
        private IDictionary<string, long> m_updateTimerSet;
        private List<OfflinePlayerRecord> m_offlineRecordList;
        private string m_roomListJsonData;

        public GameCenter(GameServerContainer container, string sqliteConnStr)
        {
            //服务容器引用
            m_serverContainer = container;
            //数据库操作者
            m_centerDBAgent = new GameCenterDBAgent(sqliteConnStr);
            //客户端会话id集
            m_socketIdSet = new Dictionary<string, bool>();
            //玩家对象集
            m_playerSet = new Dictionary<string, CenterPlayer>();
            //房间集
            m_roomSet = new Dictionary<string, CenterRoom>();
            //客户端会话id到玩家id的关系集
            m_mapperSocketIdtoPlayerId = new Dictionary<string, string>();
            //离线玩家的按时间排序列表
            m_offlineRecordList = new List<OfflinePlayerRecord>();
            //大厅房间列表JSON缓存
            m_roomListJsonData = "[]";
            //初始化计时器
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
                //启动数据库操作者
                m_centerDBAgent.Start();
            }
            catch (Exception ex) { LogHelper.LogError(ex.Message + "|" + ex.StackTrace); }
        }
        private void Finish()
        {
            try
            {
                //释放所有房间
                StopHallRooms();
                //释放数据库操作者
                m_centerDBAgent.Stop();
            }
            catch (Exception ex) { LogHelper.LogError(ex.Message + "|" + ex.StackTrace); }         
        }
        private void Run()
        {
            Stopwatch stopwatch = new Stopwatch();
            Begin();
            while (!m_loopThreadExit)
            {
                stopwatch.Restart();
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
                Thread.Sleep(1);
                LogicUpdate(stopwatch.ElapsedMilliseconds);
            }
            Finish();
        }
        #endregion


        #region 计时器
        private void InitTimer()
        {
            m_updateTimerSet = new Dictionary<string, long>();
            m_updateTimerSet.Add("RoomListUpdate", 0);
            m_updateTimerSet.Add("OfflinePlayerDispose", 0);
        }
        private void LogicUpdate(long milliseceonds)
        {
            try
            {
                string[] keys = m_updateTimerSet.Keys.ToArray();
                foreach (string key in keys)
                    m_updateTimerSet[key] += milliseceonds;

                //间隔一秒更新房间列表
                if (m_updateTimerSet["RoomListUpdate"] > 1000)
                {
                    m_updateTimerSet["RoomListUpdate"] = 0;
                    UpdateRoomList();
                }

                //间隔一秒处理掉线超时的玩家
                if (m_updateTimerSet["OfflinePlayerDispose"] > 1000)
                {
                    m_updateTimerSet["OfflinePlayerDispose"] = 0;
                    DisposeOfflinePlayer();
                }

            }
            catch (Exception ex) { LogHelper.LogError(ex.Message + "|" + ex.StackTrace); }
            
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
                //输入格式校验
                Regex playerId_reg = new Regex(@"^[a-zA-Z0-9]{8,20}$");  //8-20位的数字/字母大小写
                Regex password_reg = new Regex(@"^.{8,20}$"); //8-20位密码
                if (!playerId_reg.IsMatch(playerId))
                    throw new InfoException("用户名格式错误");
                if (!password_reg.IsMatch(password))
                    throw new InfoException("密码格式错误");
                //写入数据库
                m_centerDBAgent.PlayerRegister(playerId, password);
                flag = true;
            }
            catch (InfoException ex)
            {
                CenterUserTip(socketId, ex.Message);
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
                //1检查输入是否正确
                m_centerDBAgent.PlayerLogin(playerId, password);
                //2检查该Socket是否已经登录账户
                if (m_mapperSocketIdtoPlayerId.ContainsKey(socketId))
                    throw new InfoException("你已经登录一个账户");
                //3检查该用户是否已被Socket登录
                CenterPlayer player = null;
                m_playerSet.TryGetValue(playerId, out player);
                if (player != null)
                    if (player.SocketId != null)
                        throw new InfoException("该玩家已登录");
                flag = true;
            }
            catch (InfoException ex)
            {
                CenterUserTip(socketId, ex.Message);
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
                    PlayerInfo info = m_centerDBAgent.GetPlayerInfo(playerId);
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
            eventArgs.Type = GameClientAgent.QueueEventArgs.MessageType.Server_Client;
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
                eventArgs.Type = GameClientAgent.QueueEventArgs.MessageType.Server_Client;
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
                        PlayerCreateRoom(player, jsonObj.GetValue("GameId").ToString(), jsonObj.GetValue("Caption").ToString(), jsonObj.GetValue("Password").ToString());
                        break;
                    case "JoinRoom":
                        PlayerJoinRoom(player, jsonObj.GetValue("RoomId").ToString(), jsonObj.GetValue("Password").ToString());
                        break;
                    case "LeaveRoom":
                        PlayerLeaveRoom(player);
                        break;
                    case "RequestHallInfo":
                        PlayerRequestHallInfo(player);
                        break;
                    case "RequestPlayerInfo":
                        PlayerRequestPlayerInfo(player);
                        break;
                    case "ChangePlayerInfo":
                        PlayerChangePlayerInfo(player, jsonObj.GetValue("Name").ToString(), (int)jsonObj.GetValue("HeadNo"));
                        break;
                    case "HallChat":
                        PlayerHallChat(player, jsonObj.GetValue("Chat").ToString());
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
        private void UpdateRoomList()
        {
            JArray jsonArray = new JArray();
            foreach (CenterRoom room in m_roomSet.Values)
            {
                JObject jobj = new JObject();
                jobj.Add("RoomId", room.RoomId);
                jobj.Add("RoomCaption", room.RoomCaption);
                jobj.Add("GameId", room.GameId);
                jobj.Add("RoomStatus", (int)room.RoomStatus);
                jobj.Add("PlayerCount", room.PlayerCount);
                jobj.Add("MaxPlayerCount", room.MaxPlayerCount);
                jobj.Add("HasPassword", !String.IsNullOrEmpty(room.RoomPassword));
                jsonArray.Add(jobj);
            }
            m_roomListJsonData = jsonArray.ToString();
        }
        private void DisposeOfflinePlayer()
        {
            if (m_offlineRecordList.Count > 0)
            {
                m_offlineRecordList.Sort((obj1, obj2) =>
                {
                    return obj1.CompareTo(obj2);
                });
                OfflinePlayerRecord record = m_offlineRecordList.First();
                while (record.dateTime < DateTime.Now && m_offlineRecordList.Count > 0)
                {
                    //将其踢出服务器
                    string playerId = record.playerId;
                    CenterPlayer player = null;
                    m_playerSet.TryGetValue(playerId, out player);
                    if (player != null)
                    {
                        this.PlayerLeaveRoom(player);
                    }
                    //移除该项并选取下一个
                    m_offlineRecordList.RemoveAt(0);
                    if (m_offlineRecordList.Count > 0)
                        record = m_offlineRecordList.First();
                }
            }
        }
        private void PlayerOnline(CenterPlayer player)
        {
        }
        private void PlayerOffline(CenterPlayer player)
        {
            //通知房间
            if (player.InRoomId != null)
            {
                CenterRoom room = null;
                m_roomSet.TryGetValue(player.InRoomId, out room);
                if (room != null)
                {
                    room.PlayerOffline(player.PlayerId);
                }
            }
            //加入离线玩家列表
            //HARDCODE 10分钟后剔除玩家
            DateTime deadline = DateTime.Now.AddMinutes(10);
            m_offlineRecordList.Add(new OfflinePlayerRecord(deadline, player.PlayerId));
        }
        private void PlayerReconnect(CenterPlayer player)
        {
            //通知房间
            if (player.InRoomId != null)
            {
                CenterRoom room = null;
                m_roomSet.TryGetValue(player.InRoomId, out room);
                if (room != null)
                {
                    room.PlayerReConnect(player.PlayerId, player.SocketId);
                }
            }
            //将玩家从离线列表移除
            m_offlineRecordList.RemoveAll((obj) =>
            {
                if (obj.playerId.Equals(player.PlayerId))
                    return true;
                return false;
            });
        }
        private void PlayerCreateRoom(CenterPlayer player, string gameId, string roomCaption, string roomPassword)
        {
            if (player.InRoomId != null)
            {
                HallUserTip(player, "已经在一个房间中");
                return;
            }
            {
                Regex roomTitle_reg = new Regex(@"^.{0,16}$");  //3-16位任意
                if (!roomTitle_reg.IsMatch(roomCaption))
                {
                    HallUserTip(player, "房间名格式错误");
                    return;
                }
            }
            {
                Regex roomPassword_reg = new Regex(@"^.{0,16}$");  //0-16位任意
                if (!roomPassword_reg.IsMatch(roomPassword))
                {
                    HallUserTip(player, "房间密码格式错误");
                    return;
                }
            }

            {
                //1开启房间
                string roomId = SecurityHelper.CreateGuid();
                while (m_roomSet.ContainsKey(roomId))
                    roomId = SecurityHelper.CreateGuid();
                CenterRoom room = new CenterRoom(m_serverContainer, roomId, gameId, roomCaption, roomPassword);
                room.StartGame();
                room.PlayerJoin(player.PlayerId, player.SocketId, player.Info);
                m_roomSet[roomId] = room;
                //2更新玩家所在房间
                player.InRoomId = room.RoomId;
                //3向客户端返回信息
                JObject jsonObj = new JObject();
                jsonObj.Add("Action", "InRoom");
                JObject content = new JObject();
                content.Add("RoomId", room.RoomId);
                content.Add("GameId", room.GameId);
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
            m_roomSet.TryGetValue(roomId, out room);
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
                //1通知房间
                room.PlayerJoin(player.PlayerId, player.SocketId, player.Info);
                //2更新玩家所在房间
                player.InRoomId = room.RoomId;
                //3向客户端返回信息
                JObject jsonObj = new JObject();
                jsonObj.Add("Action", "InRoom");
                JObject content = new JObject();
                content.Add("RoomId", room.RoomId);
                content.Add("GameId", room.GameId);
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
                //当房间为空时 销毁该房间
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
            //HARDCODE 截断过长的字符串
            if (chat.Length > 255)
                chat = chat.Substring(0, 255);
            JObject jsonObj = new JObject();
            jsonObj.Add("Action", "HallChat");
            JObject content = new JObject();
            content.Add("Chat", chat);
            content.Add("Sender", player.Info.Name);
            jsonObj.Add("Content", content);
            HallBroadcast(jsonObj.ToString());
        }
        private void PlayerRequestPlayerInfo(CenterPlayer player)
        {
            //构建JSON并返回
            JObject jsonObj = new JObject();
            jsonObj.Add("Action", "ResponsePlayerInfo");
            JObject content = new JObject();
            content.Add("Id", player.Info.Id);
            content.Add("Name", player.Info.Name);
            content.Add("Point", player.Info.Point);
            content.Add("HeadNo", player.Info.IconNo);
            jsonObj.Add("Content", content);
            HallResponse(player, jsonObj.ToString());
        }
        private void PlayerRequestHallInfo(CenterPlayer player)
        {
            //构建JSON并返回
            JObject jsonObj = new JObject();
            jsonObj.Add("Action", "ResponseHallInfo");
            JObject content = new JObject();
            content.Add("RoomList", m_roomListJsonData);
            content.Add("PlayerCount", m_playerSet.Count);
            content.Add("RoomCount", m_roomSet.Count);
            jsonObj.Add("Content", content);
            HallResponse(player, jsonObj.ToString());
        }
        private void PlayerChangePlayerInfo(CenterPlayer player, string name, int iconNo)
        {
            //获取PlayerInfo
            PlayerInfo info = player.Info;
            info.Name = name;
            info.IconNo = iconNo;
            //尝试更新到数据库
            bool flag = false;
            try
            {
                //输入格式校验
                Regex playerName_reg = new Regex(@"^.{3,12}$");  //3-12位任意
                if (!playerName_reg.IsMatch(name))
                    throw new InfoException("昵称格式错误");
                //写入数据库
                m_centerDBAgent.UpdatePlayerInfo(player.PlayerId, info);
                flag = true;
            }
            catch (InfoException ex)
            {
                HallUserTip(player, ex.Message);
            }
            //入库成功
            if (flag)
            {
                player.Info = info;
                JObject jsonObj = new JObject();
                jsonObj.Add("Action", "ChangePlayerInfoSuccess");
                JObject content = new JObject();
                content.Add("Id", player.Info.Id);
                content.Add("Name", player.Info.Name);
                content.Add("Point", player.Info.Point);
                content.Add("HeadNo", player.Info.IconNo);
                jsonObj.Add("Content", content);
                HallResponse(player, jsonObj.ToString());
            }
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
            eventArgs.Type = GameClientAgent.QueueEventArgs.MessageType.Server_Client;
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

                GameClientAgent.QueueEventArgs eventArgs = new GameClientAgent.QueueEventArgs();
                eventArgs.Type = GameClientAgent.QueueEventArgs.MessageType.Server_Client;
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


        #region 工具类
        private class OfflinePlayerRecord : IComparable<OfflinePlayerRecord>
        {
            public DateTime dateTime;
            public string playerId;

            public OfflinePlayerRecord(DateTime dateTime, string playerId)
            {
                this.dateTime = dateTime;
                this.playerId = playerId;
            }
            public int CompareTo(OfflinePlayerRecord obj)
            {
                return dateTime.CompareTo(obj.dateTime);
            }
        }
        #endregion
    }
}
