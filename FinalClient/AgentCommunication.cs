//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Net;
//using System.Net.Sockets;
//using System.Runtime.Serialization.Formatters.Binary;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace Router
//{
//    public class AgentCommunication
//    {
//        IPEndPoint agentLocalEP, agentEP;
//        public static Socket agentSocket;
       
        
//        String address;
//        //  private ManualResetEvent allReceive = new ManualResetEvent(false);
//        private ManualResetEvent agentConnectDone = new ManualResetEvent(false);
//        private ManualResetEvent agentReceiveDone = new ManualResetEvent(false);
//        private ManualResetEvent agentSendDone = new ManualResetEvent(false);

//        public AgentCommunication(String ip)
//        {
//            address = ip;
           
//            agentLocalEP = new IPEndPoint(IPAddress.Parse(ip), 6666);
//            agentEP = new IPEndPoint(IPAddress.Parse("127.6.6.6"), 6666);
//            agentSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
//            agentSocket.Bind(agentLocalEP);
//            agentSocket.ReceiveBufferSize = 1024 * 100;

//            agentSocket.BeginConnect(agentEP,
//                    new AsyncCallback(AgentConnectCallback), agentSocket);
//            agentConnectDone.WaitOne();


//            Thread agentThread = new Thread(agentRun);
//            agentThread.Start();
//        }

//        private void AgentConnectCallback(IAsyncResult ar)
//        {
//            try
//            {
//                // Retrieve the socket from the state object.
//                Socket client = (Socket)ar.AsyncState;

//                // Complete the connection.
//                lock (this)
//                {
//                    client.EndConnect(ar);
//                }
//                Console.WriteLine("SIGNALING: {0} Socket connected to {1}", client.LocalEndPoint.ToString(), client.RemoteEndPoint.ToString());

//                // Signal that the connection has been made.
//                agentConnectDone.Set();
//            }
//            catch (Exception e)
//            {
//                Console.WriteLine(e.ToString());
//            }
//        }

//        private void agentRun()
//        {
//            try
//            {
//                while (true)
//                {
//                    agentReceiveDone.Reset();
//                    Console.WriteLine("Waiting for data from AGENT...");
//                    AgentReceive();
//                    agentReceiveDone.WaitOne();
//                }

//            }
//            catch (Exception e)
//            {
//                Console.WriteLine(e.ToString());
//            }
//        }

//        public void AgentReceive()
//        {
//            try
//            {
//                // Create the state object.
//                AgentStateObject state = new AgentStateObject();
//                state.workSocket = agentSocket;
//                //response = String.Empty;
//                // Begin receiving the data from the remote device.
//                agentSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
//                    new AsyncCallback(AgentReceiveCallback), state);
//            }
//            catch (Exception e)
//            {
//                Console.WriteLine(e.ToString());
//            }
//        }

//        private void AgentReceiveCallback(IAsyncResult ar)
//        {
//            try
//            {
//                // Retrieve the state object and the client socket 
//                // from the asynchronous state object.
//                AgentStateObject state = (AgentStateObject)ar.AsyncState;
//                Socket client = state.workSocket;

//                // to bardzo wazne przypisanie socketu, otrzymanego po zestawieniu polaczenia i nasluch ustawiany musi byc na tym sockecie!
//                agentSocket = client;

//                int bytesRead = client.EndReceive(ar);

//                BinaryFormatter formattor = new BinaryFormatter();

//                MemoryStream ms = new MemoryStream(state.buffer);

//                state.ad = (ExtSrc.AgentData)formattor.Deserialize(ms);
            
//                ProcessAgentData(state.ad);
            
//                agentReceiveDone.Set();
//            }
//            catch (Exception e)
//            {
//                Console.WriteLine(e.ToString());
//            }
//        }

//        private void ProcessAgentData(ExtSrc.AgentData agentData)
//        {
//            switch (agentData.message)
//            {
//                case ExtSrc.AgentComProtocol.ROUTE_FOR_U_EDGE:
                    
//                    break;
//                case ExtSrc.AgentComProtocol.ROUTE_FOR_U:
//                    break;
//                case ExtSrc.AgentComProtocol.DISROUTE:
//                    break;
//                case ExtSrc.AgentComProtocol.DISROUTE_EDGE:
//                    break;
//                case ExtSrc.AgentComProtocol.U_CAN_SEND:
//                    break;
               
//                default:
//                    Console.WriteLine("Zły msg przybył");
//                    break;
//            }
//        }

//        public void AgentSend( ExtSrc.AgentData conn)
//        {
//            MemoryStream fs = new MemoryStream();

//            BinaryFormatter formatter = new BinaryFormatter();

//            formatter.Serialize(fs, conn);

//            byte[] buffer = fs.ToArray();



//            // Begin sending the data to the remote device.
//            agentSocket.BeginSend(buffer, 0, buffer.Length, 0,
//                new AsyncCallback(AgentSendCallback), agentSocket);
//            agentSendDone.WaitOne();
//        }

//        private void AgentSendCallback(IAsyncResult ar)
//        {
//            try
//            {
//                // Retrieve the socket from the state object.
//                Socket client = (Socket)ar.AsyncState;

//                // Complete sending the data to the remote device.
//                int bytesSent = client.EndSend(ar);
//                Console.WriteLine("Sent {0} bytes to AGENT.", bytesSent);

//                // Signal that all bytes have been sent.
//                agentSendDone.Set();
//            }
//            catch (Exception e)
//            {
//                Console.WriteLine(e.ToString());
//            }
//        }

//    }
//    public class AgentStateObject
//    {
//        // Client socket.
//        public Socket workSocket = null;
//        // Size of receive buffer.
//        public const int BufferSize = 1024 * 100;
//        // Receive buffer.
//        public byte[] buffer = new byte[BufferSize];//nn
//        public ExtSrc.AgentData ad { get; set; }
//    }
//}//nnn
