using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Collections;

namespace FinalClient
{
    class Client
    {
        public static XmlDocument wires;
        public static FIB fib;
        String address;
        IPEndPoint cloudEP;
        ArrayList sockets;
        private ManualResetEvent connectDone = new ManualResetEvent(false);
        private ManualResetEvent sendDone = new ManualResetEvent(false);
        private ManualResetEvent receiveDone = new ManualResetEvent(false);
        public ManualResetEvent allDone = new ManualResetEvent(false);
        public ManualResetEvent allReceive = new ManualResetEvent(false);

        private String response = String.Empty;

        public static void readFIB()
        {
            fib = new FIB();
            String xmlString = File.ReadAllText("wires.xml");
            using (XmlReader reader = XmlReader.Create(new StringReader(xmlString)))
            {
                while (reader.ReadToFollowing("wire"))
                {
                    reader.MoveToFirstAttribute();
                    string f_ip = reader.Value;
                    reader.MoveToNextAttribute();
                    string f_port = reader.Value;
                    reader.MoveToNextAttribute();
                    string s_ip = reader.Value;
                    reader.MoveToNextAttribute();
                    string s_port = reader.Value;

                    fib.add(new Wire(   new IPEndPoint(IPAddress.Parse(f_ip), Convert.ToInt32(f_port)), 
                                        new IPEndPoint(IPAddress.Parse(s_ip), Convert.ToInt32(s_port))   )   );
                }
            }
        }


        private ArrayList findingPorts()
        {
            ArrayList tmp = new ArrayList();
            for (int i = 0; i < fib.Wires.Count; i++)
            {
                Wire w = fib.Wires[i] as Wire;
                if (address.Equals(w.One.Address.ToString()))
                {
                    tmp.Add(w.One.Port);
                }
                if (address.Equals(w.Two.Address.ToString()))
                {
                    tmp.Add(w.Two.Port);
                }
            }
            return tmp;
        }

        public Client(string ip)
        {
            address = ip;
            ArrayList ports = findingPorts();
            sockets = new ArrayList();
            cloudEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8000);
            for (int i = 0; i < ports.Count; i++)
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(address), Convert.ToInt32(ports[i]));
                Socket tmp = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                tmp.Bind(endPoint);
                tmp.BeginConnect(cloudEP, new AsyncCallback(ConnectCallback), tmp);
                sockets.Add(tmp);
            }
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
                    for (int i = 0; i < sockets.Count; i++)
                    {
                        Socket s = sockets[i] as Socket;
                        Receive(s);
                    }
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

        public void Send(String data, String targetIP)
        {
            Socket s = findTarget(targetIP);
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            s.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), s);
            sendDone.WaitOne();
        }

        private Socket findTarget(String target)
        {
            for (int i = 0; i < fib.Wires.Count; i++)
            {
                Wire w = fib.Wires[i] as Wire;
                if ((address.Equals(w.One.Address.ToString()) && target.Equals(w.Two.Address.ToString())) ||
                    (address.Equals(w.Two.Address.ToString()) && target.Equals(w.One.Address.ToString())))
                {
                    int port;
                    if (address.Equals(w.One.Address.ToString())) port = w.One.Port;
                    else port = w.Two.Port;
                    IPEndPoint iep = new IPEndPoint(IPAddress.Parse(address), port);
                    for (int j = 0; j < sockets.Count; j++)
                    {
                        Socket so = sockets[j] as Socket;
                        if (so.LocalEndPoint.Equals(iep)) return so;
                    }                    
                }
            }
            return null;
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

        public void Receive(Socket soc)
        {
            try
            {
                // Create the state object.
                StateObject state = new StateObject();
                state.workSocket = soc;
                //response = String.Empty;
                // Begin receiving the data from the remote device.
                soc.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
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
