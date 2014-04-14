using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Router
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpClient connection = new Router("www.google.com", 80, 5000).Connect();
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
            connection.Close();

            Console.ReadLine();
        }
    }
}
