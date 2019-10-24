using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSocketSharp;
using WebSocketSharp.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WebSocket
{
    public class GameSocket : WebSocketBehavior
    {
        static public GameSocket GetSocket()
        {
            GameSocket socket = new GameSocket();
            socket.m_gameServer = GameServer.Instance;
            return socket;
        }

        private GameServer m_gameServer;

        protected override void OnMessage(MessageEventArgs e)
        {
            try
            {
                JObject jsonObj = JObject.Parse(e.Data);
                string type = jsonObj.GetValue("Type").ToString();
                string data = jsonObj.GetValue("Data").ToString();
                QueueEventArgs eventArgs = new QueueEventArgs();
                eventArgs.Socket = this;
                switch (type)
                {
                    case "Client_Server":
                        eventArgs.Type = QueueEventArgs.MessageType.Client_Server;
                        break;
                    case "Client_Hall":
                        eventArgs.Type = QueueEventArgs.MessageType.Client_Hall;
                        break;
                    case "Client_Room":
                        eventArgs.Type = QueueEventArgs.MessageType.Client_Room;
                        break;
                    default:
                        eventArgs.Type = QueueEventArgs.MessageType.None;
                        break;
                }
                eventArgs.Data = data;
                m_gameServer.PushSocketMessage(eventArgs);
            }
            catch { }
        }

    }

}
