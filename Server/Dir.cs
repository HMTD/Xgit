using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Server
{
    class Dir
    {
        public List<string> FileList = new List<string>();
        public List<string> DirList = new List<string>();
        public string Dirs = "";
        public Dir(string Path)
        {
            Path = Path.Replace("\\\\", "\\");
            if (Path.Last() != '\\')
                Path += "\\";
            ReadDir(Path);
            foreach (string Var in DirList)
                Dirs += "{" + Var + "}";
        }
        public void ReadDir(string Path)
        {
            try
            {
                string[] DirBuffer = Directory.GetFileSystemEntries(Path);
                Parallel.For(0, DirBuffer.Length, (int i) =>
                {
                    if (File.Exists(DirBuffer[i]))
                        FileList.Add(DirBuffer[i]);
                    else
                    {
                        DirList.Add(DirBuffer[i]);
                        ReadDir(DirBuffer[i]);
                    }
                });
            }
            catch { }
        }
    }
}
