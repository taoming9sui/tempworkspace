using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace WebSocket
{
    public class GameCenter
    {

        static public GameCenter Instance = new GameCenter();

        private ConcurrentQueue<CenterQueueEventArgs> m_eventQueue;
        private Thread m_loopThread;
        private bool m_loopThreadExit = false;
        private IDictionary<string, GameSocket> m_socketSet;
        private IDictionary<string, GamePlayer> m_playerSet;
        private IDictionary<string, GameRoom> m_roomSet;
        private IDictionary<string, string> m_mapperSocketIdtoPlayerId;

        #region 线程主控制
        public GameCenter()
        {
            Init();
        }

        private void Init()
        {
            m_eventQueue = new ConcurrentQueue<CenterQueueEventArgs>();
            m_loopThread = new Thread(Run);

        }

        public void Start()
        {
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
        public void PushSocketMessage(CenterQueueEventArgs eventArgs)
        {
            m_eventQueue.Enqueue(eventArgs);
        }

        /// <summary>
        /// 服务器逻辑主循环
        /// </summary>
        private void Run()
        {
            while (!m_loopThreadExit)
            {
                CenterQueueEventArgs eventArgs;
                if (m_eventQueue.TryDequeue(out eventArgs))
                {
                    switch (eventArgs.Type)
                    {
                        case CenterQueueEventArgs.MessageType.Client_Center:
                            this.OnClient_Center(eventArgs.Data, eventArgs.Socket);
                            break;
                        case CenterQueueEventArgs.MessageType.Client_Hall:
                            this.OnClient_Hall(eventArgs.Data, eventArgs.Socket);
                            break;
                        case CenterQueueEventArgs.MessageType.Client_Room:
                            this.OnClient_Room(eventArgs.Data, eventArgs.Socket);
                            break;
                    }
                }
                Thread.Sleep(1);
            }
        }
        #endregion
     

        #region 游戏平台工作
        private void OnClient_Center(string data, GameSocket socket)
        {
            
            try
            {
                JObject jsonObj = JObject.Parse(data);
                string action = jsonObj.GetValue("Action").ToString();
                switch (action)
                {
                    case "Connect":
                        break;
                    case "Disconnect":
                        break;
                }
            }
            catch { }
        }
        private void SocketConnect(GameSocket socket)
        {
            string id = socket.ID;
            if(!m_socketSet.ContainsKey(id))
                m_socketSet[id] = socket;
        }
        private void SocketDisconnect(GameSocket socket)
        {
            string id = socket.ID;
            if (m_socketSet.ContainsKey(id))
                m_socketSet.Remove(id);
        }
        #endregion
        #region 大厅工作
        private void OnClient_Hall(string data, GameSocket socket)
        {
            try
            {
            }
            catch { }
        }
        #endregion

        #region 转发至房间
        private void OnClient_Room(string data, GameSocket socket)
        {
            try
            {
            }
            catch { }
        }
        #endregion



    }
}
