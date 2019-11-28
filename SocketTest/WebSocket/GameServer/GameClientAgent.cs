using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Threading;
using WebSocket.Utils;

namespace WebSocket.GameServer
{
    public class GameClientAgent
    {
        static public GameClientAgent Instance = new GameClientAgent();

        private GameCenter m_gameCenter = GameCenter.Instance; 

        private ConcurrentQueue<QueueEventArgs> m_eventQueue;
        private Thread m_loopThread;
        private bool m_loopThreadExit = false;
        private IDictionary<string, PlayerSocket> m_socketSet;

        /// <summary>
        /// 队列消息类
        /// </summary>
        public class QueueEventArgs : EventArgs
        {
            public enum MessageType { None, Socket_Connect, Socket_Disconnect, Socket_Message, Server_Client };

            public MessageType Type { get; set; }
            public string Data { get; set; }
            public Object Param1;
            public Object Param2;
        }


        public GameClientAgent()
        {
        }
        #region 消息队列循环
        public void Start()
        {
            if (m_loopThread == null || !m_loopThread.IsAlive)
            {
                m_socketSet = new Dictionary<string, PlayerSocket>();
                m_eventQueue = new ConcurrentQueue<QueueEventArgs>();
                m_loopThread = new Thread(Run);
                m_loopThreadExit = false;
                m_loopThread.Start();
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
                        case QueueEventArgs.MessageType.Socket_Connect:
                            this.SocketConnect((PlayerSocket)eventArgs.Param1);
                            break;
                        case QueueEventArgs.MessageType.Socket_Disconnect:
                            this.SocketDisconnect((PlayerSocket)eventArgs.Param1);
                            break;
                        case QueueEventArgs.MessageType.Socket_Message:
                            this.ForwardCenter((PlayerSocket)eventArgs.Param1, eventArgs.Data);
                            break;
                        case QueueEventArgs.MessageType.Server_Client:
                            this.ForwardClient((string)eventArgs.Param1, eventArgs.Data);
                            break;      
                    }
                }
                Thread.Sleep(1);
            }
        }
        #endregion


        #region 客户端->服务端
        private void SocketConnect(PlayerSocket socket)
        {
            try
            {
                string id = socket.ID;
                GameCenter.QueueEventArgs eventArgs = new GameCenter.QueueEventArgs();
                eventArgs.Type = GameCenter.QueueEventArgs.MessageType.Client_Center;
                JObject jsonObj = new JObject();
                jsonObj.Add("Action", "Connect");
                eventArgs.Data = jsonObj.ToString();
                eventArgs.Param1 = socket.ID;
                m_gameCenter.PushMessage(eventArgs);

                if (!m_socketSet.ContainsKey(id))
                    m_socketSet[id] = socket;
            }
            catch (Exception ex) { LogHelper.LogError(ex.Message + "|" + ex.StackTrace); }
        }
        private void SocketDisconnect(PlayerSocket socket)
        {
            try
            {
                string id = socket.ID;
                GameCenter.QueueEventArgs eventArgs = new GameCenter.QueueEventArgs();
                eventArgs.Type = GameCenter.QueueEventArgs.MessageType.Client_Center;
                JObject jsonObj = new JObject();
                jsonObj.Add("Action", "Disconnect");
                eventArgs.Data = jsonObj.ToString();
                eventArgs.Param1 = socket.ID;
                m_gameCenter.PushMessage(eventArgs);

                if (m_socketSet.ContainsKey(id))
                    m_socketSet.Remove(id);
            }
            catch (Exception ex) { LogHelper.LogError(ex.Message + "|" + ex.StackTrace); }
        }
        private void ForwardCenter(PlayerSocket socket, string jsonData)
        {
            try
            {
                JObject jsonObj = JObject.Parse(jsonData);
                string type = jsonObj.GetValue("Type").ToString();
                string data = jsonObj.GetValue("Data").ToString();
                GameCenter.QueueEventArgs eventArgs = new GameCenter.QueueEventArgs();
                switch (type)
                {
                    case "Client_Center":
                        eventArgs.Type = GameCenter.QueueEventArgs.MessageType.Client_Center;
                        break;
                    case "Client_Hall":
                        eventArgs.Type = GameCenter.QueueEventArgs.MessageType.Client_Hall;
                        break;
                    case "Client_Room":
                        eventArgs.Type = GameCenter.QueueEventArgs.MessageType.Client_Room;
                        break;
                    default:
                        eventArgs.Type = GameCenter.QueueEventArgs.MessageType.None;
                        break;
                }
                eventArgs.Data = data;
                eventArgs.Param1 = socket.ID;
                m_gameCenter.PushMessage(eventArgs);
            }
            catch (Exception ex) { LogHelper.LogError(ex.Message + "|" + ex.StackTrace); }
        }
        #endregion


        #region 服务端->客户端
        private void ForwardClient(string socketId, string data)
        {
            try
            {
                PlayerSocket socket = this.m_socketSet[socketId];
                socket.SocketSend(data);
            }
            catch (Exception ex) { LogHelper.LogError(ex.Message + "|" + ex.StackTrace); }
        }
        #endregion




    }
}
