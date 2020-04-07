using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSocketSharp;
using WebSocketSharp.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GamePlatformServer.Utils;

namespace GamePlatformServer.GameServer.ServerObjects
{
    public class PlayerSocket : WebSocketBehavior
    {
        public GameServerContainer m_serverContainer;
        private bool m_valid = false;

        /// <summary>
        /// 客户端发来消息 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMessage(MessageEventArgs e)
        {
            if (m_valid)
            {
                //合法的连接
                try
                {
                    GameClientAgent.QueueEventArgs eventArgs = new GameClientAgent.QueueEventArgs();
                    eventArgs.Param1 = this;
                    eventArgs.Type = GameClientAgent.QueueEventArgs.MessageType.Socket_Message;
                    eventArgs.Data = e.Data;
                    m_serverContainer.ClientAgent.PushMessage(eventArgs);
                }
                catch (Exception ex) { LogHelper.LogError(ex.Message + "|" + ex.StackTrace); }
            }
            else
            {
                //连接需要验证 发起会话
                string md5_client = e.Data;
                string sessionId = this.ID;
                string salt = DateTime.UtcNow.ToShortTimeString();
                string code = string.Format("{0}-{1}", sessionId, salt);
                string md5_server = SecurityHelper.CreateMD5(code);
                if(md5_client == md5_server)
                {
                    //合法连接
                    this.m_valid = true;
                    this.Send("Valid");
                    GameClientAgent.QueueEventArgs eventArgs = new GameClientAgent.QueueEventArgs();
                    eventArgs.Param1 = this;
                    eventArgs.Type = GameClientAgent.QueueEventArgs.MessageType.Socket_Connect;
                    m_serverContainer.ClientAgent.PushMessage(eventArgs);
                }
                else
                {
                    //非法连接 直接断开
                    this.Close();
                }
            }

        }

        /// <summary>
        /// 客户端断开连接
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClose(CloseEventArgs e)
        {
            if (m_valid)
            {
                try
                {
                    GameClientAgent.QueueEventArgs eventArgs = new GameClientAgent.QueueEventArgs();
                    eventArgs.Param1 = this;
                    eventArgs.Type = GameClientAgent.QueueEventArgs.MessageType.Socket_Disconnect;
                    m_serverContainer.ClientAgent.PushMessage(eventArgs);
                }
                catch (Exception ex) { LogHelper.LogError(ex.Message + "|" + ex.StackTrace); }
            }

        }


        /// <summary>
        /// 客户端开启连接
        /// </summary>
        protected override void OnOpen()
        {
            try
            {
                if (!m_valid)
                {
                    //需要验证 发送验证ID
                    string sessionId = this.ID;
                    this.Send(sessionId);
                }
            }
            catch (Exception ex) { LogHelper.LogError(ex.Message + "|" + ex.StackTrace); }
        }

        /// <summary>
        /// 向客户端发送消息
        /// </summary>
        /// <param name="data"></param>
        public void SocketSend(string data)
        {
            this.Send(data);
        }

    }

}
