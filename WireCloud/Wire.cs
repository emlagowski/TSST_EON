using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WireCloud
{
    class Wire
    {
        TcpListener end1;
        TcpListener end2;
        Int32 port1;
        Int32 port2;
        IPAddress localIP;
        Boolean wireIsOn;
        /*W konstruktorze tworzymy TcpListenery i odpalamy dla nich oddzielne watki ktore nasluchuja na portach. Chcialem jeden watek dla jednego kabla
         * ale nie wiem jak bo kazdy nasluch blokuje wykonywanie kodu. Sa jakies bajery do zrobienia watkow bardziej wydajnie chyba typu ThreadPool
         * ale nie chcialomi sie wglebiac juz.
         */
        public Wire(int port1, int port2)
        {
            this.port1 = port1;
            this.port2 = port2;
            localIP = IPAddress.Parse(LocalIPAddress());
            end1 = new TcpListener(localIP, port1);
            end2 = new TcpListener(localIP, port2);
            Thread thread1 = new Thread(new ThreadStart(threadRun1));
            Thread thread2 = new Thread(new ThreadStart(threadRun2));
            wireIsOn = true;
            thread1.Start();
            thread2.Start();
            Console.WriteLine("Wire beetwen ports {0} and {1} connected", port1, port2);


        }
        /* Sa dwie oddzielne blizniacze metody d watkow bo nie mozna przekazywac paratmetrow do metod uzywanych w watku wiec nie wiedzialem jak przekazac
         * wartosci end1 i end2. W watku zaczyna sie od nasluchiwania na danym porcie, a gdy cos przyjdzie pobieramy to do tablicy bajtowej, 
         * tworzymy TcpClienta zeby odebrane dane wyslac drugim portem.
         */
        private void threadRun1()
        {
            end1.Start();
            Console.WriteLine("Thread for port {0} started", port1);
            while (wireIsOn)
            {
                TcpClient client = end1.AcceptTcpClient(); //Metoda ta blokuje wykonywanie kodu dopoki cos nie przyjdzie na port;
                Console.WriteLine("Connection established at port: {0}", port1);
                Byte[] bytes = new Byte[256];
                String data = null;
                NetworkStream stream = client.GetStream();

                int i;

                // Loop to receive all the data sent by the client. 
                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    // Translate data bytes to a ASCII string.
                    data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                    Console.WriteLine("Received at port {0}: {1}", port1, data);

                    // Process the data sent by the client.
                    //data = data.ToUpper();

                    //byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);

                    // Send back a response.
                    // stream.Write(msg, 0, msg.Length);
                    //Console.WriteLine("Sent: {0}", data);
                }
                IPEndPoint localend = new IPEndPoint(localIP, port2);
                TcpClient sendingClient = new TcpClient();
                sendingClient.Connect(localIP, port2);
                NetworkStream stream1 = sendingClient.GetStream();
                stream1.Write(bytes, 0, bytes.Length);
            }
            Console.WriteLine("Exit form 1 thread");
        }
        private void threadRun2()
        {
            end2.Start();
            Console.WriteLine("Thread for port {0} started", port2);
            while (wireIsOn)
            {
                TcpClient client = end2.AcceptTcpClient();
                Console.WriteLine("Connection established at port: {0}", port2);
                Byte[] bytes = new Byte[256];
                String data = null;
                NetworkStream stream = client.GetStream();

                int i;

                // Loop to receive all the data sent by the client. 
                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    // Translate data bytes to a ASCII string.
                    data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                    Console.WriteLine("Received at port {0}: {1}", port2, data);


                    // Process the data sent by the client.
                    //data = data.ToUpper();

                    //byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);

                    // Send back a response.
                    // stream.Write(msg, 0, msg.Length);
                    //Console.WriteLine("Sent: {0}", data);
                }
                IPEndPoint localend = new IPEndPoint(localIP, port1);
                TcpClient sendingClient = new TcpClient();
                sendingClient.Connect(localIP, port1);
                NetworkStream stream1 = sendingClient.GetStream();
                stream1.Write(bytes, 0, bytes.Length);
            }
            Console.WriteLine("Exit form 2 thread");
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
