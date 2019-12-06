using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;
using WebSocket.GameServer.ServerObjects;

namespace WebSocket.GameServer.GameModuels
{
    public abstract class GameModuel
    {
        abstract public string GameName { get; }
        abstract public int MaxPlayerCount { get ; }
        abstract public bool IsOpened { get; }

        protected IDictionary<string, PlayerInfo> m_playerSet;
        private CenterRoom m_room;

        public GameModuel(CenterRoom room)
        {
            m_room = room;
            m_playerSet = new Dictionary<string, PlayerInfo>();

            m_eventQueue = new ConcurrentQueue<QueueEventArgs>();
            m_loopThread = new Thread(Run);
        }

        /// <summary>
        /// 队列消息类
        /// </summary>
        public class QueueEventArgs : EventArgs
        {
            public enum MessageType { None, Message, Join, Leave, Connect, Disconnect };
            public MessageType Type { get; set; }
            public string Data { get; set; }
            public Object Param1;
            public Object Param2;
        }

        private ConcurrentQueue<QueueEventArgs> m_eventQueue;
        private Thread m_loopThread;
        private bool m_loopThreadExit = false;

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
        /// 游戏逻辑主循环
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
                        case QueueEventArgs.MessageType.Message:
                            break;
                        case QueueEventArgs.MessageType.Join:
                            break;
                        case QueueEventArgs.MessageType.Leave:
                            break;
                        case QueueEventArgs.MessageType.Connect:
                            break;
                        case QueueEventArgs.MessageType.Disconnect:
                            break;
                    }
                }
                LogicUpdate();
                Thread.Sleep(1);
            }
        }
        #endregion

        #region 内置事件
        abstract protected void OnPlayerMessage();
        abstract protected void OnPlayerJoin();
        abstract protected void OnPlayerLeave();
        abstract protected void OnPlayerConnect();
        abstract protected void OnPlayerDisconnect();
        abstract protected void LogicUpdate();
        #endregion

        #region 可供调用接口
        protected void SendMessage(string playerId, string data)
        {
            m_room.GameMessageResponse(playerId, data);
        }
        protected void BroadMessage(string data)
        {
            m_room.GameMessageBroad(data);
        }
        #endregion


    }
}
