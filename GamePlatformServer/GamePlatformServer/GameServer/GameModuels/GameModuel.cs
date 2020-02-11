using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;
using GamePlatformServer.GameServer.ServerObjects;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

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

        private GameServerContainer m_serverContainer;
        private IDictionary<string, string> m_socketIdMapper;


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
        public GameModuel(GameServerContainer container)
        {
            //容器引用
            m_serverContainer = container;
            //SocketId映射记录
            m_socketIdMapper = new Dictionary<string, string>();
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
                            PlayerJoin(eventArgs.Data, (string)eventArgs.Param1, (PlayerInfo)eventArgs.Param2);
                            OnPlayerJoin((string)eventArgs.Param1, (PlayerInfo)eventArgs.Param2);
                            break;
                        case QueueEventArgs.MessageType.Leave:
                            PlayerLeave((string)eventArgs.Param1);
                            OnPlayerLeave((string)eventArgs.Param1);
                            break;
                        case QueueEventArgs.MessageType.Connect:
                            PlayerReconnect(eventArgs.Data, (string)eventArgs.Param1);
                            OnPlayerReconnect((string)eventArgs.Param1);
                            break;
                        case QueueEventArgs.MessageType.Disconnect:
                            PlayerDisconnect((string)eventArgs.Param1);
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

        #region 内部方法
        private void PlayerJoin(string socketId, string playerId, PlayerInfo info)
        {
            m_socketIdMapper[playerId] = socketId;
        }
        private void PlayerLeave(string playerId)
        {
            m_socketIdMapper.Remove(playerId);
        }
        private void PlayerReconnect(string socketId, string playerId)
        {
            m_socketIdMapper[playerId] = socketId;
        }
        private void PlayerDisconnect(string playerId)
        {
            m_socketIdMapper.Remove(playerId);
        }
        #endregion

        #region 内置事件
        abstract protected void OnPlayerMessage(string playerId, string msgData);
        abstract protected void OnPlayerJoin(string playerId, PlayerInfo info);
        abstract protected void OnPlayerLeave(string playerId);
        abstract protected void OnPlayerReconnect(string playerId);
        abstract protected void OnPlayerDisconnect(string playerId);
        abstract protected void LogicUpdate(long milliseconds);
        abstract protected void Begin();
        abstract protected void Finish();
        #endregion


        #region 可供调用接口
        protected void SendMessage(string playerId, string data)
        {
            string socketId = "";
            if (m_socketIdMapper.TryGetValue(playerId, out socketId))
            {
                GameClientAgent.QueueEventArgs eventArgs = new GameClientAgent.QueueEventArgs();
                JObject jsonObj = new JObject();
                jsonObj.Add("Type", "Server_Room");
                jsonObj.Add("Data", JObject.Parse(data));
                eventArgs.Type = GameClientAgent.QueueEventArgs.MessageType.Server_Client;
                eventArgs.Data = jsonObj.ToString();
                eventArgs.Param1 = socketId;
                m_serverContainer.ClientAgent.PushMessage(eventArgs);
            }
        }
        protected void BroadMessage(string data)
        {
            foreach(string socketId in m_socketIdMapper.Values)
            {
                GameClientAgent.QueueEventArgs eventArgs = new GameClientAgent.QueueEventArgs();
                JObject jsonObj = new JObject();
                jsonObj.Add("Type", "Server_Room");
                jsonObj.Add("Data", JObject.Parse(data));
                eventArgs.Type = GameClientAgent.QueueEventArgs.MessageType.Server_Client;
                eventArgs.Data = jsonObj.ToString();
                eventArgs.Param1 = socketId;
                m_serverContainer.ClientAgent.PushMessage(eventArgs);
            }
        }
        #endregion


    }
}
