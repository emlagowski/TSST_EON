using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;
using System.Collections;
using System.Xml;
using System.Runtime.Serialization.Formatters.Binary;

namespace FinalServer
{
    public class Server
    {
        public Signaling signaling;
        IPEndPoint endPoint;
        Socket localSocket;
        ArrayList sockets;
        FIB fib;
        public ManualResetEvent allDone = new ManualResetEvent(false);
        String xmlFileName = "log.xml", xmlConnectionName = "connections.xml";
        XmlDocument xmlDoc, xmlDocConnection;
        XmlNode rootNode, rootNodeConnection;

        public Server(string ip, int port)
        {
            signaling = new Signaling();
            fib = new FIB();
            xmlDoc = new XmlDocument();
            rootNode = xmlDoc.CreateElement("cloud-log");
            xmlDocConnection = new XmlDocument();
            rootNodeConnection = xmlDocConnection.CreateElement("connections");
            xmlDoc.AppendChild(rootNode);
            xmlDocConnection.AppendChild(rootNodeConnection);
            sockets = new ArrayList();
            endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            localSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            localSocket.Bind(endPoint);
            Thread t = new Thread(Run);
            t.Start();
        }
        
        void addLog(String t, String f_ip, String t_ip, String d)
        {
            XmlNode userNode = xmlDoc.CreateElement("event");
            XmlAttribute type = xmlDoc.CreateAttribute("type");
            XmlAttribute from = xmlDoc.CreateAttribute("from");
            XmlAttribute to = xmlDoc.CreateAttribute("to");
            type.Value = t;
            from.Value = f_ip;
            to.Value = t_ip;
            userNode.Attributes.Append(type);
            userNode.Attributes.Append(from);
            userNode.Attributes.Append(to);
            userNode.InnerText = d;
            rootNode.AppendChild(userNode);
            lock(this)
            xmlDoc.Save(xmlFileName);
        }

        void addConnection(String _ip)
        {
            XmlNode userNode = xmlDocConnection.CreateElement("router");
            XmlAttribute ip = xmlDocConnection.CreateAttribute("ip");
            ip.Value = _ip;
            userNode.Attributes.Append(ip);
            //userNode.InnerText = _ip;
            rootNodeConnection.AppendChild(userNode);
            lock (xmlDocConnection)
            {
                xmlDocConnection.Save(xmlConnectionName);
            }
        }

        public XmlDocument Doc
        {
            get { return xmlDoc; }
        }

        public ArrayList Sockets
        {
            get { return sockets; }
        }

        public String[] EPoints
        {
            get 
            { 
                String[] tmp = new String[sockets.Count];
                for (int i = 0; i < sockets.Count; i++)
                {
                    Socket s = sockets[i] as Socket;
                    tmp[i] = s.RemoteEndPoint.ToString();
                }
                return tmp;
            }
        }

        void Run()
        {
            try
            {
                localSocket.Listen(100);

                while (true)
                {
                    // Set the event to nonsignaled state.
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.
                    Console.WriteLine("Waiting for a connection...");
                    localSocket.BeginAccept(new AsyncCallback(AcceptCallback), localSocket);

                    // Wait until a connection is made before continuing.
                    allDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();
        }

        public void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            allDone.Set();

            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            sockets.Add(handler);
            addConnection(handler.RemoteEndPoint.ToString());
            Console.WriteLine("Socket [{0}] {1} - {2} was added to sockets list", sockets.Count, handler.LocalEndPoint.ToString(), handler.RemoteEndPoint.ToString());

            // Create the state object.
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }

        public void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket. 
            int bytesRead = handler.EndReceive(ar);
            BinaryFormatter formattor = new BinaryFormatter();

            MemoryStream ms = new MemoryStream(state.buffer);

            state.dt = (ExtSrc.Data)formattor.Deserialize(ms);

            Console.WriteLine("Read '{0}'[{1} bytes] from socket {2}.",
                      state.dt.ToString(), bytesRead, IPAddress.Parse(((IPEndPoint)handler.RemoteEndPoint).Address.ToString()));
            addLog("Receive", handler.LocalEndPoint.ToString(), handler.RemoteEndPoint.ToString(), state.dt.ToString());

            Socket s = findTarget((IPEndPoint)handler.RemoteEndPoint);
            StateObject newState = new StateObject();
            newState.workSocket = handler;

            Send(s, state.dt);

            handler.BeginReceive(newState.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), newState);
           /* if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.
                state.sb.Append(Encoding.ASCII.GetString(
                    state.buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read 
                // more data.
                content = state.sb.ToString();
                if (content.IndexOf("<EOF>") > -1)
                {
                    // All the data has been read from the 
                    // client. Display it on the console.
                    Console.WriteLine("Read '{0}'[{1} bytes] from socket {2}.",
                       content, content.Length, IPAddress.Parse(((IPEndPoint)handler.RemoteEndPoint).Address.ToString()));
                    addLog("Receive", handler.LocalEndPoint.ToString(), handler.RemoteEndPoint.ToString(), content);
                    // Echo the data back to the client.
                    Socket s = findTarget((IPEndPoint)handler.RemoteEndPoint);
                    StateObject newState = new StateObject();
                    newState.workSocket = handler;

                    Send(s, content);
                    
                    handler.BeginReceive(newState.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReadCallback), newState);
                }
                else
                {
                    // Not all data received. Get more.
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                }
            }*/
        }

