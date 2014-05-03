using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;

namespace FinalClient
{
    class Signaling
    {
        IPEndPoint signalingEP, cloudSignalingEP;
        Socket signalingSocket;
        private ManualResetEvent signalingReceive = new ManualResetEvent(false);
        private ManualResetEvent connectDone = new ManualResetEvent(false);
        private ManualResetEvent receiveDone = new ManualResetEvent(false);
        private ManualResetEvent sendDone = new ManualResetEvent(false);

        private String response = String.Empty;


        private List<WireBand> AvalaibleBandIN;
        private List<WireBand> AvalaibleBandOUT;
        private List<Connection> Connections;

        public Signaling()
        {
            initWireBand();
            //NARAZIE ZAKOMENTOWANE BO TO DO 2 ETAPU

            /*   signalingEP = new IPEndPoint(IPAddress.Parse("127.6.6.6"), 6666);
               cloudSignalingEP = new IPEndPoint(IPAddress.Parse("127.6.6.5"), 6666);
               signalingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
               signalingSocket.Bind(signalingEP);
               signalingSocket.BeginConnect(cloudSignalingEP,
                       new AsyncCallback(ConnectCallback), signalingSocket);
               connectDone.WaitOne();

               Thread t0 = new Thread(RunSignaling);
               t0.Start();*/
        }

        private void RunSignaling()
        {
            try
            {
                while (true)
                {
                    signalingReceive.Reset();
                    Console.WriteLine("SIGNALING: Waiting for signaling data...");
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
                SignalingStateObject state = (SignalingStateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // byte[] clientData = new byte[1024 * 1024 * 50];

                // int receivedBytesLen = clientSock.Receive(clientData);

                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar);

                BinaryFormatter formattor = new BinaryFormatter();

                MemoryStream ms = new MemoryStream(state.buffer);

                state.conn = (Connection)formattor.Deserialize(ms);

                Connections.Add(state.conn);
                //TUTAJ JESCZE TRZEBA BEDZIE DOROBIC ZE JAK OTRZYMA OBIEKT TO MUSI ZAALOKOWAC OBPOWIEDNIE PASMA W POLACZENIACH IN I OUT.


                /* if (bytesRead > 0)
                 {
                     // There might be more data, so store the data received so far.
                     //state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                     System.arraycopy(state.buffer, 0, state.result, 0, state.buffer.length);

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
                 }*/
                receiveDone.Set();
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

                Console.WriteLine("SIGNALING: {0} Socket connected to {1}", client.LocalEndPoint.ToString(), client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.
                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        private void Send(Socket client, Connection conn)
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
                Console.WriteLine("SIGNALING: Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }


        private void initWireBand()
        {

            AvalaibleBandIN = new List<WireBand>();
            AvalaibleBandOUT = new List<WireBand>();


            String xmlString = File.ReadAllText("wires.xml");
            using (XmlReader reader = XmlReader.Create(new StringReader(xmlString)))
            {
                while (reader.ReadToFollowing("wire"))
                {
                    reader.MoveToAttribute("Id");
                    string id = reader.Value;
                    reader.MoveToNextAttribute();
                    string capacity = reader.Value;
                    reader.MoveToNextAttribute();
                    string distance = reader.Value;

                    AvalaibleBandIN.Add(new WireBand(Convert.ToInt32(id), Convert.ToInt32(capacity), Convert.ToInt32(distance)));


                }
            }

        }

        public void checkIfConnEstablished()
        {

        }

        private void updateLambdas()
        {

        }
    }
    public class SignalingStateObject
    {
        // Client socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 256;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        public Connection conn { get; set; }
    }
}

