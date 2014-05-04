using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FinalClient
{
    public class AgentCommunication
    {
        IPEndPoint localEP, agentEP;
        public static Socket socket;
        private ManualResetEvent signalingReceive = new ManualResetEvent(false);
        private ManualResetEvent connectDone = new ManualResetEvent(false);
        private ManualResetEvent receiveDone = new ManualResetEvent(false);
        private ManualResetEvent sendDone = new ManualResetEvent(false);
        
        public AgentCommunication(String ip) 
        {
            localEP = new IPEndPoint(IPAddress.Parse(ip), 6666);
            agentEP = new IPEndPoint(IPAddress.Parse("127.6.6.6"), 6666);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(localEP);
            socket.BeginConnect(agentEP,
                    new AsyncCallback(ConnectCallback), socket);
            connectDone.WaitOne();


            Thread t0 = new Thread(Run);
            t0.Start();
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.
                client.EndConnect(ar);

                Console.WriteLine("SIGNALING: {0} Socket connected to {1}", client.LocalEndPoint.ToString(), client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.
                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void Run()
        {
            try
            {
                while (true)
                {
                    signalingReceive.Reset();
                    Console.WriteLine("Waiting for data from AGENT...");
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
                SignalingStateObject state = new SignalingStateObject();
                state.workSocket = socket;
                //response = String.Empty;
                // Begin receiving the data from the remote device.
                socket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
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
                AgentStateObject state = (AgentStateObject)ar.AsyncState;
                Socket client = state.workSocket;

              
                int bytesRead = client.EndReceive(ar);

                BinaryFormatter formattor = new BinaryFormatter();

                MemoryStream ms = new MemoryStream(state.buffer);

                state.ad = (ExtSrc.AgentData)formattor.Deserialize(ms);

                
                             
                receiveDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void Send(Socket client, ExtSrc.AgentData conn)
        {
            MemoryStream fs = new MemoryStream();

            BinaryFormatter formatter = new BinaryFormatter();

            formatter.Serialize(fs, conn);

            byte[] buffer = fs.ToArray();



            // Begin sending the data to the remote device.
            client.BeginSend(buffer, 0, buffer.Length, 0,
                new AsyncCallback(SendCallback), client);
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
                Console.WriteLine("Sent {0} bytes to AGENT.", bytesSent);

                // Signal that all bytes have been sent.
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

    }
    public class AgentStateObject
    {
        // Client socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024*10;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];//nn
        public ExtSrc.AgentData ad { get; set; }
    }
}//nnn
