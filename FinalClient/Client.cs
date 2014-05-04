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
using System.Runtime.Serialization.Formatters.Binary;

namespace FinalClient
{
    public class Client
    {
        public Signaling signaling;
        public static XmlDocument wires;
        public static FIB fib;
        XmlDocument xmlLog, xmlWires;
        XmlNode rootNodeLog, rootNodeWires;
        public String logName, wiresName;
        public String address;
        IPEndPoint cloudEP, clientEP;
        Socket clientSocket, client; // clientSocket is just for listening
        ArrayList sockets;
        private String response = String.Empty;
        UnexpectedFIB unFib;

        private ManualResetEvent connectDone = new ManualResetEvent(false);
        private ManualResetEvent sendDone = new ManualResetEvent(false);
        private ManualResetEvent receiveDone = new ManualResetEvent(false);
        public ManualResetEvent allDone = new ManualResetEvent(false);
        public ManualResetEvent allReceive = new ManualResetEvent(false);


        public Client(string ip)
        {
            address = ip;
            unFib = new UnexpectedFIB();
            readUnFIB();
            signaling = new Signaling();
            xmlLog = new XmlDocument();
            rootNodeLog = xmlLog.CreateElement("router-log");
            xmlLog.AppendChild(rootNodeLog);
            xmlWires = new XmlDocument();
            rootNodeWires = xmlWires.CreateElement("wires");
            xmlWires.AppendChild(rootNodeWires);
            logName = address + ".xml";
            wiresName = address + ".Wires.xml";
            ArrayList ports = findingPorts();
            sockets = new ArrayList();
            cloudEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8000);
            clientEP = new IPEndPoint(IPAddress.Parse(address), 7000);
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocket.Bind(clientEP);
            new Thread(WaitingForClient).Start();
            for (int i = 0; i < ports.Count; i++)
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(address), Convert.ToInt32(ports[i]));
                Socket tmp = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                tmp.Bind(endPoint);
                tmp.BeginConnect(cloudEP, new AsyncCallback(ConnectCallback), tmp);
                sockets.Add(tmp);
                String samp = findTargetIP(endPoint);
                addWires(samp);
            }
            connectDone.WaitOne();
            Thread t = new Thread(Run);
            t.Start();
        }


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
                    reader.MoveToNextAttribute();
                    string id = reader.Value;

                    fib.add(new Wire(new IPEndPoint(IPAddress.Parse(f_ip), Convert.ToInt32(f_port)),
                                        new IPEndPoint(IPAddress.Parse(s_ip), Convert.ToInt32(s_port)),
                                        Convert.ToInt32(id)));
                }
            }
        }

        public void readUnFIB()
        {
            String xmlString = File.ReadAllText("unexpectedFIB.xml");
            using (XmlReader reader = XmlReader.Create(new StringReader(xmlString)))
            {
                reader.ReadToFollowing(String.Concat("w",address.Replace(".","")));
                while (reader.ReadToFollowing("wire"))
                {
                    reader.MoveToFirstAttribute();
                    string first = reader.Value;
                    reader.MoveToNextAttribute();
                    string second = reader.Value;
                    //czy on czyta tylko w calejklamrze address??
                    unFib.addNext(first, second);
                }
            }
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

        void addWires(String _ip)
        {
            XmlNode userNode = xmlWires.CreateElement("wire");
            XmlAttribute ip = xmlWires.CreateAttribute("ip");
            ip.Value = _ip;
            userNode.Attributes.Append(ip);
            //userNode.InnerText = _ip;
            rootNodeWires.AppendChild(userNode);
            lock (xmlWires)
            {
                xmlWires.Save(wiresName);
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

        void WaitingForClient()
        {
            try
            {
                clientSocket.Listen(100);

                while (true)
                {
                    // Set the event to nonsignaled state.
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.
                    clientSocket.BeginAccept(new AsyncCallback(AcceptCallback), clientSocket);

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
            client = handler;
            addWires(handler.RemoteEndPoint.ToString());
            Console.WriteLine("User [{0}] {1} - {2} was added to sockets list", sockets.Count, handler.LocalEndPoint.ToString(), handler.RemoteEndPoint.ToString());

            // Create the state object.
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }

        public void ReadCallback(IAsyncResult ar)
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

                receiveDone.Set();
                allReceive.Set();
              //  Console.WriteLine("User {0} Received '{1}'[{2} bytes] from router {3}.", client.LocalEndPoint.ToString(),
               //           state.dt.ToString(), bytesRead, client.RemoteEndPoint.ToString());

                Send(state.dt);

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

        public void Send(ExtSrc.Data data)
        {
            // tu mozna wyrzucic targetIP z argumentow bo ip docelowe jest w pakiecie data
            Socket s = findTarget(data.EndAddress);
            // Convert the string data to byte data using ASCII encoding.
        /*    byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            s.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), s);
            sendDone.WaitOne();*/
            if (signaling.checkIfConnEstablished(data.connectionID))
            {
                MemoryStream fs = new MemoryStream();

                BinaryFormatter formatter = new BinaryFormatter();

                formatter.Serialize(fs, data);

                byte[] buffer = fs.ToArray();



                // Begin sending the data to the remote device.
                s.BeginSend(buffer, 0, buffer.Length, 0,
                    new AsyncCallback(SendCallback), s);
                sendDone.WaitOne();
            }
            else { Console.WriteLine("Connection must be established first at: {0}", address); }
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

            // jesli nie znalazlo bezposredniego polaczenia!
            String unexpectedIP = unFib.findTarget(target);
            Socket unSo = findTarget(unexpectedIP);
            return unSo;
        }

        private String findTargetIP(IPEndPoint iep)
        {
            for (int i = 0; i < fib.Wires.Count; i++)
            {
                Wire w = fib.Wires[i] as Wire;
                if (iep.Equals(w.Two))
                {
                    return w.One.Address.ToString();
                }
                if (iep.Equals(w.One))
                {
                    return w.Two.Address.ToString();
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
                addLog("Send", client.LocalEndPoint.ToString(), client.RemoteEndPoint.ToString(), "none");
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
                Boolean flag = false;
                // Retrieve the state object and the client socket 
                // from the asynchronous state object.
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar);
                BinaryFormatter formattor = new BinaryFormatter();

                MemoryStream ms = new MemoryStream(state.buffer);

                state.dt = (ExtSrc.Data)formattor.Deserialize(ms);

                if (state.dt.EndAddress.Equals(address)) flag = true;

          /*      //if (bytesRead > 0)
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
                addLog("Receive", client.LocalEndPoint.ToString(), client.RemoteEndPoint.ToString(), response);
           */
                receiveDone.Set();
                allReceive.Set();
                Console.WriteLine("Socket {0} Read '{1}'[{2} bytes] from socket {3}.", client.LocalEndPoint.ToString(),
                        state.dt.ToString(), bytesRead, client.RemoteEndPoint.ToString());
                    addLog("Receive", client.LocalEndPoint.ToString(), client.RemoteEndPoint.ToString(), state.dt.ToString());
                if (!flag) Send(state.dt);               

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
       // public StringBuilder sb = new StringBuilder();
        public ExtSrc.Data dt;
    }
}
