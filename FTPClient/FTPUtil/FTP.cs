using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FTPUtil
{
    public class FTP
    {

        //控制端口的socket用于传输命令，数据端口的socket用于数据传输
        internal Socket controlSocket;
        internal Socket dataSocket;

        private String serverHost;

        public FTP(String serverHost, int portInt, String user, String password)
        {
            this.serverHost = serverHost;
            Connect(ref controlSocket, serverHost, portInt);
            ReadControlPort();
            Send("USER anonymous");
            ReadControlPort();
            Send("PASS anonymous");
            ReadControlPort();

            //默认进入被动模式
            Send("PASV");
            String dataSocketMessage = ReadControlPort();
            String[] datas = dataSocketMessage.Split('(')[1].Split(')')[0].Split(',');
            int dataPort = int.Parse(datas[4]) * 256 + int.Parse(datas[5]);

            Connect(ref dataSocket, serverHost, dataPort);
        }

        public FTP(String serverHost, int portInt):this(serverHost, portInt, "anonymous", "anonymous"){}

        /// <summary>
        /// 向服务器的控制端口发送消息
        /// </summary>
        internal void Send(String order)
        {
            byte[] bytes = Encoding.Default.GetBytes((order + "\r\n").ToCharArray());
            controlSocket.Send(bytes, bytes.Length, 0);
        }

        /// <summary>
        /// 从客户端的控制端口读消息
        /// </summary>
        internal String ReadControlPort()
        {
            String reply = String.Empty;
            Thread.Sleep(200);
            byte[] buffer = new byte[1024];
            int count;
            do
            {
                count = controlSocket.Receive(buffer, buffer.Length, 0);
                reply += Encoding.UTF8.GetString(buffer, 0, count);
            } while (count >= buffer.Length);
            return reply;
        }

        /// <summary>
        /// 从客户端的数据端口读消息，并返回字符串
        /// </summary>
        internal String ReadDataPortAsString()
        {
            String reply = String.Empty;
            //Thread.Sleep(200);
            byte[] buffer = new byte[1024];
            int count;
            do
            {
                count = dataSocket.Receive(buffer, buffer.Length, 0);
                reply += Encoding.UTF8.GetString(buffer, 0, count);
            } while (count >= buffer.Length);
            return reply;
        }

        /// <summary>
        /// 从客户端的数据端口读消息，并返回字节流
        /// </summary>
        internal byte[] ReadDataPortAsByte(ref int count)
        {
            byte[] buffer = new byte[1024];
            count = dataSocket.Receive(buffer, buffer.Length, 0);
            return buffer;
        }

        /// <summary>
        /// 建立socket连接
        /// </summary>
        /// <param name="socket">socket的引用</param>
        /// <param name="serverHost">服务器ip</param>
        /// <param name="port">端口号</param>
        internal void Connect(ref Socket socket,String serverHost,int port)
        {
            IPHostEntry serverEntry = Dns.GetHostEntry(serverHost);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint endpoint = new IPEndPoint(serverEntry.AddressList[1], port);//AddressList[1]存疑
            socket.Connect(endpoint);
        }

        public static void Main()
        {
            FTP ftp = new FTP("192.168.1.4", 21);
            ICommand cmd = new DownloadCommand(ftp,"\\test.txt","D:\\");
            cmd.Execute();
            Console.Write(cmd.GetReply());
        }
    }
}
