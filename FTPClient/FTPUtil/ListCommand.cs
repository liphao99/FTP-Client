using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FTPUtil
{
    public class ListCommand : Command
    {
        public List<List<String>> Files { get; private set; }
        public List<List<String>> Directories { get; private set; }
        private FTP ftp;
        private String fullpath;
        private String reply = null;

        /// <summary>
        /// 获取文件列表
        /// </summary>
        /// <param name="ftp"></param>
        /// <param name="fullpath">目标文件夹的绝对路径</param>
        public ListCommand(FTP ftp, string fullpath)
        {
            this.ftp = ftp;
            this.fullpath = fullpath;
            this.Files = null;
            this.Directories = null;
        }

        public void Execute()
        {
            Files = null;
            Directories = null;
            ftp.ConnectDataPortByPASV();
            ftp.Send("LIST " + fullpath);
            reply = ftp.ReadControlPort();
            String res = ftp.ReadDataPortAsString();
            ftp.CloseDataPort();
            String[] list = Regex.Split(res, "\r\n", RegexOptions.IgnoreCase);
            int count = list.Length - 1;
            Files = new List<List<String>>();
            Directories = new List<List<String>>();
            for(int i = 0;i<count;i++)
            {
                String file = list[i];
                List<String> item = new List<string>(Regex.Split(file, "\\s+", RegexOptions.IgnoreCase));
                if (item[2].Equals("<DIR>"))
                {
                    Directories.Add(item);
                }
                else
                {
                    Files.Add(item);
                }
            }
            reply = ftp.ReadControlPort();
        }

        public string GetReply()
        {
            return reply;
        }
        
    }
}
