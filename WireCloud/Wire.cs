using System;
using System.Collections.Generic;
using System.IO;
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
            //Thread thread1 = new Thread(new ThreadStart(threadRun1));
            //Thread thread2 = new Thread(new ThreadStart(threadRun2));
            wireIsOn = true;
            //thread1.Start();
            //thread2.Start();
            StartTheThread(end1, port1, port2);
            StartTheThread(end2, port2, port1);
            Console.WriteLine("Wire beetwen ports {0} and {1} connected", port1, port2);


        }


        /* Jedna metoda sluzy do wolania z parametrami, dzieki czemu przekazujemy argumenty do watkow.
         */
        public Thread StartTheThread(TcpListener end, Int32 portStart, Int32 portStop)
        {
            var t = new Thread(() => threadRun(end, portStart, portStop));
            t.Start();
            return t;
        }

        /**
         * W watku zaczyna sie od nasluchiwania na danym porcie, a gdy cos przyjdzie pobieramy to do tablicy bajtowej, 
         * tworzymy TcpClienta zeby odebrane dane wyslac drugim portem.
         * */

        private void threadRun(TcpListener end, Int32 portStart, Int32 portStop)
        {
            end.Start();
            Console.WriteLine("Thread for port {0} started", portStart);
            while (wireIsOn)
            {
                try
                {
                    TcpClient client = end.AcceptTcpClient(); //Metoda ta blokuje wykonywanie kodu dopoki cos nie przyjdzie na port;
                    Console.WriteLine("Connection established at port: {0}", portStart);
                    Byte[] bytes = new Byte[256];
                    String data = null;
                    Stream stream = client.GetStream();

                    int i;
                    i = stream.Read(bytes, 0, bytes.Length);
                    // Loop to receive all the data sent by the client. 
                    while (i != 0)
                    {
                        // Translate data bytes to a ASCII string.
                        data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                        Console.WriteLine("Received at port {0}: {1}", portStart, data);
                        Console.WriteLine("Data size : " + data.Length);
                        i = stream.Read(bytes, 0, bytes.Length);
                    }
                    TcpClient sendingClient = new TcpClient();
                    sendingClient.Connect(localIP, portStop);
                    Stream streamSend = sendingClient.GetStream();
                    byte[] sendMsg = System.Text.Encoding.ASCII.GetBytes(data);
                    streamSend.Write(sendMsg, 0, sendMsg.Length);
                    client.Close();
                    sendingClient.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            Console.WriteLine("Exit form thread");
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
