using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Threading;
using GamePlatformServer.Utils;
using WebSocketSharp.Server;

namespace GamePlatformServer.GameServer.ServerObjects
{
    public class GameClientAgent
    {
        public class QueueEventArgs : EventArgs
        {
            public enum MessageType { None, Socket_Connect, Socket_Disconnect, Socket_Message, Server_Client };

            public MessageType Type { get; set; }
            public string Data { get; set; }
            public Object Param1;
            public Object Param2;
        }

        private int m_socketPort;
        private string m_socketPath;
        private ConcurrentQueue<QueueEventArgs> m_eventQueue;
        private Thread m_loopThread;
        private bool m_loopThreadExit = false;

        private WebSocketServer m_socketServer;
        private GameServerContainer m_serverContainer; 
        private IDictionary<string, PlayerSocket> m_socketSet;

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
        public GameClientAgent(GameServerContainer container, int port, string path)
        {
            //服务容器引用
            m_serverContainer = container;
            //客户端会话集
            m_socketSet = new Dictionary<string, PlayerSocket>();
            //访问路径
            m_socketPort = port;
            //访问路径
            m_socketPath = path;
            //WebSocket监听
            m_socketServer = new WebSocketServer(String.Format("ws://0.0.0.0:{0}", m_socketPort.ToString()));
            m_socketServer.WebSocketServices.AddService<PlayerSocket>(m_socketPath, (socket) =>
            {
                socket.m_serverContainer = m_serverContainer;
            });
        }

        #region 消息队列循环
        public void PushMessage(QueueEventArgs eventArgs)
        {
            if (m_eventQueue != null)
                m_eventQueue.Enqueue(eventArgs);
        }
        private void Begin()
        {
            try
            {
                m_socketServer.Start();
            }
            catch (Exception ex) { LogHelper.LogError(ex.Message + "|" + ex.StackTrace); }
            
        }
        private void Finish()
        {
            try
            {
                m_socketServer.Stop();
            }
            catch (Exception ex) { LogHelper.LogError(ex.Message + "|" + ex.StackTrace); }
        }
        private void Run()
        {
            Begin();
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
                            this.ReceiveMessage((PlayerSocket)eventArgs.Param1, eventArgs.Data);
                            break;
                        case QueueEventArgs.MessageType.Server_Client:
                            this.SendMessage((string)eventArgs.Param1, eventArgs.Data);
                            break;      
                    }
                }
                Thread.Sleep(1);
            }
            Finish();
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
                m_serverContainer.Center.PushMessage(eventArgs);

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
                m_serverContainer.Center.PushMessage(eventArgs);

                if (m_socketSet.ContainsKey(id))
                    m_socketSet.Remove(id);
            }
            catch (Exception ex) { LogHelper.LogError(ex.Message + "|" + ex.StackTrace); }
        }
        private void ReceiveMessage(PlayerSocket socket, string jsonData)
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
                m_serverContainer.Center.PushMessage(eventArgs);
            }
            catch (Exception ex) { LogHelper.LogError(ex.Message + "|" + ex.StackTrace); }
        }
        #endregion


        #region 服务端->客户端
        private void SendMessage(string socketId, string jsonData)
        {
            try
            {
                PlayerSocket socket = this.m_socketSet[socketId];
                socket.SocketSend(jsonData);
            }
            catch (Exception ex) { LogHelper.LogError(ex.Message + "|" + ex.StackTrace); }
        }
        #endregion

    }
}
