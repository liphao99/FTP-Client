using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTPUtil
{
    public class UploadCommand : TransferCommand
    {

        private Object lockObj = new Object();
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ftp">FTP链接</param>
        /// <param name="source">准备上传文件的绝对路径, 如C:/Myfiles/file.txt</param>
        /// <param name="destination">上传的目标路径</param>
        public UploadCommand(FTP ftp, String source, String destination)
        {
            this.ftp = ftp;
            this.Source = source;
            this.Destination = destination;
        }


        /// <summary>
        /// 中断上传，返回断点续传类
        /// </summary>
        /// <returns></returns>
        public override Command Abort()
        {
            if (!started)
            {
                return null;
            }
            lock (lockObj)
            {
                ftp.CloseDataPort();
                ftp.Send("ABOR");
                reply = ftp.ReadControlPort();
            }
            return new UploadContinue(ftp, Source, Destination, Point);
        }

        /// <summary>
        /// 将目标文件发送到服务端
        /// </summary>
        public override void Execute()
        {
            /* ftp已经初始化，利用ftp进行文件传输
            * 首先得到本地文件
            * 通过连接进行IO写入
            * 清理资源
            */
            String[] dirArr = this.Source.Split('\\');
            String fileName = dirArr[dirArr.Length - 1];
            ftp.ConnectDataPortByPASV();
            ftp.Send("CWD " + Destination + "\r\n");
            reply = ftp.ReadControlPort();
            Console.WriteLine(reply);
            ftp.Send("STOR " + fileName + "\r\n");
            reply = ftp.ReadControlPort();
            Console.WriteLine(reply);
            //if (!reply.Equals("150"))
            //{
            //    throw new Exception("Exception occurs!");
            //}
            //读取本地文件
            FileStream fs = new FileStream(Source, FileMode.Open, FileAccess.Read);
            try
            {
                int count = 0;
                byte[] buffer = new byte[fs.Length];
                fs.Read(buffer, Point, (int)fs.Length);
                ftp.WriteDataPort(buffer, ref count);
                this.Point = count;

            }
            catch(IOException e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                fs.Close();
                ftp.CloseDataPort();
            }
        }

        public override string GetReply()
        {
            return this.reply;
        }

        
    }
}
