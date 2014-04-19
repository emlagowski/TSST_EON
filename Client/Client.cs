using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class Client
    {
        Int32 port;
        TcpListener listener;
        TcpClient tcpClient;
        IPAddress localIP;

        public Client()
        {
            localIP = IPAddress.Parse(LocalIPAddress());
        }
        public void send(string str)
        {
            
            NetworkStream stream1 = tcpClient.GetStream();
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(str);
            stream1.Write(msg, 0, msg.Length);
            Console.WriteLine("Message sent: {0}", str);

        }
        public void connect(Int32 port)
        {
            this.port = port;
            IPEndPoint localend = new IPEndPoint(localIP, port);
            TcpClient tcpClient = new TcpClient(localend);
            // = new TcpListener(localIP, port);
            Thread thread = new Thread(new ThreadStart(threadRun));
            thread.Start();
            Console.WriteLine("Starting listening at port {0}", port);


        }
        private void threadRun()
        {
            //listener.Start();
            Console.WriteLine("Thread for port {0} started", port);
            //TcpClient client = listener.AcceptTcpClient(); //Metoda ta blokuje wykonywanie kodu dopoki cos nie przyjdzie na port;
            Console.WriteLine("Connection established at port: {0}", port);
            Byte[] bytes = new Byte[256];
            String data = null;
            NetworkStream stream = tcpClient.GetStream();

            int i;

            // Loop to receive all the data sent by the client. 
            while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
            {
                // Translate data bytes to a ASCII string.
                data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                Console.WriteLine("Received at port {0}: {1}", port, data);

                // Process the data sent by the client.
                //data = data.ToUpper();

                //byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);

                // Send back a response.
                // stream.Write(msg, 0, msg.Length);
                //Console.WriteLine("Sent: {0}", data);
            }

        }

        /*Nie wiem czy ta metoda jest potrzebna pewnie mozna inaczej to porobic na localhoscie
        * w kazdym razie metoda ta pobiera adres IP komputera na ktorym wszystko to dziala.
        * 
        */
        public string LocalIPAddress()
        {
            IPHostEntry host;
            string localIP = "";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }
    }
}
