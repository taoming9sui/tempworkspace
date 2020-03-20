using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GamePlatformServer.GameServer.ServerObjects;
using GamePlatformServer.GameServer.GameModuels;
using GamePlatformServer.Utils;

namespace GamePlatformServer.GameServer
{
    public class GameServerContainer
    {
        private int m_socketPort;
        private string m_socketPath;
        private string m_sqliteConnStr;
        private GameCenter m_center;
        private GameClientAgent m_clientAgent;


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
            try
            {
                //初始化实例
                LogHelper.LogInfo("初始化GameCenter……");
                m_center = new GameCenter(this, m_sqliteConnStr);
                LogHelper.LogInfo(string.Format("初始化GameClientAgent在{0}", m_socketPort));
                m_clientAgent = new GameClientAgent(this, m_socketPort, m_socketPath);
                //启动实例
                LogHelper.LogInfo("启动GameCenter……");
                m_center.Start();
                LogHelper.LogInfo("启动GameClientAgent……");
                m_clientAgent.Start();
                //成功
                LogHelper.LogInfo("服务器启动完成！");
            }
            catch(Exception ex)
            {
                //失败
                LogHelper.LogError(ex.Message + "|" + ex.StackTrace);
                LogHelper.LogInfo("服务器启动失败:" + ex.Message);
            }
        }

        public void Stop()
        {
            try
            {
                //停止实例
                LogHelper.LogInfo("停止GameClientAgent……");
                if (m_clientAgent != null)
                    m_clientAgent.Stop();
                LogHelper.LogInfo("停止GameCenter……");
                if (m_center != null)
                    m_center.Stop();
                //成功
                LogHelper.LogInfo("服务器已关闭!");
            }
            catch (Exception ex)
            {
                LogHelper.LogError(ex.Message + "|" + ex.StackTrace);
                LogHelper.LogInfo("服务器已关闭:" + ex.Message);
            }

        }

    }
}
