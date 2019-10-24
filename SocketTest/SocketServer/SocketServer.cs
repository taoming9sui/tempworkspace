using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace SocketServer
{
    public class SocketServer
    {
        private string _ip = string.Empty;
        private int _port = 0;
        private Socket _socket = null;
        private byte[] buffer = new byte[1024 * 1024 * 2];
        private HashSet<Socket> _socketSet = new HashSet<Socket>();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ip">监听的IP</param>
        /// <param name="port">监听的端口</param>
        public SocketServer(string ip, int port)
        {
            this._ip = ip;
            this._port = port;
        }
        public SocketServer(int port)
        {
            this._ip = "0.0.0.0";
            this._port = port;
        }

        public void StartListen()
        {
            try
            {
                //1.0 实例化套接字(IP4寻找协议,流式协议,TCP协议)
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //2.0 创建IP对象
                IPAddress address = IPAddress.Parse(_ip);
                //3.0 创建网络端口,包括ip和端口
                IPEndPoint endPoint = new IPEndPoint(address, _port);
                //4.0 绑定套接字
                _socket.Bind(endPoint);
                //5.0 设置最大连接数
                _socket.Listen(int.MaxValue);
                Console.WriteLine("监听{0}消息成功", _socket.LocalEndPoint.ToString());
                //6.0 开始监听
                Thread thread = new Thread(ListenClientConnect);
                thread.Start();
            }
            catch (Exception ex)
            {
            }
        }
        /// <summary>
        /// 监听客户端连接
        /// </summary>
        private void ListenClientConnect()
        {
            try
            {
                while (true)
                {
                    //Socket创建的新连接
                    Socket clientSocket = _socket.Accept();
                    _socketSet.Add(clientSocket);
                    clientSocket.Send(Encoding.UTF8.GetBytes("服务端发送消息:"));
                    Thread thread = new Thread(ReceiveMessage);
                    thread.Start(clientSocket);
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// 接收客户端消息
        /// </summary>
        /// <param name="socket">来自客户端的socket</param>
        private void ReceiveMessage(object socket)
        {
            Socket clientSocket = (Socket)socket;
            while (true)
            {
                try
                {
                    //获取从客户端发来的数据 监听过程中会阻塞
                    int length = clientSocket.Receive(buffer);
                    Console.WriteLine("接收客户端{0},消息{1}", clientSocket.RemoteEndPoint.ToString(), Encoding.UTF8.GetString(buffer, 0, length));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    _socketSet.Remove(clientSocket);
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                    break;
                }
            }
        }
    }

}