        private Socket findTarget(IPEndPoint iPEndPoint)
        {
            for (int i = 0; i < fib.Wires.Count; i++)
            {
                Wire w = fib.Wires[i] as Wire;
                if (iPEndPoint.Equals(w.One))
                {
                    for (int j = 0; j < sockets.Count; j++)
                    {
                        Socket so = sockets[j] as Socket;
                        if (so.RemoteEndPoint.Equals(w.Two)) return so;
                    }
                }
                if (iPEndPoint.Equals(w.Two))
                {
                    for (int j = 0; j < sockets.Count; j++)
                    {
                        Socket so = sockets[j] as Socket;
                        if (so.RemoteEndPoint.Equals(w.One)) return so;
                    }
                }
            }
            Console.WriteLine("Target was not found in the FIB.");
            return null;
        }

        private void Send(Socket handler, ExtSrc.Data data)
        {
           /* // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
            */
            IPEndPoint ep = handler.RemoteEndPoint as IPEndPoint;
           // if (signaling.checkIfConnEstablished(data.connectionID))
           // {
                MemoryStream fs = new MemoryStream();

                BinaryFormatter formatter = new BinaryFormatter();

                formatter.Serialize(fs, data);

                byte[] buffer = fs.ToArray();



                // Begin sending the data to the remote device.
                handler.BeginSend(buffer, 0, buffer.Length, 0,
                    new AsyncCallback(SendCallback), handler);

           // }
            //else { Console.WriteLine("Connection [ID:{0}] must be established first at SERVER to {1}", data.connectionID, ep.Address.ToString()); }
            
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client {1}.", bytesSent, IPAddress.Parse(((IPEndPoint)handler.RemoteEndPoint).Address.ToString()));
                addLog("Send", handler.LocalEndPoint.ToString(), handler.RemoteEndPoint.ToString(), "none");
                //handler.Shutdown(SocketShutdown.Both);
                //handler.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }

    public class FIB
    {
        ArrayList _wires;

        public ArrayList Wires
        {
            get { return _wires; }
        }

        public FIB()
        {
             _wires = new ArrayList();
           /* _wires.Add(new Wire(new IPEndPoint(IPAddress.Parse("127.0.0.10"), 8020), new IPEndPoint(IPAddress.Parse("127.0.0.20"), 8010)));
            _wires.Add(new Wire(new IPEndPoint(IPAddress.Parse("127.0.0.10"), 8030), new IPEndPoint(IPAddress.Parse("127.0.0.30"), 8010)));
            _wires.Add(new Wire(new IPEndPoint(IPAddress.Parse("127.0.0.10"), 8050), new IPEndPoint(IPAddress.Parse("127.0.0.50"), 8010)));
            _wires.Add(new Wire(new IPEndPoint(IPAddress.Parse("127.0.0.30"), 8050), new IPEndPoint(IPAddress.Parse("127.0.0.50"), 8030)));
            _wires.Add(new Wire(new IPEndPoint(IPAddress.Parse("127.0.0.30"), 8040), new IPEndPoint(IPAddress.Parse("127.0.0.40"), 8030)));
            _wires.Add(new Wire(new IPEndPoint(IPAddress.Parse("127.0.0.40"), 8050), new IPEndPoint(IPAddress.Parse("127.0.0.50"), 8040)));
           */ 
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
                    reader.MoveToNextAttribute();
                    string id = reader.Value;

                    _wires.Add(new Wire(new IPEndPoint(IPAddress.Parse(f_ip), Convert.ToInt32(f_port)),
                                        new IPEndPoint(IPAddress.Parse(s_ip), Convert.ToInt32(s_port)),
                                        Convert.ToInt32(id)));
                }
            }
        }
    }

    public class Wire
    {
        IPEndPoint _one, _two;
        int ID;

        public IPEndPoint One
        {
            get { return _one; }
        }

        public IPEndPoint Two
        {
            get { return _two; }
        }

        public Wire(IPEndPoint first, IPEndPoint second, int id)
        {
            _one = first;
            _two = second;
            ID = id;
        }
    }

    public class StateObject
    {
        // Client  socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024*5;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public ExtSrc.Data dt;
    }
}
