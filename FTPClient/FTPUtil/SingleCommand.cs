using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTPUtil
{
    class SingleCommand:ICommand
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

        public void Execute(string cmd){}

        public string GetReply()
        {
            return reply;
        }
    }
}
