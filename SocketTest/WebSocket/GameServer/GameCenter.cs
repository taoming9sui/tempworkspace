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

namespace WebSocket.GameServer
{
    public class GameCenter
    {
        private GameServerContainer m_serverContainer; 

        private ConcurrentQueue<QueueEventArgs> m_eventQueue;
        private Thread m_loopThread;
        private bool m_loopThreadExit = false;
        private IDictionary<string, ServerPlayer> m_playerSet;
        private IDictionary<string, ServerRoom> m_roomSet;
        private IDictionary<string, string> m_mapperSocketIdtoPlayerId;

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
            m_serverContainer = container;
            m_playerSet = new Dictionary<string, ServerPlayer>();
            m_roomSet = new Dictionary<string, ServerRoom>();

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
                        PlayerRegister(jsonObj.GetValue("PlayerId").ToString(), jsonObj.GetValue("Password").ToString());
                        break;
                    case "Login":
                        PlayerLogin(jsonObj.GetValue("PlayerId").ToString(), jsonObj.GetValue("Password").ToString(), socketId);
                        break;
                    case "Logout":

                        break;
                    case "Connect":

                        break;
                    case "Disconnect":

                        break;
                }
            }
            catch (Exception ex) { LogHelper.LogError(ex.Message + "|" + ex.StackTrace); }
        }
        private void PlayerRegister(string playerId, string password)
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
                    {
                        throw new InfoException(String.Format("用户已注册：{0}", playerId));
                    }
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
                }
                catch (InfoException ex)
                {
                    LogHelper.LogInfo(ex.Message);
                }
                catch (Exception ex)
                {
                    LogHelper.LogError(ex.Message + "|" + ex.StackTrace);
                }
            }
        }
        private void PlayerLogin(string playerId, string password, string socketId)
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
                    LogHelper.LogInfo(ex.Message);
                }
                catch (Exception ex)
                {
                    LogHelper.LogError(ex.Message + "|" + ex.StackTrace);
                }
            }

            if (login_flag)
            {
                if (m_playerSet.ContainsKey(playerId))
                {
                    //玩家留存在游戏中心
                    ServerPlayer player = m_playerSet[playerId];
                    player.SocketId = socketId;
                }
                else
                {
                    //为新登录玩家
                    ServerPlayer player = new ServerPlayer();
                    player.PlayerId = playerId;
                    player.InRoomId = null;
                    player.SocketId = socketId;
                    player.Info = new PlayerInfo();
                    m_playerSet[playerId] = player;
                }           
            }
        }
        private void PlayerLogout(string playerId, string password, string socketId)
        {
            RemovePlayer(playerId);
        }
        private void RemovePlayer(string playerId)
        {
            ServerPlayer player = null;
            m_playerSet.TryGetValue(playerId, out player);
            if (player != null)
            {
                //1 通知大厅
                PlayerLeaveRoom(player);
                //2 移除
                m_playerSet.Remove(player.PlayerId);
            }
        }
        private void CenterResponse(string socketId, string data)
        {

        }
        #endregion


        #region 大厅工作
        private void OnClient_Hall(string data, string socketId)
        {
            try
            {
            }
            catch (Exception ex) { LogHelper.LogError(ex.Message + "|" + ex.StackTrace); }
        }
        private void PlayerLeaveRoom(ServerPlayer player)
        {
            if (player.InRoomId == null)
                return;
            //1 通知房间
            ServerRoom room = null;
            m_roomSet.TryGetValue(player.InRoomId, out room);
            if (room != null)
            {

            }
            //2 移除
            player.InRoomId = null;
        }
        #endregion


        #region 转发至房间
        private void OnClient_Room(string data, string socketId)
        {
            try
            {
            }
            catch (Exception ex) { LogHelper.LogError(ex.Message + "|" + ex.StackTrace); }
        }
        #endregion


    }
}
