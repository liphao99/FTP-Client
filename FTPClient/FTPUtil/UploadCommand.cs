using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTPUtil
{
    class UploadCommand : TransferCommand
    {

        private Object lockObj = new Object();
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ftp">FTP链接</param>
        /// <param name="source">准备上传文件的绝对地址</param>
        /// <param name="destination">上传的目标目录，以 \ 结尾</param>
        public UploadCommand(FTP ftp, String source, String destination)
        {
            this.ftp = ftp;
            this.Source = source;
            this.Destination = destination;
        }

        public bool IsDestExist()
        {

            return false;
        }
        /// <summary>
        /// 中断上传，记录
        /// </summary>
        /// <returns></returns>
        public override ICommand Abort()
        {
            throw new NotImplementedException();
        }

        public override void Execute()
        {
            String fileName = Source.Split('\\').Last();
            lock (lockObj)
            {
                ftp.Send("STOR " + Source);
                String num = ftp.ReadControlPort().Split(' ').Last();
                Size = int.Parse(num);
                ftp.ConnectDataPortByPASV();
                reply = ftp.ReadControlPort();
            }
            FileStream fs = new FileStream(Destination + fileName, FileMode.Create);
            int count = 0;
            byte[] data;
            do
            {
                data = ftp.ReadDataPort(ref count);
                fs.Write(data, 0, data.Length);
                Point += data.Length;  //设置检查点
            } while (count >= data.Length);
            reply = ftp.ReadControlPort();
            ftp.CloseDataPort();
            fs.Flush();
            fs.Close(); 
        }

        public override string GetReply()
        {
            return this.reply;
        }
    }
}
