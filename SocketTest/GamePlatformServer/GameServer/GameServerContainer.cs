using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSocketSharp.Server;
using GamePlatformServer.GameServer.ServerObjects;
using GamePlatformServer.GameServer.GameModuels;

namespace GamePlatformServer.GameServer
{
    public class GameServerContainer
    {
        private int m_socketPort;
        private string m_socketPath;
        private string m_sqliteConnStr;
        private GameCenter m_center;
        private GameClientAgent m_clientAgent;
        private WebSocketServer m_socketServer;


        public GameCenter Center { get { return m_center; } }
        public GameClientAgent ClientAgent { get { return m_clientAgent; } }

        public GameServerContainer(int port, string path, string connStr)
        {
            m_socketPort = port;
            m_socketPath = path;
            m_sqliteConnStr = connStr;
        }

        public void Start()
        {
            //开启GameCenter
            m_center = new GameCenter(this, m_sqliteConnStr);
            m_center.Start();
            //开启GameClientAgent
            m_clientAgent = new GameClientAgent(this);
            m_clientAgent.Start();
            //开启WebSocket监听
            m_socketServer = new WebSocketServer(String.Format("ws://localhost:{0}", m_socketPort.ToString()));
            m_socketServer.AddWebSocketService(m_socketPath, new Func<PlayerSocket>(this.GetSocket));
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
