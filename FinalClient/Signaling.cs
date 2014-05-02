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
       private ManualResetEvent receiveDone = new ManualResetEvent(false);
       private String response = String.Empty;


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
        public void Receive()
        {
            try
            {
                // Create the state object.
                StateObject state = new StateObject();
                state.workSocket = signalingSocket;
                //response = String.Empty;
                // Begin receiving the data from the remote device.
                signalingSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket 
                // from the asynchronous state object.
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    // Get the rest of the data.
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    // All the data has arrived; put it in response.
                    if (state.sb.Length > 1)
                    {
                        response = state.sb.ToString();
                    }
                    // Signal that all bytes have been received.
                    receiveDone.Set();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

       private void ConnectCallback(IAsyncResult ar)
       {
           try
           {
               // Retrieve the socket from the state object.
               Socket client = (Socket)ar.AsyncState;

               // Complete the connection.
               client.EndConnect(ar);

               Console.WriteLine("{0} Socket connected to {1}", client.LocalEndPoint.ToString(), client.RemoteEndPoint.ToString());

               // Signal that the connection has been made.
               connectDone.Set();
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
