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
using System.Xml;

namespace Client
{
    public class Client
    {
        public String localAddress, remoteAddress;
        IPEndPoint localEndPoint, remoteEndPoint;
        Socket socket;

        public List<KeyValuePair<MsgType, String>> messages { get; set; }

        private ManualResetEvent connectDone = new ManualResetEvent(false);
        public ManualResetEvent allReceive = new ManualResetEvent(false);
        private ManualResetEvent receiveDone = new ManualResetEvent(false);
        private ManualResetEvent sendDone = new ManualResetEvent(false);

        private String response = String.Empty;


        public Client(String ip)
        {
            localAddress = ip;
            messages = new List<KeyValuePair<MsgType, string>>();
        }

        public void initialization()
        {
            localEndPoint = new IPEndPoint(IPAddress.Parse(localAddress), 7000);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(localEndPoint);
        }

        public string IpToString(EndPoint endPoint)
        {
            return (endPoint as IPEndPoint).Address.ToString();
        }

        /**
         * Metoda wołana w celu nawiazania połaczenia z routerem brzegowym.
         **/
        public void connect(String router_ip)
        {
            Console.WriteLine("Connecting to " + router_ip);
            remoteAddress = router_ip;
            remoteEndPoint = new IPEndPoint(IPAddress.Parse(remoteAddress), 7000);
            Thread t = new Thread(Run);
            t.Start();
            socket.BeginConnect(remoteEndPoint, new AsyncCallback(ConnectCallback), socket);
            connectDone.WaitOne();
        }

        /**
         * Ta metoda jest automatycznie wolana pod podlaczeniu sie do routera. Konczy ustanawianie polaczenia.
         **/
        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.
                client.EndConnect(ar);

                Console.WriteLine("Connected to {0}", IpToString(client.RemoteEndPoint));

                // Signal that the connection has been made.
                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        /**
         * Metoda zapewnia ciągłe odbieranie w kliencie.
         **/
        void Run()
        {
            try
            {
                while (true)
                {
                    allReceive.Reset();
                    Receive();
                    allReceive.WaitOne();
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
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar);
                BinaryFormatter formattor = new BinaryFormatter();

                MemoryStream ms = new MemoryStream(state.buffer);

                state.dt = (ExtSrc.ClientData)formattor.Deserialize(ms);
                receiveDone.Set();
                allReceive.Set();
                Console.WriteLine("R: '{0}'[{1} bytes] from router {2}.",
                          state.dt.info, bytesRead, state.dt.EndAddress);
                messages.Add(new KeyValuePair<MsgType, string>(MsgType.RECEIVED, state.dt.ToString()));
                //addLog("Receive", client.RemoteEndPoint.ToString(), client.LocalEndPoint.ToString(), state.dt.ToString());

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void Send(int bandwidth, String data, String endAddress)
        
        {
            if (endAddress.Equals(localAddress))
            {
                Console.WriteLine("Sent to yourself: "+data);
                return;
            }
            MemoryStream fs = new MemoryStream();

            BinaryFormatter formatter = new BinaryFormatter();

            formatter.Serialize(fs, new ExtSrc.ClientData(bandwidth, data, endAddress));
            
            byte[] buffer = fs.ToArray();

            // Begin sending the data to the remote device.
            socket.BeginSend(buffer, 0, buffer.Length, 0,
                new AsyncCallback(SendCallback), socket);
            messages.Add(new KeyValuePair<MsgType, string>(MsgType.SEND, data));
            //addLog("Send", localAddress, targetIP, data);
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
                Console.WriteLine("S: {0} bytes to {1}.", bytesSent, IpToString(client.RemoteEndPoint));
                // Signal that all bytes have been sent.
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void closing()
        {
            socket.Close();
            System.Windows.Forms.Application.Exit();
            System.Environment.Exit(1);
        }
    }

    public enum MsgType
    {
        SEND, RECEIVED
    }

    public class StateObject
    {
        // Client socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024*5;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public ExtSrc.ClientData dt;
    }
}
