using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTPUtil
{
    public class DownloadCommand :TransferCommand
    {

        public DownloadCommand(FTP ftp,String source,String destination)
        {
            this.ftp = ftp;
            Source = source;
            Destination = destination;
        }

        /// <summary>
        /// 中断下载，并返回下载断点续传类
        /// </summary>
        public override ICommand Abort()
        {
            //todo
            throw new NotImplementedException();
        }

        public override void Execute()
        {
            //todo:还没有给Size赋值
            ftp.Send("SIZE " + Source);
            Size = int.Parse(ftp.ReadControlPort().Split(' ').Last());
            String fileName = Source.Split('\\').Last();
            ftp.Send("RETR " + Source);
            reply = ftp.ReadControlPort();
            FileStream fs = new FileStream(Destination+fileName, FileMode.Create);
            int count = 0;
            byte[] data;
            do
            {
                data = ftp.ReadDataPortAsByte(ref count);
                fs.Write(data, 0, data.Length);
                Point += data.Length;
            } while (count >= data.Length);
            fs.Flush();
            fs.Close();
        }
        

        public override string GetReply()
        {
            return reply;
        }
    }
}
