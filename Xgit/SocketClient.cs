using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Xgit
{

    class SocketClient
    {
        Socket Client;
        public delegate void deleMessage(string Message);
        public deleMessage Messages;
        public delegate void deleUconnect();
        public deleUconnect Uconnect;
        public SocketClient()
        {
            Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
        public void Connect(EndPoint ServerAddr)
        {
            Client.Connect(ServerAddr);
            Thread thread = new Thread(new ThreadStart(() => recv()));
            thread.Start();
        }
        public void recv()
        {
            while (true)
            {
                byte[] buffer = new byte[65535];
                int length = -1; try
                {
                    length = Client.Receive(buffer);
                }
                catch
                {
                    Uconnect();
                    return;
                }
                if (length < 1)
                {
                    Uconnect();
                    return;
                }
                else
                {
                    Messages(Encoding.UTF8.GetString(buffer, 0, length));
                }
            }
        }
    }
}
