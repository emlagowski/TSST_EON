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

        private ManualResetEvent connectDone = new ManualResetEvent(false);
        public ManualResetEvent allReceive = new ManualResetEvent(false);
        private ManualResetEvent receiveDone = new ManualResetEvent(false);
        private ManualResetEvent sendDone = new ManualResetEvent(false);

        private String response = String.Empty;

        public String logName;
        XmlDocument xmlLog;
        XmlNode rootNodeLog;


        public Client(String ip)
        {
            logName= ip + ".xml";
            xmlLog = new XmlDocument();
            rootNodeLog = xmlLog.CreateElement("user-log");
            xmlLog.AppendChild(rootNodeLog);
            localAddress = ip;
            localEndPoint = new IPEndPoint(IPAddress.Parse(localAddress), 7000);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(localEndPoint);
        }

        void addLog(String t, String f_ip, String t_ip, String d)
        {
            XmlNode userNode = xmlLog.CreateElement("event");
            XmlAttribute type = xmlLog.CreateAttribute("type");
            XmlAttribute from = xmlLog.CreateAttribute("from");
            XmlAttribute to = xmlLog.CreateAttribute("to");
            type.Value = t;
            from.Value = f_ip;
            to.Value = t_ip;
            userNode.Attributes.Append(type);
            userNode.Attributes.Append(from);
            userNode.Attributes.Append(to);
            userNode.InnerText = d;
            rootNodeLog.AppendChild(userNode);
            xmlLog.Save(logName);
        }

        public void connect(String router_ip)
        {
            remoteAddress = router_ip;
            remoteEndPoint = new IPEndPoint(IPAddress.Parse(remoteAddress), 7000);
            Thread t = new Thread(Run);
            t.Start();
            socket.BeginConnect(remoteEndPoint, new AsyncCallback(ConnectCallback), socket);
            connectDone.WaitOne();
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.
                client.EndConnect(ar);

                Console.WriteLine("{0} User connected to {1} Router", client.LocalEndPoint.ToString(), client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.
                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

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

                state.dt = (ExtSrc.Data)formattor.Deserialize(ms);
                /*  //if (bytesRead > 0)
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
                  Console.WriteLine("User {0} Received '{1}'[{2} bytes] from router {3}.", client.LocalEndPoint.ToString(),
                          response, response.Length, client.RemoteEndPoint.ToString());*/
                receiveDone.Set();
                allReceive.Set();
                Console.WriteLine("User {0} Received '{1}'[{2} bytes] from router {3}.", client.LocalEndPoint.ToString(),
                          state.dt.ToString(), bytesRead, client.RemoteEndPoint.ToString());
                addLog("Receive", client.RemoteEndPoint.ToString(), client.LocalEndPoint.ToString(), state.dt.ToString());

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void Send(String targetIP, int bandwidth, String data, int id)
        {
            // Convert the string data to byte data using ASCII encoding.
          /*  byte[] byteData = Encoding.ASCII.GetBytes(targetIP+"|"+data);

            // Begin sending the data to the remote device.
            socket.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), socket);
            sendDone.WaitOne();*/

            MemoryStream fs = new MemoryStream();

            BinaryFormatter formatter = new BinaryFormatter();

            formatter.Serialize(fs, new ExtSrc.Data(targetIP, bandwidth, data, id));

            byte[] buffer = fs.ToArray();



            // Begin sending the data to the remote device.
            socket.BeginSend(buffer, 0, buffer.Length, 0,
                new AsyncCallback(SendCallback), socket);
            addLog("Send", localAddress, targetIP, data);
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
                Console.WriteLine("Sent {0} bytes from {1} to router {2}.", bytesSent, client.LocalEndPoint.ToString(), client.RemoteEndPoint.ToString());
                // Signal that all bytes have been sent.
                sendDone.Set();
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
        public const int BufferSize = 1024*5;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public ExtSrc.Data dt;
    }
}
