using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using System.Net;
using System.Collections;

namespace Router
{
    class RouterProgram
    {
        static void Main(string[] args)
        {



            /*TcpClient connection = new Router("www.google.com", 80, 5000).Connect();
            NetworkStream stream = connection.GetStream();

            // Send 10 bytes
            byte[] to_send = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0xa };
            stream.Write(to_send, 0, to_send.Length);

            // Receive 10 bytes
            byte[] readbuf = new byte[10]; // you must allocate space first
            stream.ReadTimeout = 10000; // 10 second timeout on the read
            stream.Read(readbuf, 0, 10); // read

            // Disconnect nicely
            stream.Close(); // workaround for a .net bug: http://support.microsoft.com/kb/821625
            connection.Close();*/

            IPAddress localIP = IPAddress.Parse(Router.LocalIPAddress());
            TcpClient client = new TcpClient();
            Console.Write("Enter the INT port number : ");
            int port = Convert.ToInt32(Console.ReadLine());
            client.Connect(localIP, port);
            Console.WriteLine("Connected");
            Console.Write("Enter the string to be transmitted : ");

            String str = Console.ReadLine();
            Stream stm = client.GetStream();

            ASCIIEncoding asen = new ASCIIEncoding();
            byte[] ba = asen.GetBytes(str);
            Console.WriteLine("Transmitting.....");
            BitArray bits = new BitArray(ba);
            for(int i = 0; i < bits.Length; i++)
            {
                if(bits[i]) Console.Write("1");
                else Console.Write("0");
            }
            stm.Write(ba, 0, ba.Length);

            byte[] bb = new byte[100];
            //int k = stm.Read(bb, 0, 100);

            for (int i = 0; i < 5; i++)
                Console.Write(Convert.ToChar(bb[i]));

            client.Close();

            Console.ReadLine();
            ///hjgfhgdfs
        }
    }
}
