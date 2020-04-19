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
            UploadCommand uploadCommand = new UploadCommand(ftp, "D:\\Games\\Screenshots\\newTest.png", "");
            uploadCommand.Execute();            
        }

        [TestMethod]
        public void downloadTest()
        {
            FTP ftp = new FTP("192.168.1.105", 21); //命令端口
            DownloadCommand download = new DownloadCommand(ftp, "newTest.png", "C:\\Users\\11640\\Desktop\\");
            download.Execute();
        }
    }
}
