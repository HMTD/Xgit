using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace Xgit
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpClient tcpClient = new TcpClient();
            tcpClient.Connect(IPAddress.Parse("127.0.0.1"), 233);
            byte[] FileByte = File.ReadAllBytes("emm.exe");
            string[] send = new string[] { "NewObject", "New" };
            tcpClient.Client.Send(Encoding.UTF8.GetBytes("{" + Convert.ToBase64String(Encoding.UTF8.GetBytes("NewObject")) + "}"));
            tcpClient.Client.Send(Encoding.UTF8.GetBytes("{" + Convert.ToBase64String(Encoding.UTF8.GetBytes("New,emm.exe")) + "}"));
            tcpClient.Client.Send(Encoding.UTF8.GetBytes("{"+Convert.ToBase64String(Encoding.UTF8.GetBytes($"Data,emm.exe,{Convert.ToBase64String(FileByte)}"))+"}"));
            tcpClient.Client.Send(Encoding.UTF8.GetBytes("{"+Convert.ToBase64String(Encoding.UTF8.GetBytes("End,emm.exe"))+"}"));
            Console.ReadLine();
        }
    }
}
