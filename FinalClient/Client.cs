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
    class Client
    {
        IPEndPoint endPoint, endPoint2, cloudEP;
        Socket localSocket, localRecieveSocket;
        private ManualResetEvent connectDone = new ManualResetEvent(false);
        private ManualResetEvent sendDone = new ManualResetEvent(false);
        private ManualResetEvent receiveDone = new ManualResetEvent(false);
        public ManualResetEvent allDone = new ManualResetEvent(false);
        public ManualResetEvent allReceive = new ManualResetEvent(false);
        private List<WireBand> AvalaibleBandIN;
        private List<WireBand> AvalaibleBandOUT;
        private List<Connection> Connections;

        
        private String response = String.Empty;

        public Client(string ip, int port)
        {
            endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            endPoint2 = new IPEndPoint(IPAddress.Parse(ip), port + 1);
            localSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            localRecieveSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            cloudEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8000);
            localSocket.Bind(endPoint);
            localRecieveSocket.Bind(endPoint2);
            localSocket.BeginConnect(cloudEP,
                    new AsyncCallback(ConnectCallback), localSocket);
            connectDone.WaitOne();
            Thread t = new Thread(Run);
            t.Start();
        }

        void Run()
        {
            try
            {
                while (true)
                {
                    allReceive.Reset();
                    Console.WriteLine("Waiting for data...");
                    Receive();
                    allReceive.WaitOne();
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

        public void Send(String data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            localSocket.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), localSocket);
            sendDone.WaitOne();
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes from {1} to server {2}.", bytesSent, client.LocalEndPoint.ToString(), client.RemoteEndPoint.ToString());

                // Signal that all bytes have been sent.
                sendDone.Set();
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
                state.workSocket = localSocket;
                //response = String.Empty;
                // Begin receiving the data from the remote device.
                localSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
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

                //if (bytesRead > 0)
                //{
                    // There might be more data, so store the data received so far.
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    // Get the rest of the data.
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);
                //}
                //else
               // {
                    // All the data has arrived; put it in response.
                    if (state.sb.Length > 1)
                    {
                        response = state.sb.ToString();
                    }
                    // Signal that all bytes have been received.
                    receiveDone.Set();
                    allReceive.Set();
                //}
                Console.WriteLine("Socket {0} Read '{1}'[{2} bytes] from socket {3}.", client.LocalEndPoint.ToString(),
                        response, response.Length, client.RemoteEndPoint.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }


    }

    public class StateObject
    {
        // Client socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 256;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }
}
