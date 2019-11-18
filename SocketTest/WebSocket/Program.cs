using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Data.SQLite;
using WebSocket.Utils;

namespace WebSocket
{
    public class Program
    {
        

        public static void Main(string[] args)
        {
            Test2();
        }


        private static void Test1()
        {
            GameCenter.Instance.Start();
            Console.ReadKey(true);

            var wssv = new WebSocketServer("ws://localhost:8888");
            wssv.AddWebSocketService("/Fuck", new Func<GameSocket>(GameSocket.GetSocket));
            wssv.Start();
            Console.WriteLine("websocket server started at [localhost:8888]");
            wssv.Stop();

            Console.ReadKey(true);
            GameCenter.Instance.Stop();
        }

        private static void Test2()
        {
            Console.ReadKey(true);

            string md5 = MD5Encoder.CreateMD5("奶茶");
            Console.WriteLine(md5);
            LogHelper.LogInfo(md5);

            Console.ReadKey(true);
        }

    }
}
