using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSocketSharp;
using WebSocketSharp.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebSocket.GameServer;

namespace WebSocket
{
    public class GameSocket : WebSocketBehavior
    {
        static public GameSocket GetSocket()
        {
            GameSocket socket = new GameSocket();
            socket.m_gameServer = GameCenter.Instance;
            return socket;
        }

        private GameCenter m_gameServer;

        protected override void OnMessage(MessageEventArgs e)
        {
            try
            {
                JObject jsonObj = JObject.Parse(e.Data);
                string type = jsonObj.GetValue("Type").ToString();
                string data = jsonObj.GetValue("Data").ToString();
                CenterQueueEventArgs eventArgs = new CenterQueueEventArgs();
                eventArgs.Socket = this;
                switch (type)
                {
                    case "Client_Center":
                        eventArgs.Type = CenterQueueEventArgs.MessageType.Client_Center;
                        break;
                    case "Client_Hall":
                        eventArgs.Type = CenterQueueEventArgs.MessageType.Client_Hall;
                        break;
                    case "Client_Room":
                        eventArgs.Type = CenterQueueEventArgs.MessageType.Client_Room;
                        break;
                    default:
                        eventArgs.Type = CenterQueueEventArgs.MessageType.None;
                        break;
                }
                eventArgs.Data = data;
                m_gameServer.PushSocketMessage(eventArgs);
            }
            catch { }
        }

        protected override void OnClose(CloseEventArgs e)
        {
            CenterQueueEventArgs eventArgs = new CenterQueueEventArgs();
            eventArgs.Type = CenterQueueEventArgs.MessageType.Client_Center;
            JObject jsonObj = new JObject();
            jsonObj.Add("Action", "Disconnect");
            eventArgs.Data = jsonObj.ToString();
            m_gameServer.PushSocketMessage(eventArgs);
        }

        protected override void OnOpen()
        {
            CenterQueueEventArgs eventArgs = new CenterQueueEventArgs();
            eventArgs.Type = CenterQueueEventArgs.MessageType.Client_Center;
            JObject jsonObj = new JObject();
            jsonObj.Add("Action", "Connect");
            eventArgs.Data = jsonObj.ToString();
            m_gameServer.PushSocketMessage(eventArgs);
        }

        public void Send(ClientSocketEventArgs eventArgs)
        {
            try
            {
                JObject jsonObj = new JObject();
                string type = "";
                string data = eventArgs.Data;
                switch (eventArgs.Type)
                {
                    case ClientSocketEventArgs.MessageType.Server_Center:
                        type = "Server_Center";
                        break;
                    case ClientSocketEventArgs.MessageType.Server_Hall:
                        type = "Server_Hall";
                        break;
                    case ClientSocketEventArgs.MessageType.Server_Room:
                        type = "Server_Room";
                        break;
                }
                jsonObj.Add("Data", eventArgs.Data);
                jsonObj.Add("Type", type);
                this.Send(jsonObj.ToString());
            }
            catch { }
        }



    }

}
