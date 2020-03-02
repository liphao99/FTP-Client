using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTPUtil
{
    public abstract class TransferCommand : ICommand
    {
        //需要绝对路径
        public String Source { get; set; }

        //需要绝对路径
        public String Destination { get; set; }

        //文件大小，单位字节
        public int Size { get; protected set; }

        //当前进度
        public int Point { get; protected set; }


        protected FTP ftp;
        protected String reply;

        public abstract void Execute();
        public abstract string GetReply();

        /// <summary>
        /// 暂停，返回断点重续对象
        /// </summary>
        /// <returns>该传输命令对应的断点传输命令</returns>
        public abstract ICommand Abort(); 
    }
}
