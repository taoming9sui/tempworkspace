using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSocketSharp.Server;

namespace WebSocket.GameServer
{
    public class GameServerContainer
    {
        private GameCenter m_center;
        private GameClientAgent m_clientAgent;
        private WebSocketServer m_socketServer;

        internal GameCenter Center { get { return m_center; } }
        internal GameClientAgent ClientAgent { get { return m_clientAgent; } }

        public GameServerContainer()
        {

        }

        public void Start()
        {
            //开启GameCenter
            m_center = new GameCenter(this);
            m_center.Start();
            //开启GameClientAgent
            m_clientAgent = new GameClientAgent(this);
            m_clientAgent.Start();
            //开启WebSocket监听
            m_socketServer = new WebSocketServer("ws://localhost:8888");
            m_socketServer.AddWebSocketService("/Fuck", new Func<PlayerSocket>(this.GetSocket));
            m_socketServer.Start();

        }

        public void Stop()
        {
            //关闭WebSocket监听
            m_socketServer.Stop();
            //关闭GameClientAgent
            m_clientAgent.Stop();
            //关闭GameCenter
            m_center.Stop();

        }

        private PlayerSocket GetSocket()
        {
            PlayerSocket socket = new PlayerSocket();
            socket.m_serverContainer = this;
            return socket;
        }


    }
}
