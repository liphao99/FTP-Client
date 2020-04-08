using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FTPUtil;
namespace UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestExecute()
        {
            FTP ftp = new FTP("192.168.1.105", 21); //命令端口
            UploadCommand uploadCommand = new UploadCommand(ftp, "D:/Games/Screenshots/test.png", "");
            uploadCommand.Execute();            
        }
    }
}
