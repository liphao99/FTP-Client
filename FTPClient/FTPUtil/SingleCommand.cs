using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTPUtil
{
    /// <summary>
    /// 只需要单条指令且不需要使用数据端口的简单命令类
    /// </summary>
    public class SingleCommand:ICommand
    {
        private FTP ftp;
        private String reply;
        private String cmd;
        public SingleCommand(FTP ftp, String cmd)
        {
            this.cmd = cmd;
            this.ftp = ftp;
        }

        public void Execute()
        {
            ftp.Send(cmd);
            reply = ftp.ReadControlPort();
        }
        

        public string GetReply()
        {
            return reply;
        }
    }
}
