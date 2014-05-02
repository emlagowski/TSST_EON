using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;

namespace FinalClient
{
    class Signaling
    {
       IPEndPoint signalingEP, cloudSignalingEP;
       Socket signalingSocket;
       private ManualResetEvent signalingReceive = new ManualResetEvent(false);
       private ManualResetEvent connectDone = new ManualResetEvent(false);


       private List<WireBand> AvalaibleBandIN;
       private List<WireBand> AvalaibleBandOUT;
       private List<Connection> Connections;

       public Signaling()
       {
           initWireBand();//signaling
           //signaling
           signalingEP = new IPEndPoint(IPAddress.Parse("127.6.6.6"), 6666);
           cloudSignalingEP = new IPEndPoint(IPAddress.Parse("127.6.6.5"), 6666);
           signalingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
           signalingSocket.Bind(signalingEP);
           signalingSocket.BeginConnect(cloudSignalingEP,
                   new AsyncCallback(ConnectCallback), signalingSocket);
           connectDone.WaitOne();

           Thread t0 = new Thread(RunSignaling);
           t0.Start();
       }

       private void RunSignaling()
       {
           try
           {
               while (true)
               {
                   signalingReceive.Reset();
                   Console.WriteLine("Waiting for signaling data...");
                   Receive();
                   signalingReceive.WaitOne();
               }

           }
           catch (Exception e)
           {
               Console.WriteLine(e.ToString());
           }
       }

       private void initWireBand()
       {
           //trzeba dorobic potem
           //TEST
           AvalaibleBandIN = new List<WireBand>();
           AvalaibleBandOUT = new List<WireBand>();

           AvalaibleBandIN.Add(new WireBand(1, 12));
           AvalaibleBandIN.Add(new WireBand(2, 10));
           AvalaibleBandOUT.Add(new WireBand(1, 6));
           AvalaibleBandOUT.Add(new WireBand(2, 8));


       }
    }

}
