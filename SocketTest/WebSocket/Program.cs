using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Data.SQLite;
using WebSocket.Utils;
using WebSocket.GameServer;

namespace WebSocket
{
    public class Program
    {
        

        public static void Main(string[] args)
        {
            Test1();
        }


        private static void Test1()
        {     
            Console.ReadKey(true);

            GameCenter.Instance.Start();
            GameClientAgent.Instance.Start();
            var wssv = new WebSocketServer("ws://localhost:8888");
            wssv.AddWebSocketService("/Fuck", new Func<PlayerSocket>(PlayerSocket.GetSocket));
            wssv.Start();
            Console.WriteLine("websocket server started at [localhost:8888]");


            Console.ReadKey(true);
            wssv.Stop();
            GameClientAgent.Instance.Stop();
            GameCenter.Instance.Stop();

        }

        private static void Test2()
        {
            Console.ReadKey(true);

            string md5 = SecurityHelper.CreateMD5("奶茶");
            Console.WriteLine(md5);
            LogHelper.LogError(md5);

            Console.ReadKey(true);
        }

    }
}
