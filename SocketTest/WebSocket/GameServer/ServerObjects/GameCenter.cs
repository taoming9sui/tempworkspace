using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;
using Newtonsoft.Json.Linq;
using WebSocket.Utils;
using System.Data.SQLite;
using WebSocket.Exceptions;
using System.Text.RegularExpressions;
using System.Data;

namespace WebSocket.GameServer.ServerObjects
{
    public class GameCenter
    {
        private ConcurrentQueue<QueueEventArgs> m_eventQueue;
        private Thread m_loopThread;
        private bool m_loopThreadExit = false;

        private GameServerContainer m_serverContainer;
        private IDictionary<string, CenterPlayer> m_playerSet;
        private IDictionary<string, CenterRoom> m_roomSet;
        private IDictionary<string, string> m_mapperSocketIdtoPlayerId;
        private IDictionary<string, int> m_updateTimerSet;
        private DataTable m_roomListInfo;

        /// <summary>
        /// 队列消息类
        /// </summary>
        public class QueueEventArgs : EventArgs
        {
            public enum MessageType { None, Client_Center, Client_Hall, Client_Room };

            public MessageType Type { get; set; }
            public string Data { get; set; }
            public Object Param1;
            public Object Param2;
        }

        public GameCenter(GameServerContainer container)
        {
            //服务容器引用
            m_serverContainer = container;
            //玩家对象集
            m_playerSet = new Dictionary<string, CenterPlayer>();
            //房间集
            m_roomSet = new Dictionary<string, CenterRoom>();
            //客户端会话id到玩家id的关系集
            m_mapperSocketIdtoPlayerId = new Dictionary<string, string>();
            //房间信息列表
            m_roomListInfo = new DataTable();
            m_roomListInfo.Columns.Add("RoomId", typeof(string));
            m_roomListInfo.Columns.Add("RoomTitle", typeof(string));
            m_roomListInfo.Columns.Add("RoomStatus", typeof(string));
            m_roomListInfo.Columns.Add("HasPassword", typeof(bool));
            m_roomListInfo.Columns.Add("MaxPlayerCount", typeof(int));
            m_roomListInfo.Columns.Add("PlayerCount", typeof(int));
            //计时器集
            m_updateTimerSet = new Dictionary<string, int>();
            m_updateTimerSet.Add("RoomListUpdate", 0);
            //消息队列进程对象
            m_eventQueue = new ConcurrentQueue<QueueEventArgs>();
            m_loopThread = new Thread(Run);    
        }

        #region 消息队列循环
        public void Start()
        {
            if (!m_loopThread.IsAlive)
            {
                m_loopThreadExit = false;
                m_loopThread.Start();
            }
            else
            {
                throw new Exception("当前部门正在运作！");
            }
        }
        public void Stop()
        {
            m_loopThreadExit = true;
        }
        /// <summary>
        /// 接收队列消息
        /// </summary>
        /// <param name="eventArgs"></param>
        public void PushMessage(QueueEventArgs eventArgs)
        {
            if (m_eventQueue != null)
            {
                m_eventQueue.Enqueue(eventArgs);
            }
            else
            {
                throw new Exception("当前部门尚未启动！");
            }
        }

