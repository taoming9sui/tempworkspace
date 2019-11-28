using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;
using Newtonsoft.Json.Linq;
using WebSocket.Utils;

namespace WebSocket.GameServer
{
    public class GameCenter
    {
        static public GameCenter Instance = new GameCenter();

        private ConcurrentQueue<QueueEventArgs> m_eventQueue;
        private Thread m_loopThread;
        private bool m_loopThreadExit = false;
        private IDictionary<string, GamePlayer> m_playerSet;
        private IDictionary<string, GameRoom> m_roomSet;
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

        public GameCenter()
        {
        }
        #region 消息队列循环
        public void Start()
        {
            m_eventQueue = new ConcurrentQueue<QueueEventArgs>();
            m_loopThread = new Thread(Run);
            m_loopThreadExit = false;
            m_loopThread.Start();
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
                    case "Login":
                        GameClientAgent.QueueEventArgs evt = new GameClientAgent.QueueEventArgs();
                        evt.Type = GameClientAgent.QueueEventArgs.MessageType.Server_Client;
                        evt.Data = "fuck you!!";
                        evt.Param1 = socketId;
                        GameClientAgent.Instance.PushMessage(evt);
                        break;
                    case "Logout":
                        break;
                }
            }
            catch (Exception ex) { LogHelper.LogError(ex.Message + "|" + ex.StackTrace); }
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
