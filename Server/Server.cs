using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
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
                if (Regex.Matches(messageBufferDict[endPoint], "{").Count != Regex.Matches(messageBufferDict[endPoint], "}").Count)
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
                            Console.WriteLine($"{{{endPoint}}} 请求:{ListMessageBict[endPoint][i]}");
                            FileDataDict[endPoint].Add(Command[1], new List<byte[]>());
                            Console.WriteLine($"新文件；{Command[1]}");
                        }
                        else if (Command[0] == "Data")
                        {
                            byte[] FileByte = Convert.FromBase64String(Command[2]);
                            Console.WriteLine($"{{{endPoint.ToString()}}} 传输数据数据,所属文件:{Command[1]},长度:{FileByte.Length}");
                            FileDataDict[endPoint][Command[1]].Add(FileByte);
                        }
                        else if (Command[0] == "NewObject")
                        {
                            Console.WriteLine($"{{{endPoint.ToString()}}} 创建新传输:{Command[1]}");
                            if (Directory.Exists(Path + Command[1]))
                            {
                                Console.WriteLine($"报错：{Command[1]} 已经存在");
                                socketServer.Send(endPoint, Encoding.UTF8.GetBytes("{" + Convert.ToBase64String(Encoding.UTF8.GetBytes("DirIsHave," + Command[1])) + "}"));
                            }
                            else
                            {
                                FileDataDict.Add(endPoint, new Dictionary<string, List<byte[]>>());
                                Console.WriteLine($"新Object：{Command[1]}");
                            }
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
                                socketServer.Send(endPoint, Encoding.UTF8.GetBytes($"{{{Convert.ToBase64String(Encoding.UTF8.GetBytes($"FileOK,{Command[1]}"))}}}"));
                                Console.WriteLine($"文件保存完毕，位置：{Path + Command[1]}");
                            })); Save.Start();
                        }
                        else if (Command[0] == "Get")
                        {
                            Console.WriteLine($"{{{endPoint.ToString()}}}收到下载文件请求");
                            Thread Send = new Thread(new ThreadStart(() =>
                            {
                                Dir dir = new Dir(Path + "\\git");
                                socketServer.Send(endPoint, Encoding.UTF8.GetBytes("{" + Convert.ToBase64String(Encoding.UTF8.GetBytes($"DirList,{dir.Dirs}")) + "}"));
                                foreach (string Var in dir.FileList)
                                {
                                    byte[] ReadFile = File.ReadAllBytes(Var);
                                    string base64 = Convert.ToBase64String(ReadFile);
                                    string sendstring = "Data," + Var.Replace(Path + "\\git", "") + "," + base64;
                                    string sendBase64 = "{" + Convert.ToBase64String(Encoding.UTF8.GetBytes(sendstring)) + "}";
                                    socketServer.Send(endPoint, Encoding.UTF8.GetBytes("{" + Convert.ToBase64String(Encoding.UTF8.GetBytes($"New,{Var.Replace(Path + "\\git", "")}")) + "}"));
                                    socketServer.Send(endPoint, Encoding.UTF8.GetBytes(sendBase64));
                                    socketServer.Send(endPoint, Encoding.UTF8.GetBytes("{" + Convert.ToBase64String(Encoding.UTF8.GetBytes($"End,{Var.Replace(Path + "\\git", "")}")) + "}"));
                                }
                            }));
                            Send.Start();
                        }
                        else if (Command[0] == "Select")
                        {
                            string[] DirData = Directory.GetDirectories(Path);
                            string DataString = "";
                            foreach (string Var in DirData)
                            {
                                DataString += "{" + Var.Replace(Path, "") + "}";
                            }
                            socketServer.Send(endPoint, Encoding.UTF8.GetBytes("{" + Convert.ToBase64String(Encoding.UTF8.GetBytes(DataString)) + "}"));
                        }
                    }
                    ListMessageBict[endPoint].Remove(ListMessageBict[endPoint][i]);
                }
            }
        }
        public void Uconnect(EndPoint endPoint)
        {
            messageBufferDict.Remove(endPoint);
        }
    }
}
















