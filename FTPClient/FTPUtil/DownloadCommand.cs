using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTPUtil
{
    class DownloadCommand : ICommand
    {
        private FTP ftp;
        private String fileName;
        private String reply;

        public DownloadCommand(FTP ftp)
        {
            this.ftp = ftp;
        }

        public void Execute()
        {
            
        }

        /// <param name="cmd">目标文件名</param>
        public void Execute(string cmd)
        {
            fileName = cmd;
            ftp.Send("RETR " + fileName);
            reply = ftp.ReadControlPort();
            FileStream fs = new FileStream(ftp.LocalAddress+cmd, FileMode.Create);
            int count = 0;
            byte[] data;
            do
            {
                data = ftp.ReadDataPortAsByte(ref count);
                fs.Write(data, 0, data.Length);
            } while (count > data.Length);
            fs.Flush();
            fs.Close();
        }

        public string GetReply()
        {
            return reply;
        }
    }
}