        /// <summary>
        /// 服务器逻辑主循环
        /// </summary>
        private void Run()
        {
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
                HallLogicUpdate();
                Thread.Sleep(1);
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
                    case "Disconnect":
                        ClientDisconnect(socketId);
                        break;
                }
            }
            catch (Exception ex) { LogHelper.LogError(ex.Message + "|" + ex.StackTrace); }
        }
        private void ClientRegister(string playerId, string password, string socketId)
        {
            using (SQLiteConnection conn = SQLiteHelper.GetConnection())
            {
                conn.Open();
                try
                {
                    //检查输入格式
                    Regex playerId_reg = new Regex(@"^[a-zA-Z0-9]{8,20}$");  //8-20数字大小写字母
                    Regex password_reg = new Regex(@"^(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z]).{8,20}$"); //至少有一个数字 至少有一个小写字母 至少有一个大写字母 8-20位密码
                    if (!playerId_reg.IsMatch(playerId))
                        throw new InfoException("用户名格式错误");
                    if (!password_reg.IsMatch(password))
                        throw new InfoException("密码格式错误");
                    //检查用户是否重复
                    SQLiteCommand cmd1 = new SQLiteCommand(conn);
                    cmd1.CommandText = "select player_id from player_register where player_id=@p0";
                    cmd1.CommandType = System.Data.CommandType.Text;
                    cmd1.Parameters.Add(new SQLiteParameter("@p0", System.Data.DbType.String));
                    cmd1.Parameters[0].Value = playerId;
                    SQLiteDataReader reader = cmd1.ExecuteReader();
                    bool hasOne = reader.HasRows;
                    reader.Close();
                    if (hasOne)
                        throw new InfoException("该用户已被注册");
                    //填写注册表单
                    string player_id = playerId;
                    string password_salt = SecurityHelper.CreateRandomString(16);
                    string password_md5 = SecurityHelper.CreateMD5(password + password_salt);
                    long register_date = Convert.ToInt64(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds);
                    //执行入库操作
                    SQLiteCommand cmd2 = new SQLiteCommand(conn);
                    cmd2.CommandText = "insert into player_register (player_id,password_md5,password_salt,register_date) values (@p0,@p1,@p2,@p3)";
                    cmd2.CommandType = System.Data.CommandType.Text;
                    cmd2.Parameters.Add(new SQLiteParameter("@p0", System.Data.DbType.String));
                    cmd2.Parameters[0].Value = playerId;
                    cmd2.Parameters.Add(new SQLiteParameter("@p1", System.Data.DbType.String));
                    cmd2.Parameters[1].Value = password_md5;
                    cmd2.Parameters.Add(new SQLiteParameter("@p2", System.Data.DbType.String));
                    cmd2.Parameters[2].Value = password_salt;
                    cmd2.Parameters.Add(new SQLiteParameter("@p3", System.Data.DbType.Int64));
                    cmd2.Parameters[3].Value = register_date;
                    cmd2.ExecuteNonQuery();
                    //客户端返回注册成功
                    JObject jsonObj = new JObject();
                    jsonObj.Add("Action", "RegisterSuccess");
                    CenterResponse(socketId, jsonObj.ToString());
                }
                catch (InfoException ex)
                {
                    CenterUserTip(socketId, ex.Message);
                }
                catch (Exception ex)
                {
                    LogHelper.LogError(ex.Message + "|" + ex.StackTrace);
                }
            }
        }
        private void ClientLogin(string playerId, string password, string socketId)
        {
            bool login_flag = false;

            using (SQLiteConnection conn = SQLiteHelper.GetConnection())
            {
                conn.Open();
                try
                {
                    //检查用户是否存在
                    SQLiteCommand cmd1 = new SQLiteCommand(conn);
                    cmd1.CommandText = "select * from player_register where player_id=@p0";
                    cmd1.CommandType = System.Data.CommandType.Text;
                    cmd1.Parameters.Add(new SQLiteParameter("@p0", System.Data.DbType.String));
                    cmd1.Parameters[0].Value = playerId;
                    SQLiteDataReader reader = cmd1.ExecuteReader();
                    bool hasOne = reader.Read();
                    if (!hasOne)
                    {
                        reader.Close();
                        throw new InfoException("用户名密码错误");
                    }
                    string password_salt = reader["password_salt"].ToString();
                    string password_md5 = reader["password_md5"].ToString();
                    string input_md5 = SecurityHelper.CreateMD5(password + password_salt);
                    reader.Close();
                    //验证是否成功
                    if (input_md5.Equals(password_md5))
                    {
                        login_flag = true;
                    }
                    else
                    {
                        throw new InfoException("用户名密码错误");
                    }
                }
                catch (InfoException ex)
                {
                    CenterUserTip(socketId, ex.Message);
                }
                catch (Exception ex)
                {
                    LogHelper.LogError(ex.Message + "|" + ex.StackTrace);
                }
            }

            if (login_flag)
            {
                //1客户端返回登录成功
                JObject jsonObj = new JObject();
                jsonObj.Add("Action", "LoginSuccess");
                CenterResponse(socketId, jsonObj.ToString());
                //2管理玩家对象
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
                    player.Info = new PlayerInfo();
                    player.Info.Name = playerId;
                    m_playerSet[playerId] = player;
                    //通知大厅有玩家上线
                    PlayerOnline(player);
                }
            }
        }
        private void ClientLogout(string socketId)
        {
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
                }
            }
            m_mapperSocketIdtoPlayerId.Remove(socketId);
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
            foreach (string socketId in m_mapperSocketIdtoPlayerId.Keys)
            {
                GameClientAgent.QueueEventArgs eventArgs = new GameClientAgent.QueueEventArgs();
                eventArgs.Data = jsonStr;
                eventArgs.Param1 = socketId;
                m_serverContainer.ClientAgent.PushMessage(eventArgs);
            }
        }
        #endregion


        #region 大厅工作
        private void HallLogicUpdate()
        {

        }
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
                }

            }
            catch (Exception ex) { LogHelper.LogError(ex.Message + "|" + ex.StackTrace); }
        }
        private void PlayerOnline(CenterPlayer player)
        {
        }
        private void PlayerOffline(CenterPlayer player)
        {
            CenterRoom room = null;
            m_roomSet.TryGetValue(player.InRoomId, out room);
            if (room != null)
            {
                room.PlayerOffline(player.PlayerId);
            }
        }
        private void PlayerReconnect(CenterPlayer player)
        {
            CenterRoom room = null;
            m_roomSet.TryGetValue(player.InRoomId, out room);
            if (room != null)
            {
                room.PlayerReConnect(player.PlayerId);
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
                string roomId = SecurityHelper.CreateGuid();
                while (m_roomSet.ContainsKey(roomId))
                    roomId = SecurityHelper.CreateGuid();
                CenterRoom room = new CenterRoom(this, roomId, gameId, roomTitle, roomPassword);
                room.PlayerJoin(player.PlayerId, player.Info);
                player.InRoomId = room.RoomId;
                //向客户端返回信息
                JObject jsonObj = new JObject();
                jsonObj.Add("Action", "BeInRoom");
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
                room.PlayerJoin(player.PlayerId, player.Info);
                player.InRoomId = room.RoomId;
                //向客户端返回信息
                JObject jsonObj = new JObject();
                jsonObj.Add("Action", "BeInRoom");
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
                GameClientAgent.QueueEventArgs eventArgs = new GameClientAgent.QueueEventArgs();;
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
                try
                {
                    string playerId = m_mapperSocketIdtoPlayerId[socketId];
                    player = m_playerSet[playerId];
                    room = m_roomSet[player.InRoomId];
                }
                catch { }

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
        public void RoomResponse(string playerId, string data)
        {
            CenterPlayer player = null;
            m_playerSet.TryGetValue(playerId, out player);
            if (player != null)
            {
                GameClientAgent.QueueEventArgs eventArgs = new GameClientAgent.QueueEventArgs();
                JObject jsonObj = new JObject();
                jsonObj.Add("Type", "Server_Room");
                jsonObj.Add("Data", JObject.Parse(data));
                eventArgs.Data = jsonObj.ToString();
                eventArgs.Param1 = player.SocketId;
                m_serverContainer.ClientAgent.PushMessage(eventArgs);
            }
        }
        #endregion

    }
}
