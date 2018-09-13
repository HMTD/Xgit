using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Servers servers = new Servers(IPAddress.Parse("0.0.0.0"), 233);
            servers.Path = @"C:\Users\Administrator\Desktop\";
            Console.ReadLine();
        }
    }
}
