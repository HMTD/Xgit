using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class Servers
    {
        public string Path;
        SocketServer socketServer;
        public Dictionary<EndPoint, string> messageBufferDict = new Dictionary<EndPoint, string>();
        public Dictionary<EndPoint, List<string>> ListMessageBict = new Dictionary<EndPoint, List<string>>();
        public Dictionary<EndPoint, Dictionary<string, List<byte[]>>> FileDataDict = new Dictionary<EndPoint, Dictionary<string, List<byte[]>>>();
        public Servers(IPAddress IP, int Port)
        {
            socketServer = new SocketServer(IP, Port);
            socketServer.newConnect += NewConnect;
            socketServer.messages += Message;
            socketServer.Uconnect += Uconnect;
            socketServer.Start();
        }
        public void NewConnect(EndPoint endPoint)
        {
            messageBufferDict.Add(endPoint, "");
            ListMessageBict.Add(endPoint, new List<string>());
            Console.WriteLine($"新连接：{endPoint.ToString()}");
        }
        public void Message(EndPoint endPoint, string Messages)
        {
            messageBufferDict[endPoint] += Messages;
            if (messageBufferDict[endPoint].Contains("}"))
            {
                string[] messages = messageBufferDict[endPoint].Split(new char[] { '{', '}' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < messages.Length - 1; i++)
                {
                    ListMessageBict[endPoint].Add(Encoding.UTF8.GetString(Convert.FromBase64String(messages[i])));
                }
                if (messages.Last() != null && messages.Last() != "")
                {
                    ListMessageBict[endPoint].Add(Encoding.UTF8.GetString(Convert.FromBase64String(messages.Last()))); messageBufferDict[endPoint] = "";
                }
                else
                    messageBufferDict[endPoint] = messages[messages.Length - 1];
            }
            if (ListMessageBict[endPoint].Count > 0)
            {
                for (int i = 0; i < ListMessageBict[endPoint].Count; i++)
                {
                    if (ListMessageBict[endPoint][i] != null && ListMessageBict[endPoint][i] != "")
                    {
                        string[] Command = (ListMessageBict[endPoint][i]).Split(',');
                        if (Command[0] == "New")
                        {
                            FileDataDict[endPoint].Add(Command[1], new List<byte[]>());
                            Console.WriteLine($"新文件；{Command[1]}");
                        }
                        else if (Command[0] == "Data")
                        {
                            FileDataDict[endPoint][Command[1]].Add(Convert.FromBase64String(Command[2]));
                            Console.WriteLine($"({Command[1]})数据，长度：{ Convert.FromBase64String(Command[2]).Length}");
                        }
                        else if (Command[0] == "NewObject")
                        {
                            if (Directory.Exists(Path + Command[1]))
                                socketServer.Send(endPoint, Encoding.UTF8.GetBytes("{" + Convert.ToBase64String(Encoding.UTF8.GetBytes("DirIsHave," + Command[1])) + "}"));
                            FileDataDict.Add(endPoint, new Dictionary<string, List<byte[]>>());
                            Console.WriteLine("新Object");
                        }
                        else if (Command[0] == "End")
                        {
                            Console.WriteLine($"文件{{{Command[1]}}}完成传输");
                            Thread Save = new Thread(new ThreadStart(() =>
                            {
                                List<byte> buffer = new List<byte>();
                                for (int ia = 0; ia < FileDataDict[endPoint][Command[1]].Count; ia++)
                                    for (int ib = 0; ib < FileDataDict[endPoint][Command[1]][ia].Length; ib++)
                                        buffer.Add(FileDataDict[endPoint][Command[1]][ia][ib]);
                                File.WriteAllBytes(Path + Command[1], buffer.ToArray());
                                socketServer.Send(endPoint, Encoding.UTF8.GetBytes($"{{{Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Command[1]},OK"))}}}"));
                                Console.WriteLine($"文件保存完毕，位置：{Path + Command[1]}");
                            })); Save.Start();
                        }
                    }
                }
            }
        }
        public void Uconnect(EndPoint endPoint)
        {
            messageBufferDict.Remove(endPoint);
        }
    }
}
















