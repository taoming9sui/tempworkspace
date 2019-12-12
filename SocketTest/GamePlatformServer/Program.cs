using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Data.SQLite;
using GamePlatformServer.Utils;
using GamePlatformServer.GameServer;

namespace GamePlatformServer
{
    public class Program
    {
        
        public static void Main(string[] args)
        {
            Test1();
        }


        private static void Test1()
        {
            int port = Int32.Parse(ConfigurationManager.AppSettings["Port"]);
            string path = ConfigurationManager.AppSettings["Path"];
            string connStr = ConfigurationManager.ConnectionStrings["SQLite"].ConnectionString;
            GameServerContainer container = new GameServerContainer(port, path, connStr);
            container.Start();
            Console.WriteLine("websocket server started");

            Console.ReadKey(true);

            container.Stop();
            Console.WriteLine("websocket server stoped");
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
