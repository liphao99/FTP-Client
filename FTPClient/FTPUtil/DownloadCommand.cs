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
        private Object lockObj = new object();

        /// <param name="ftp">FTP链接</param>
        /// <param name="source">需要下载的文件的绝对路径</param>
        /// <param name="destination">将文件下载到哪个目录下，不包含文件名，如:"D:\\Download\"</param>
        public DownloadCommand(FTP ftp,String source,String destination)
        {
            this.ftp = ftp;
            Source = source;
            Destination = destination;
        }

        /// <summary>
        /// 中断下载，并返回下载断点续传类
        /// </summary>
        public override Command Abort()
        {
            lock (lockObj)
            {
                ftp.CloseDataPort();
                ftp.Send("ABOR");
                reply = ftp.ReadControlPort();
            }
            int breakpoint = Point;
            return null;//return new DownloadContinue(ftp,source,destination,breakpoint);
        }

        /// <summary>
        /// 获取下载文件的大小，并开始数据传输
        /// </summary>
        public override void Execute()
        {
            String fileName;
            lock (lockObj)
            {
                ftp.Send("SIZE " + Source);
                String num = ftp.ReadControlPort().Split(' ').Last();
                Size = int.Parse(num);
                fileName = Source.Split('\\').Last();
                ftp.ConnectDataPortByPASV();
                ftp.Send("RETR " + Source);
                reply = ftp.ReadControlPort();
            }
            FileStream fs = new FileStream(Destination+fileName, FileMode.Create);
            int count = 0;
            byte[] data;
            do
            {
                data = ftp.ReadDataPort(ref count);
                fs.Write(data, 0, data.Length);
                Point += data.Length;
            } while (count >= data.Length);
            reply = ftp.ReadControlPort();
            ftp.CloseDataPort();
            fs.Flush();
            fs.Close();
        }
        

        public override string GetReply()
        {
            return reply;
        }
    }
}
