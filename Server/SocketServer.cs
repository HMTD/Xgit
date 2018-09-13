using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Server
{
    class SocketServer
    {
        private TcpListener tcpListener;
        private Thread threadAccept;
        Dictionary<EndPoint, Socket> esDictionary = new Dictionary<EndPoint, Socket>();
        public delegate void deleNewConnect(EndPoint endPoint);
        public deleNewConnect newConnect;
        public delegate void deleMessage(EndPoint endPoint, string Message);
        public deleMessage messages;
        public delegate void deleUconnect(EndPoint endPoint);
        public deleUconnect Uconnect;
        public SocketServer(IPAddress IP, int Port)
        {
            tcpListener = new TcpListener(IP, Port);
            Console.WriteLine("监听套接字初始化完毕");
        }
        public void Start()
        {
            Console.WriteLine("绑定地址");
            tcpListener.Start();
            Console.WriteLine("开始侦听");
            threadAccept = new Thread(new ThreadStart(() => AcceptConnect()));
            threadAccept.Start();
        }
        private void AcceptConnect()
        {
            Console.WriteLine("接受并挂起连接");
            while (true)
            {
                Socket newSocket = tcpListener.AcceptSocket();
                esDictionary.Add(newSocket.RemoteEndPoint, newSocket);
                newConnect(newSocket.RemoteEndPoint);
                Thread newThread = new Thread(new ThreadStart(() => Recv(newSocket.RemoteEndPoint)));
                newThread.Start();
            }
        }
        private void Recv(EndPoint endPoint)
        {
            byte[] buffer = new byte[65535];
            while (true)
            {
                int length = -1;
                try
                {
                    length = esDictionary[endPoint].Receive(buffer);
                }
                catch (Exception e)
                {
                    esDictionary.Remove(endPoint);
                    Uconnect(endPoint);
                    return;
                }
                if (length < 1)
                {
                    esDictionary.Remove(endPoint);
                    Uconnect(endPoint);
                    return;
                }
                else
                {
                    messages(endPoint, Encoding.UTF8.GetString(buffer, 0, length));
                }
            }
        }
        public void Send(EndPoint endPoint, byte[] message)
        {
            esDictionary[endPoint].Send(message);
        }
        public void Stop()
        {
            tcpListener.Stop();
            threadAccept.Abort();
            Parallel.For(0, esDictionary.Count, (int i) =>
              {
                  esDictionary.Values.ToArray()[i].Close();
              });

        }
    }
}