﻿using System;
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
            GameServerContainer container = new GameServerContainer();
            container.Start();
            Console.WriteLine("websocket server started at [localhost:8888]");
            Console.ReadKey(true);
            container.Stop();
            Console.WriteLine("websocket server stoped at [localhost:8888]");

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
