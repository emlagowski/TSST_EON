using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace CloudNew
{
    class Program
    {
        static void Main(string[] args)
        {
            Cloud cloud = new Cloud(2222);
            /*TcpClient a1 = new TcpClient();
            a1.Connect(cloud.localIP, cloud.port);
            NetworkStream ns = a1.GetStream();
            byte[] sendMsg = System.Text.Encoding.ASCII.GetBytes("127.0.0.1:3|127.0.0.2:5");
            ns.Write(sendMsg, 0, sendMsg.Length);*/
            Console.ReadLine();
        }
    }
}
