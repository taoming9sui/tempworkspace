using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocketServer
{
    class Program
    {
        static void Main(string[] args)
        {
            SocketServer server = new SocketServer(8888);
            server.StartListen();
            Console.ReadKey();
        }
    }
}
