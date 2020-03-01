using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTPUtil
{
    public class CommandFactory
    {
        private FTP ftp;

        public CommandFactory(FTP ftp)
        {
            this.ftp = ftp;
        }

        public ICommand GetCommand(String command)
        {
            switch (command)
            {
                case "RETR":
                    return new DownloadCommand(ftp);
                case "LIST":
                    //return new USERCommand(ftp);
                default:
                    return new SingleCommand(ftp,command);
            }
        }
    }
}
