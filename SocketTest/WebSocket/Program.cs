using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Data.SQLite;

namespace WebSocket
{
    public class Program
    {
        private static string DBConnStr = "data source=C:\\Users\\admin\\Desktop\\test\\CShartTest\\SocketTest\\db\\db.db;version=3;";

        public static void Main(string[] args)
        {
            Test2();
        }


        private static void Test1()
        {
            GameServer.Instance.Start();
            Console.ReadKey(true);

            var wssv = new WebSocketServer("ws://localhost:8888");
            wssv.AddWebSocketService("/Fuck", new Func<GameSocket>(GameSocket.GetSocket));
            wssv.Start();
            Console.WriteLine("websocket server started at [localhost:8888]");
            wssv.Stop();

            Console.ReadKey(true);
            GameServer.Instance.Stop();
        }

        private static void Test2()
        {
            Console.ReadKey(true);

            SQLiteConnection conn = new SQLiteConnection(DBConnStr);
            conn.Open();
            conn.Close();


            Console.ReadKey(true);
        }

    }
}
