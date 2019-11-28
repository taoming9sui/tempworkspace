using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSocketSharp;
using WebSocketSharp.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebSocket.Utils;

namespace WebSocket.GameServer
{
    public class PlayerSocket : WebSocketBehavior
    {
        static public PlayerSocket GetSocket()
        {
            PlayerSocket socket = new PlayerSocket();
            socket.m_clientAgent = GameClientAgent.Instance;
            return socket;
        }

        private GameClientAgent m_clientAgent;

        protected override void OnMessage(MessageEventArgs e)
        {
            try
            {
                GameClientAgent.QueueEventArgs eventArgs = new GameClientAgent.QueueEventArgs();
                eventArgs.Param1 = this;
                eventArgs.Type = GameClientAgent.QueueEventArgs.MessageType.Socket_Message;
                eventArgs.Data = e.Data;
                m_clientAgent.PushMessage(eventArgs);
            }
            catch (Exception ex) { LogHelper.LogError(ex.Message + "|" + ex.StackTrace); }
        }

        protected override void OnClose(CloseEventArgs e)
        {
            try
            {
                GameClientAgent.QueueEventArgs eventArgs = new GameClientAgent.QueueEventArgs();
                eventArgs.Param1 = this;
                eventArgs.Type = GameClientAgent.QueueEventArgs.MessageType.Socket_Disconnect;
                m_clientAgent.PushMessage(eventArgs);
            }
            catch (Exception ex) { LogHelper.LogError(ex.Message + "|" + ex.StackTrace); }
        }

        protected override void OnOpen()
        {
            try
            {
                GameClientAgent.QueueEventArgs eventArgs = new GameClientAgent.QueueEventArgs();
                eventArgs.Param1 = this;
                eventArgs.Type = GameClientAgent.QueueEventArgs.MessageType.Socket_Connect;
                m_clientAgent.PushMessage(eventArgs);
            }
            catch (Exception ex) { LogHelper.LogError(ex.Message + "|" + ex.StackTrace); }
        }

        public void SocketSend(string data)
        {
            this.Send(data);
        }

    }

}
