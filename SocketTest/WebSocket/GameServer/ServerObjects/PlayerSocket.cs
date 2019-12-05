using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSocketSharp;
using WebSocketSharp.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebSocket.Utils;

namespace WebSocket.GameServer.ServerObjects
{
    public class PlayerSocket : WebSocketBehavior
    {
        public GameServerContainer m_serverContainer;

        /// <summary>
        /// 客户端发来消息
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMessage(MessageEventArgs e)
        {
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

        /// <summary>
        /// 客户端断开连接
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClose(CloseEventArgs e)
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


        /// <summary>
        /// 客户端开启连接
        /// </summary>
        protected override void OnOpen()
        {
            try
            {
                GameClientAgent.QueueEventArgs eventArgs = new GameClientAgent.QueueEventArgs();
                eventArgs.Param1 = this;
                eventArgs.Type = GameClientAgent.QueueEventArgs.MessageType.Socket_Connect;
                m_serverContainer.ClientAgent.PushMessage(eventArgs);
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
