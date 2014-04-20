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
       // Int32 port1;
        EndPoint endpoint1;
        EndPoint endpoint2;
      //  Int32 port2;
        IPAddress localIP;
        Boolean wireIsOn;
        /*W konstruktorze tworzymy TcpListenery i odpalamy dla nich oddzielne watki ktore nasluchuja na portach. Chcialem jeden watek dla jednego kabla
         * ale nie wiem jak bo kazdy nasluch blokuje wykonywanie kodu. Sa jakies bajery do zrobienia watkow bardziej wydajnie chyba typu ThreadPool
         * ale nie chcialomi sie wglebiac juz.
         */
        public Wire(int sendingPortA, int receivingPortA, int sendingPortB, int receivingPortB)
        {
            endpoint1 = new EndPoint(sendingPortA, receivingPortA);
            endpoint2 = new EndPoint(sendingPortB, receivingPortB);            
            localIP = IPAddress.Parse(LocalIPAddress());
            end1 = new TcpListener(localIP, endpoint1.receivingPort);
            end2 = new TcpListener(localIP, endpoint2.receivingPort);
            //Thread thread1 = new Thread(new ThreadStart(threadRun1));
            //Thread thread2 = new Thread(new ThreadStart(threadRun2));
            wireIsOn = true;
            //thread1.Start();
            //thread2.Start();
            StartTheThread(end1, endpoint1, endpoint2);
            StartTheThread(end2, endpoint2, endpoint1);
            Console.WriteLine("Wire beetwen ENDPOINT[RECEIVING: {0} SENDING: {1}] \n& ENDPOINT[RECEIVING: {2} SENDING: {3}] connected",
                endpoint1.receivingPort, endpoint1.sendingPort, endpoint2.receivingPort, endpoint2.sendingPort);


        }


        /* Jedna metoda sluzy do wolania z parametrami, dzieki czemu przekazujemy argumenty do watkow.
         */
        public Thread StartTheThread(TcpListener end, EndPoint pointStart, EndPoint pointStop)
        {
            var t = new Thread(() => threadRun(end, pointStart, pointStop));
            t.Start();
            return t;
        }

        /**
         * W watku zaczyna sie od nasluchiwania na danym porcie, a gdy cos przyjdzie pobieramy to do tablicy bajtowej, 
         * tworzymy TcpClienta zeby odebrane dane wyslac drugim portem.
         * */

        private void threadRun(TcpListener end, EndPoint pointStart, EndPoint pointStop)
        {
            end.Start();
            Console.WriteLine("Thread for ports RECIEIVING {0} & SENDING: {1} started", pointStart.receivingPort, pointStart.sendingPort);
            while (wireIsOn)
            {
                try
                {
                    TcpClient client = end.AcceptTcpClient(); //Metoda ta blokuje wykonywanie kodu dopoki cos nie przyjdzie na port;
                    Console.WriteLine("Connection established at RECEIVING port: {0}", pointStart.receivingPort);
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
                        Console.WriteLine("Received at port {0}: {1}", pointStart.receivingPort, data);
                        Console.WriteLine("Data size : " + data.Length);
                        i = stream.Read(bytes, 0, bytes.Length);

                    }
                    TcpClient sendingClient = new TcpClient();
                    sendingClient.Connect(localIP, pointStop.receivingPort);
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
