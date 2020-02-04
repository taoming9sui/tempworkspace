using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;
using GamePlatformServer.GameServer.ServerObjects;
using System.Diagnostics;

namespace GamePlatformServer.GameServer.GameModuels
{
    public abstract class GameModuel
    {
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

        private CenterRoom m_room;

        abstract public string GameId { get; }
        abstract public string GameName { get; }
        abstract public int MaxPlayerCount { get; }
        abstract public bool IsOpened { get; }


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
        public GameModuel(CenterRoom room)
        {
            //所在房间对象引用
            m_room = room;
        }


        #region 消息队列循环
        public void PushMessage(QueueEventArgs eventArgs)
        {
            if (m_eventQueue != null)
                m_eventQueue.Enqueue(eventArgs);
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
                        case QueueEventArgs.MessageType.Message:
                            OnPlayerMessage((string)eventArgs.Param1, eventArgs.Data);
                            break;
                        case QueueEventArgs.MessageType.Join:
                            OnPlayerJoin((string)eventArgs.Param1);
                            break;
                        case QueueEventArgs.MessageType.Leave:
                            OnPlayerLeave((string)eventArgs.Param1);
                            break;
                        case QueueEventArgs.MessageType.Connect:
                            OnPlayerConnect((string)eventArgs.Param1);
                            break;
                        case QueueEventArgs.MessageType.Disconnect:
                            OnPlayerDisconnect((string)eventArgs.Param1);
                            break;
                    }
                }
                Thread.Sleep(1);
                LogicUpdate(stopwatch.ElapsedMilliseconds);
            }
            Finish();
        }
        #endregion

        #region 内置事件
        abstract protected void OnPlayerMessage(string playerId, string msgData);
        abstract protected void OnPlayerJoin(string playerId);
        abstract protected void OnPlayerLeave(string playerId);
        abstract protected void OnPlayerConnect(string playerId);
        abstract protected void OnPlayerDisconnect(string playerId);
        abstract protected void LogicUpdate(long milliseconds);
        abstract protected void Begin();
        abstract protected void Finish();
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
