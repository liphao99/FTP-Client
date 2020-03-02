using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTPUtil
{
    public interface ICommand
    {
        void Execute();
        

        String GetReply();
    }
}
