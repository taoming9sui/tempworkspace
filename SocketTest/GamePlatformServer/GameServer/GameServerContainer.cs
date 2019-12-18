using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        private GamePlayerDBAgent m_playerDBAgent;


        public GameCenter Center { get { return m_center; } }
        public GameClientAgent ClientAgent { get { return m_clientAgent; } }
        public GamePlayerDBAgent PlayerDBAgent { get { return m_playerDBAgent; } }

        public GameServerContainer(int port, string path, string connStr)
        {
            m_socketPort = port;
            m_socketPath = path;
            m_sqliteConnStr = connStr;
        }

        public void Start()
        {
            //开启GamePlayerDBAgent
            m_playerDBAgent = new GamePlayerDBAgent(m_sqliteConnStr);
            m_playerDBAgent.Start();
            //开启GameCenter
            m_center = new GameCenter(this);
            m_center.Start();
            //开启GameClientAgent
            m_clientAgent = new GameClientAgent(this, m_socketPort, m_socketPath);
            m_clientAgent.Start();

        }

        public void Stop()
        {
            //关闭GameClientAgent
            m_clientAgent.Stop();
            //关闭GameCenter
            m_center.Stop();
            //关闭GamePlayerDBAgent
            m_playerDBAgent.Stop();

        }

    }
}
