using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;

namespace WebSocket
{
    public class GameServer
    {

        static public GameServer Instance = new GameServer();

        private ConcurrentQueue<QueueEventArgs> m_eventQueue;
        private Thread m_loopThread;
        private bool m_loopThreadExit = false;
        private IDictionary<string, GameSocket> m_socketSet;
        private IDictionary<string, GamePlayer> m_playerSet;
        private IDictionary<string, GameRoom> m_roomSet;
        private IDictionary<string, string> m_mapperSocketIdtoPlayerId;

        public GameServer()
        {
            Init();
        }

        private void Init()
        {
            m_eventQueue = new ConcurrentQueue<QueueEventArgs>();
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
        /// 接收消息事件
        /// </summary>
        /// <param name="eventArgs"></param>
        public void PushSocketMessage(QueueEventArgs eventArgs)
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
                QueueEventArgs eventArgs;
                if (m_eventQueue.TryDequeue(out eventArgs))
                {
                    switch (eventArgs.Type)
                    {
                        case QueueEventArgs.MessageType.Client_Server:
                            break;
                        case QueueEventArgs.MessageType.Client_Hall:
                            break;
                        case QueueEventArgs.MessageType.Client_Room:
                            break;
                    }
                }
            }
        }

    }
}
