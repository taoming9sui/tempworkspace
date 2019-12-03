﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;

namespace WebSocket.GameServer.Games
{
    public class GameInstance
    {
        abstract public int MaxPlayerCount { get ; }
        protected IDictionary<string, PlayerInfo> m_playerSet;
        private ServerRoom m_room;

        public GameInstance(ServerRoom room)
        {
            m_playerSet = new Dictionary<string, PlayerInfo>();
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
                Thread.Sleep(20);
            }
        }
        #endregion

        #region 内置事件
        virtual protected void OnPlayerMessage()
        {
        }
        virtual protected void OnPlayerJoin()
        {
        }
        virtual protected void OnPlayerLeave()
        {
        }
        virtual protected void OnPlayerConnect()
        {
        }
        virtual protected void OnPlayerDisconnect()
        {
        }
        #endregion

        #region 可供调用接口

        #endregion


    }
}
