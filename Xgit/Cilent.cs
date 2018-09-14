using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;

namespace Xgit
{
    class Client
    {
        SocketClient socketClient;
        public string Path;
        public string BufferMessage;
        public Client(EndPoint ServerAddr)
        {
            socketClient.Messages += Message;
            socketClient.Uconnect += Uconnect;
            socketClient.Connect(ServerAddr);
        }
        public void Message(string Messages)
        {
            BufferMessage += Messages;
            string[] Command;
            if (BufferMessage.Contains("}"))
            {
                string[] message = BufferMessage.Split(new char[] { '{', '}' }, StringSplitOptions.RemoveEmptyEntries);
                Command = new string[message.Length];
                for (int i = 0; i < message.Length; i++)
                    Command[i] = message[i];
                if (Regex.Matches(BufferMessage, "{").Count != Regex.Matches(BufferMessage, "}").Count)
                {
                    BufferMessage = "{" + message.Last();
                }
                if (Command[0] == "FileOK")
                {
                    Console.WriteLine($"文件{{{Command[1]}}}完成传输");
                }
                else if (Command[0] == "New")
                {

                }
                else if (Command[0] == "End")
                {

                }
                else if (Command[0] == "Data")
                {
                    string FileName = Command[1];
                    string FileDataBase64 = Command[2];
                    byte[] FileData = Convert.FromBase64String(FileDataBase64);
                    File.WriteAllBytes(Path + FileName, FileData);
                }
                else if (Command[0] == "DirList")
                {
                    string[] DirList = Command[1].Split(new char[] { '{', '}' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string Var in DirList)
                        Directory.CreateDirectory(Path + "\\" + Var);
                }
            }
        }
        public void Uconnect()
        {

        }
    }
}
