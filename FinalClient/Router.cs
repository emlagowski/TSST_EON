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

namespace Router
{
    using Observer = ExtSrc.Observers.Observer;
    using Subject = ExtSrc.Observers.Subject;
    using ExtSrc;

    public class Router : ExtSrc.Observers.Subject
    {
        //public Signaling signaling;
        //public AgentCommunication agentCom;
        //public static XmlDocument wires;
     //   public static ExtSrc.PhysicalWires globalPhysicalWires;
        public int ID { get; set; }
        public ExtSrc.PhysicalWires localPhysicalWires;
       // XmlDocument xmlLog, xmlWires;
        //XmlNode rootNodeLog, rootNodeWires;
       // public String logName, wiresName;
        public String address;
        protected IPEndPoint cloudEP, clientEP;
        protected Socket clientSocket, client; // clientSocket is just for listening
       // ArrayList sockets;
        protected Boolean IsListening = true;
        protected String response = String.Empty;
        //public ExtSrc.FIB fib { get; set; }
        public ExtSrc.FrequencySlotSwitchingTable freqSlotSwitchingTable { get; set; }
        public ExtSrc.ClientConnectionsTable TOclientConnectionsTable { get; set; }
        public ExtSrc.ClientConnectionsTable FROMclientConnectionsTable { get; set; }
        public Dictionary<int, ClientSocket> clientSocketDictionary { get; set; }
        public Dictionary<String, ExtSrc.DataAndID> waitingMessages { get; set; }

        //lista observerow
        protected List<Observer> observers;

        protected ManualResetEvent connectDone = new ManualResetEvent(false);
        protected ManualResetEvent sendDone = new ManualResetEvent(false);
        protected ManualResetEvent receiveDone = new ManualResetEvent(false);
        public ManualResetEvent allDone = new ManualResetEvent(false);
        public ManualResetEvent allReceive = new ManualResetEvent(false);

        Boolean isEdge = false;

        // ######################## AGENT vars
        IPEndPoint agentLocalEP, agentEP;
        public static Socket agentSocket;
        //  private ManualResetEvent allReceive = new ManualResetEvent(false);
        private ManualResetEvent agentConnectDone = new ManualResetEvent(false);
        private ManualResetEvent agentReceiveDone = new ManualResetEvent(false);
        private ManualResetEvent agentSendDone = new ManualResetEvent(false);
        
        

        public Router(string ip, Boolean isEdge)
        {
            if (isEdge) this.isEdge = true;

            address = ip;
            observers = new List<Observer>();
            freqSlotSwitchingTable = new ExtSrc.FrequencySlotSwitchingTable();
            waitingMessages = new Dictionary<String,ExtSrc.DataAndID>();
            //fib = new ExtSrc.FIB();
            localPhysicalWires = new ExtSrc.PhysicalWires();
            readLocalPhysicalWires();
            cloudEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8000);


            //--------client
            TOclientConnectionsTable = new ClientConnectionsTable();
            FROMclientConnectionsTable = new ClientConnectionsTable();
            clientSocketDictionary = new Dictionary<int, ClientSocket>();
            clientEP = new IPEndPoint(IPAddress.Parse(address), 7000);
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocket.Bind(clientEP);
            new Thread(WaitingForClient).Start();
            //-------------------------------

            //Dodaje kazda lambde jako observer routera, co pozwoli na latwiejsze notyfikowanie lambd o dzialaniu badz nie routera.
            foreach (ExtSrc.NewWire wire in localPhysicalWires.Wires)
            {
                registerObservers(wire);
                //Dodatkowo po zarejestrowaniu wszystkich lambd robie inicjalizacje socketow w lambdach
                wire.initWire(address, cloudEP);
            }
            Thread.Sleep(10000);
            Thread t = new Thread(Run);
            t.Start();

            //if (ip.Equals("127.0.1.2"))
            //{
            //    int id = localPhysicalWires.Wires[0].addFreqSlot(0, 2, Modulation.QPSK);
            //    int id1 = localPhysicalWires.Wires[1].addFreqSlot(0, 2, Modulation.QPSK);

            //    freqSlotSwitchingTable.add(1, id, 7, id1);
            //}
            //if (ip.Equals("127.0.1.5"))
            //{
            //    //int id = localPhysicalWires.Wires[0].addFreqSlot(0, 2, Modulation.QPSK);
            //    int id1 = localPhysicalWires.Wires[3].addFreqSlot(0, 2, Modulation.QPSK);

            //    clientConnectionsTable.add(7,id1,7);
            //    Console.WriteLine("");
            //   // freqSlotSwitchingTable.add(7, id1, 3, id);
            //}
            //if (ip.Equals("127.0.1.1"))
            //{
            //    int id = localPhysicalWires.Wires[0].addFreqSlot(0, 2, Modulation.QPSK);
            //    int id1 = localPhysicalWires.Wires[2].addFreqSlot(0, 2, Modulation.QPSK);

            //    freqSlotSwitchingTable.add(3, id1, 1, id);

            //    Thread.Sleep(20000);
            //    Send(new Data(1, "elo"), new int[] { 1, id });
            //}

            //if (ip.Equals("127.0.1.1"))
            //{
            //    int fiveTOsevenOUT = localPhysicalWires.Wires[0].addFreqSlot(0, 5, Modulation.QPSK);
            //    int fourTOeightOUT = localPhysicalWires.Wires[2].addFreqSlot(6, 10, Modulation.QPSK);
            //    //int id1 = localPhysicalWires.Wires[2].addFreqSlot(0, 5, Modulation.QPSK);
            //    //freqSlotSwitchingTable.add(3, id1, 1, id);
            //    FROMclientConnectionsTable.add(1, fiveTOsevenOUT, 5);
            //    TOclientConnectionsTable.add(1, fiveTOsevenOUT, 5);
            //    FROMclientConnectionsTable.add(3, fourTOeightOUT, 4);
            //    TOclientConnectionsTable.add(3, fourTOeightOUT, 4);

            //   // Thread.Sleep(10000);
            //   // Send(new Data(1, "elo"), new int[] { 1, id });
            //}
            //if (ip.Equals("127.0.1.2"))
            //{
            //    int fiveTOsevenIN = localPhysicalWires.Wires[0].addFreqSlot(0, 5, Modulation.QPSK);
            //    int fiveTOsevenOUT = localPhysicalWires.Wires[1].addFreqSlot(0, 5, Modulation.QPSK);
            //    freqSlotSwitchingTable.add(1, fiveTOsevenIN, 7, fiveTOsevenOUT);
            //}
            //if (ip.Equals("127.0.1.3"))
            //{
            //    int fiveTOsevenIN = localPhysicalWires.Wires[1].addFreqSlot(0, 5, Modulation.QPSK);
            //    int fiveTOsevenOUT = localPhysicalWires.Wires[2].addFreqSlot(0, 5, Modulation.QPSK);
            //    freqSlotSwitchingTable.add(4, fiveTOsevenIN, 5, fiveTOsevenOUT);

            //    int fourTOeightIN = localPhysicalWires.Wires[1].addFreqSlot(6, 10, Modulation.QPSK);
            //    TOclientConnectionsTable.add(4, fourTOeightIN, 8);
            //    FROMclientConnectionsTable.add(4, fourTOeightIN, 8);
            //}
            //if (ip.Equals("127.0.1.4"))
            //{
            //    int fiveTOsevenIN = localPhysicalWires.Wires[0].addFreqSlot(0, 5, Modulation.QPSK);
            //    //int id1 = localPhysicalWires.Wires[1].addFreqSlot(0, 2, Modulation.QPSK);
            //    //freqSlotSwitchingTable.add(1, id, 7, id1);
            //    TOclientConnectionsTable.add(5, fiveTOsevenIN, 7);
            //    FROMclientConnectionsTable.add(5, fiveTOsevenIN, 7);

            //}
            //if (ip.Equals("127.0.1.5"))
            //{
            //    int fiveTOsevenOUT = localPhysicalWires.Wires[1].addFreqSlot(0, 5, Modulation.QPSK);
            //    int fiveTOsevenIN = localPhysicalWires.Wires[3].addFreqSlot(0, 5, Modulation.QPSK);
            //    int fourTOeightOUT = localPhysicalWires.Wires[1].addFreqSlot(6, 10, Modulation.QPSK);
            //    int fourTOeightIN = localPhysicalWires.Wires[0].addFreqSlot(6, 10, Modulation.QPSK);
            //    freqSlotSwitchingTable.add(7, fiveTOsevenIN, 4, fiveTOsevenOUT);
            //    freqSlotSwitchingTable.add(4, fourTOeightOUT, 3, fourTOeightIN);
            //    //clientConnectionsTable.add(7, id1, 7);
            //    //Console.WriteLine("");
            //}

            agentLocalEP = new IPEndPoint(IPAddress.Parse(ip), 6666);
            agentEP = new IPEndPoint(IPAddress.Parse("127.6.6.6"), 6666);
            agentSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            agentSocket.Bind(agentLocalEP);
            agentSocket.ReceiveBufferSize = 1024 * 100;

            agentSocket.BeginConnect(agentEP,
                    new AsyncCallback(AgentConnectCallback), agentSocket);
            agentConnectDone.WaitOne();

            Thread agentThread = new Thread(agentRun);
            agentThread.Start();


            ID = Int32.Parse(ip.Substring(ip.Length - 1, 1));
            List<DijkstraData> wiresIds = new List<DijkstraData>();
            foreach(NewWire nw in localPhysicalWires.Wires)
            {
                wiresIds.Add(new DijkstraData(ID, nw.ID, nw.distance));
            }
            AgentSend(new ExtSrc.AgentData(ExtSrc.AgentComProtocol.REGISTER, wiresIds));
        }



        private void readLocalPhysicalWires()
        {
            String xmlString = File.ReadAllText(address+".xml");
            using (XmlReader reader = XmlReader.Create(new StringReader(xmlString)))
            {
                while (reader.ReadToFollowing("wire"))
                {
                    reader.MoveToFirstAttribute();
                    string wireID = reader.Value;
                    reader.MoveToNextAttribute();
                    string wireDistance = reader.Value;
                    reader.MoveToNextAttribute();
                    string maxFreqSlots = reader.Value;
                    reader.MoveToNextAttribute();
                    string portPrefix = reader.Value;
                    reader.MoveToNextAttribute();
                    string spectralWidth = reader.Value;

                    ExtSrc.NewWire nw = new ExtSrc.NewWire(Int32.Parse(wireID), Int32.Parse(wireDistance), Int32.Parse(maxFreqSlots), Int32.Parse(spectralWidth));
                    //stworz nowy wire z otrzymanych danych i dodaj go na liste wires
                    for (int i = 0; i < Int32.Parse(maxFreqSlots); i++)
                    {
                        string port = String.Concat(new String[] { portPrefix, i.ToString() });
                        ExtSrc.FrequencySlotUnit freqslotunit = new ExtSrc.FrequencySlotUnit(Int32.Parse(port), i);
                        nw.FrequencySlotUnitList.Add( freqslotunit);
                        //stworz nową lambde z otrzymanych danych i dodaj ją na liste lambd ostatnio utworzonego wire
                    }
                    localPhysicalWires.add(nw);                 
                }
            }
        }

        //public void readFIB() // now FIB
        //{
        //    String xmlString = File.ReadAllText(address+".FIB.xml");
        //    using (XmlReader reader = XmlReader.Create(new StringReader(xmlString)))
        //    {
        //        while (reader.ReadToFollowing("wire"))
        //        {
        //            reader.MoveToFirstAttribute();
        //            string first = reader.Value;
        //            reader.MoveToNextAttribute();
        //            string second = reader.Value;
        //            //czy on czyta tylko w calejklamrze address??
        //            fib.addNext(first, second);
        //        }
        //    }
        //}
        //void addLog(String t, String f_ip, String t_ip, String d)
        //{
        //    lock (this)
        //    {

        //        XmlNode userNode = xmlLog.CreateElement("event");
        //        XmlAttribute type = xmlLog.CreateAttribute("type");
        //        XmlAttribute from = xmlLog.CreateAttribute("from");
        //        XmlAttribute to = xmlLog.CreateAttribute("to");
        //        type.Value = t;
        //        from.Value = f_ip;
        //        to.Value = t_ip;
        //        userNode.Attributes.Append(type);
        //        userNode.Attributes.Append(from);
        //        userNode.Attributes.Append(to);
        //        userNode.InnerText = d;
        //        rootNodeLog.AppendChild(userNode);
        //        xmlLog.Save(logName);

        //    }
        //}

        //void addWires(String _ip)
        //{
        //    XmlNode userNode = xmlWires.CreateElement("wire");
        //    XmlAttribute ip = xmlWires.CreateAttribute("ip");
        //    ip.Value = _ip;
        //    userNode.Attributes.Append(ip);
        //    //userNode.InnerText = _ip;
        //    rootNodeWires.AppendChild(userNode);
        //    lock (xmlWires)
        //    {
        //        xmlWires.Save(wiresName);
        //    }
        //}

        //private ArrayList findingPorts()
        //{
        //    ArrayList tmp = new ArrayList();
        //    for (int i = 0; i < globalPhysicalWires.Wires.Count; i++)
        //    {
        //        ExtSrc.Wire w = globalPhysicalWires.Wires[i] as ExtSrc.Wire;
        //        if (address.Equals(w.One.Address.ToString()))
        //        {
        //            tmp.Add(w.One.Port);
        //        }
        //        if (address.Equals(w.Two.Address.ToString()))
        //        {
        //            tmp.Add(w.Two.Port);
        //        }
        //    }
        //    return tmp;
        //}


        void Run()
        {
            try
            {
                while (IsListening)
                {
                    allReceive.Reset();                 
                    foreach (ExtSrc.NewWire wire in localPhysicalWires.Wires)
                    {
                        foreach (ExtSrc.FrequencySlotUnit unit in wire.FrequencySlotUnitList)
                        {
                            ReceiveFromCloud(unit.socket);
                        }
                    }
                    Console.WriteLine("WszySTkie LaMbDY NaSlUccHuJA FalA SuPEr!");
                    allReceive.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }


        //------------------client
        void WaitingForClient()
        {
            try
            {
                clientSocket.Listen(100);

               // while (IsListening)
                while(true)
                {
                    // Set the event to nonsignaled state.
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.
                    clientSocket.BeginAccept(new AsyncCallback(AcceptClientCallback), clientSocket);
                    Console.WriteLine("CZEKAM NA KLIENTa");
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
        //

        public void AcceptClientCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            allDone.Set();
            if (!IsListening) return;
            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = (Socket)listener.EndAccept(ar);
            //obsluga dla adresow od 0 do 9 dla 127.0.0.x
            int id = Int32.Parse(((IPEndPoint)handler.RemoteEndPoint).Address.ToString().Substring(((IPEndPoint)handler.RemoteEndPoint).Address.ToString().Length - 1, 1));
           // client = handler;
            //addWires(handler.RemoteEndPoint.ToString());
            Console.WriteLine("User [{0}] {1} - {2} was added to sockets list", clientSocketDictionary.Count, handler.LocalEndPoint.ToString(), handler.RemoteEndPoint.ToString());
            AgentSend(new AgentData(    AgentComProtocol.REGISTER_CLIENT, 
                                        address, 
                                        ((IPEndPoint)handler.RemoteEndPoint).Address.ToString()));
            // Create the state object.
            ClientStateObject state = new ClientStateObject();
            state.workSocket = handler;
            clientSocketDictionary.Add(id, new ClientSocket(id, handler));
            allReceive.Set();
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
              new AsyncCallback(ReceiveFromClientCallback), state);
        }

        public void ReceiveFromClientCallback(IAsyncResult ar)
        {
            try
            {
                if (!IsListening) return;
                // Retrieve the state object and the client socket 
                // from the asynchronous state object.
                ClientStateObject state = (ClientStateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar);
                BinaryFormatter formattor = new BinaryFormatter();

                MemoryStream ms = new MemoryStream(state.buffer);

                state.cdt = (ExtSrc.ClientData)formattor.Deserialize(ms);

                receiveDone.Set();
                allReceive.Set();
                //przepakowuje client data na data

                Data data = new Data(state.cdt.bandwidthNeeded, state.cdt.info);
                String target = state.cdt.EndAddress;
                ///
                /// TODO WYSLANIE ADRESU DOCELOWEGO DO AGENTA.
                /// AGENT MUSI POZESTAWIAC DROGE W ROUTERACH I ZWROCIC CZY UDALO SIE ZESTAWIC POLACZENIE
                /// I WTEDY MOZEMY DOPIEROWYSYLAC
                /// 
                /// 
                
                String key = generateUniqueKey();
                AgentSend(new ExtSrc.AgentData(ExtSrc.AgentComProtocol.SET_ROUTE_FOR_ME, address, ((IPEndPoint)client.RemoteEndPoint).Address.ToString(),target, key, state.cdt.bandwidthNeeded));

                int id = Int32.Parse(((IPEndPoint)client.RemoteEndPoint).Address.ToString().
                        Substring(((IPEndPoint)client.RemoteEndPoint).Address.ToString().Length - 1, 1));
                // dodac na liste oczekujacych wyslan
                waitingMessages.Add(key, new ExtSrc.DataAndID(data,id));               


                //  Console.WriteLine("User {0} Received '{1}'[{2} bytes] from router {3}.", client.LocalEndPoint.ToString(),
                //           state.dt.ToString(), bytesRead, client.RemoteEndPoint.ToString());            
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }


        }

        //private void ConnectCallback(IAsyncResult ar)
        //{
        //    try
        //    {
        //        if (!IsListening) return;
        //        // Retrieve the socket from the state object.
        //        Socket client = (Socket)ar.AsyncState;

        //        // Complete the connection.
        //        client.EndConnect(ar);

        //        Console.WriteLine("{0} Socket connected to {1}", client.LocalEndPoint.ToString(), client.RemoteEndPoint.ToString());

        //        // Signal that the connection has been made.
        //        connectDone.Set();
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.ToString());
        //    }
        //}

        public void Send(ExtSrc.Data data, int[] route)
        {
            
            // ############################################ NEW START
            int id = TOclientConnectionsTable.findClient(route[0], route[1]);
            ClientSocket client;
            if (clientSocketDictionary.TryGetValue(id, out client))
            {
                MemoryStream fs = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();

                //znajduje adres poczatkowy polaczenia zeby odbiorca wiedzial skad to jest
                String originatingAddr = String.Format("127.0.0." + FROMclientConnectionsTable.findClient(route[0], route[1]));
                ClientData clientdata = new ClientData(data.bandwidthNeeded, data.info, originatingAddr); 

                formatter.Serialize(fs, clientdata);
                byte[] buffer = fs.ToArray();
                client.socket.BeginSend(buffer, 0, buffer.Length, 0,
                        new AsyncCallback(SendCallback), client.socket);
                sendDone.WaitOne();
                return;
            }
            else
            {                
                List<Socket> sockets = localPhysicalWires.getSockets(route);
                List<FrequencySlotUnit> units = localPhysicalWires.getFrequencySlotUnits(route);
                
                MemoryStream fs = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fs, data);
                byte[] buffer = fs.ToArray();

                // Begin sending the data to the remote device.
                foreach (FrequencySlotUnit unit in units)
                {
                    //unit.socket
                    unit.socket.BeginSend(buffer, 0, buffer.Length, 0,
                        new AsyncCallback(SendCallback), unit);
                    unit.sendDone.WaitOne();
                }
            }
            // ############################################ NEW END
        }

        public void Send(ExtSrc.Data data, int[] route, int fromID)
        {

            // ############################################ NEW START
            int id = TOclientConnectionsTable.findClient(route[0], route[1]);
            
            ClientSocket client;
            if (id != fromID && clientSocketDictionary.TryGetValue(id, out client))
            {
                MemoryStream fs = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();

                //znajduje adres poczatkowy polaczenia zeby odbiorca wiedzial skad to jest
                String originatingAddr = String.Format("127.0.0." + FROMclientConnectionsTable.findClient(route[0], route[1]));
                ClientData clientdata = new ClientData(data.bandwidthNeeded, data.info, originatingAddr);

                formatter.Serialize(fs, clientdata);
                byte[] buffer = fs.ToArray();
                client.socket.BeginSend(buffer, 0, buffer.Length, 0,
                        new AsyncCallback(SendCallback), client.socket);
                sendDone.WaitOne();
                return;
            }
            else
            {
                List<Socket> sockets = localPhysicalWires.getSockets(route);
                List<FrequencySlotUnit> units = localPhysicalWires.getFrequencySlotUnits(route);

                MemoryStream fs = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fs, data);
                byte[] buffer = fs.ToArray();

                // Begin sending the data to the remote device.
                foreach (FrequencySlotUnit unit in units)
                {
                    //unit.socket
                    unit.socket.BeginSend(buffer, 0, buffer.Length, 0,
                        new AsyncCallback(SendCallback), unit);
                    unit.sendDone.WaitOne();
                }
            }
            // ############################################ NEW END
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                if (!IsListening) return;
                // Retrieve the socket from the state object.
                //Socket client = (Socket)ar.AsyncState;

                FrequencySlotUnit unit = (FrequencySlotUnit)ar.AsyncState;
                Socket client = unit.socket;
                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes from {1} to server {2}.", bytesSent, client.LocalEndPoint.ToString(), client.RemoteEndPoint.ToString());
                //lock (this)
               // addLog("Send", client.LocalEndPoint.ToString(), client.RemoteEndPoint.ToString(), "none");
                // Signal that all bytes have been sent.
                unit.sendDone.Set();
            }
            catch (Exception e)
            {
                try
                {
                    Socket client = (Socket)ar.AsyncState;
                    int bytesSent = client.EndSend(ar);
                    Console.WriteLine("Sent {0} bytes from {1} to server {2}.", bytesSent, client.LocalEndPoint.ToString(), client.RemoteEndPoint.ToString());
                    sendDone.Set();

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        public void ReceiveFromCloud(Socket soc)
        {
            try
            {
                //Console.WriteLine("Recieve method from Base Router Class.");
                // Create the state object.
                StateObject state = new StateObject();
                state.workSocket = soc;
                //response = String.Empty;
                // Begin receiving the data from the remote device.
                soc.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveFromCloudCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }   

        private void ReceiveFromCloudCallback(IAsyncResult ar)
        {
            try
            {
                // Console.WriteLine("Recieve Callback from Base Router Class.");
                if (!IsListening) return;
                // Boolean flag = false;
                // Retrieve the state object and the client socket 
                // from the asynchronous state object.
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;
                // adres potrzebny do indentyfikowania lambdy i socketu
                
                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar);
                BinaryFormatter formattor = new BinaryFormatter();

                MemoryStream ms = new MemoryStream(state.buffer);

                state.dt = (ExtSrc.Data)formattor.Deserialize(ms);

                Console.WriteLine("MSG :" + state.dt.info);
               // String address = (client.LocalEndPoint as IPEndPoint).Address.ToString();
                String port = (client.LocalEndPoint as IPEndPoint).Port.ToString();
                int[] wireAndFreqSlotID = localPhysicalWires.getIDsbyPort(Int32.Parse(port));
                int[] route = freqSlotSwitchingTable.findRoute(wireAndFreqSlotID[0], wireAndFreqSlotID[1]);


                // ###### WYNALAZEK START
                // mialo sprawdzac, czy to skad przyzla wiadomosc 
                // to pierwszy FSU danego FS i tylko wtedy robic send, 
                // jesli to kolejne FSU to juz nie robic send bo pierwszy wyslal.
                Boolean canSend = false;
                foreach (NewWire nw in localPhysicalWires.Wires)
                {
                    if (nw.ID == wireAndFreqSlotID[0])
                    {
                        FrequencySlot fs;
                        nw.FrequencySlotDictionary.TryGetValue(wireAndFreqSlotID[1], out fs);
                        if (fs.FSUList.ElementAt(0).port == Int32.Parse(port)) canSend = true;

                    }
                }
                // ###### WYNALAZEK STOP

                if (route == null)
                {
                    route = wireAndFreqSlotID;
                }
                receiveDone.Set();
                allReceive.Set();
                Console.WriteLine("Socket {0} Read '{1}'[{2} bytes] from socket {3}.", client.LocalEndPoint.ToString(),
                        state.dt.ToString(), bytesRead, client.RemoteEndPoint.ToString());

                if (canSend)
                {
                    Send(state.dt, route);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        internal void closing()
        {
            IsListening = false;
            notifyObservers();
            //clientSocket.Shutdown(SocketShutdown.Send);            
          //  this.clientSocket.Close();

            if (client != null)
            {
                //client.Shutdown(SocketShutdown.Send);
                this.client.Close();
            }

            Console.WriteLine("Closing.");
            System.Windows.Forms.Application.Exit();

        }

        private void AgentConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.
                lock (this)
                {
                    client.EndConnect(ar);
                }
                Console.WriteLine("SIGNALING: {0} Socket connected to {1}", client.LocalEndPoint.ToString(), client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.
                agentConnectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void agentRun()
        {
            try
            {
                while (true)
                {
                    agentReceiveDone.Reset();
                    Console.WriteLine("Waiting for data from AGENT...");
                    AgentReceive();
                    agentReceiveDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void AgentReceive()
        {
            try
            {
                // Create the state object.
                AgentStateObject state = new AgentStateObject();
                state.workSocket = agentSocket;
                //response = String.Empty;
                // Begin receiving the data from the remote device.
                agentSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(AgentReceiveCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void AgentReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket 
                // from the asynchronous state object.
                AgentStateObject state = (AgentStateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // to bardzo wazne przypisanie socketu, otrzymanego po zestawieniu polaczenia i nasluch ustawiany musi byc na tym sockecie!
                agentSocket = client;

                int bytesRead = client.EndReceive(ar);

                BinaryFormatter formattor = new BinaryFormatter();

                MemoryStream ms = new MemoryStream(state.buffer);

                state.ad = (ExtSrc.AgentData)formattor.Deserialize(ms);

                ProcessAgentData(state.ad);

                agentReceiveDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void ProcessAgentData(ExtSrc.AgentData agentData)
        {
            int id1, id2;
            switch (agentData.message)
            {
                case ExtSrc.AgentComProtocol.ROUTE_FOR_U_EDGE:
                    ///
                    Console.WriteLine("ROUTE_FOR_U_EDGE");
                    int startfreqEdge=0;
                    if (agentData.startingFreq == -1)
                        startfreqEdge = localPhysicalWires.getWireByID(agentData.wireID).findSpaceForFS(agentData.FSUCount);
                    else
                        startfreqEdge = agentData.startingFreq;
                    id1 = localPhysicalWires.getWireByID(agentData.wireID).addFreqSlot(startfreqEdge, agentData.FSUCount, agentData.mod);
                    TOclientConnectionsTable.add(agentData.wireID, id1, agentData.clientSocketID);
                    FROMclientConnectionsTable.add(agentData.wireID, id1, agentData.clientSocketID);
                    Console.WriteLine("ROUTE SET, EDGE");
                    AgentSend(new AgentData(ExtSrc.AgentComProtocol.CONNECTION_IS_ON, startfreqEdge));
                    break;
                case ExtSrc.AgentComProtocol.ROUTE_FOR_U:
                    ///od agenta: fsucount, mod, firstwireid,secondwireid, startingfreq dla odbierajacego kabla bo juz obliczone w poprzednim roouterze
                    Console.WriteLine("ROUTE_FOR_U");
                    int startfreq = localPhysicalWires.getWireByID(agentData.secondWireID).findSpaceForFS(agentData.FSUCount);
                    if(startfreq >= 0) {
                        id1 = localPhysicalWires.getWireByID(agentData.firstWireID).addFreqSlot(agentData.startingFreq, agentData.lastFSUCount, agentData.lastMod);
                        id2 = localPhysicalWires.getWireByID(agentData.secondWireID).addFreqSlot(startfreq, agentData.FSUCount, agentData.mod);
                        freqSlotSwitchingTable.add(agentData.firstWireID, id1, agentData.secondWireID, id2);
                        Console.WriteLine("ROUTE SET, NOT EDGE");
                        AgentSend(new AgentData(ExtSrc.AgentComProtocol.CONNECTION_IS_ON, startfreq));
                    } else
                        AgentSend(new AgentData(ExtSrc.AgentComProtocol.CONNECTION_UNAVAILABLE));
                    break;
                case ExtSrc.AgentComProtocol.DISROUTE:
                    break;
                case ExtSrc.AgentComProtocol.DISROUTE_EDGE:
                    break;
                case ExtSrc.AgentComProtocol.U_CAN_SEND:
                    //Otrzymano pozwolenie na wyslanie wiadomosci z kolejki
                    Console.WriteLine("U_CAN_SEND");
                    ExtSrc.DataAndID dataID;
                    if (waitingMessages.TryGetValue(agentData.uniqueKey, out dataID))
                    {
                        // znaleziono wiadomosc oczekujaca na liscie
                        int[] route = FROMclientConnectionsTable.findRoute(dataID.ID);
                        if(route != null)
                            // wysylamy wiadomosc z kolejki
                            Send(dataID.data, route, dataID.ID);
                        else
                            // pomimo powolenie od agenta na wysylanie(co ma byc tylko gdy polaczenie zestawione) placzenie nie znalezione
                            Console.WriteLine("Couldn't find route but got permission to send from NMS");
                        // niezaleznie od powodzenia usun wiadomosc z listy( na podstawie kodu od agenta
                        waitingMessages.Remove(agentData.uniqueKey);
                    } else
                        // nie znaleziono wiadomosci na podstawie klucza
                        Console.WriteLine("No waiting message for KEY from NMS");
                    break;

                default:
                    Console.WriteLine("Zły msg przybył");
                    break;
            }
        }

        public void AgentSend(ExtSrc.AgentData conn)
        {
            MemoryStream fs = new MemoryStream();

            BinaryFormatter formatter = new BinaryFormatter();

            formatter.Serialize(fs, conn);

            byte[] buffer = fs.ToArray();



            // Begin sending the data to the remote device.
            agentSocket.BeginSend(buffer, 0, buffer.Length, 0,
                new AsyncCallback(AgentSendCallback), agentSocket);
            agentSendDone.WaitOne();
        }

        private void AgentSendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to AGENT.", bytesSent);

                // Signal that all bytes have been sent.
                agentSendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private String generateUniqueKey(){
            Guid g = Guid.NewGuid();
            String str = Convert.ToBase64String(g.ToByteArray());
            str = str.Replace("=", "");
            str = str.Replace("+", "");
            str = str.Replace("/", "");
            return str;
        }

        public void registerObservers(Observer o)
        {
            observers.Add(o);
        }
        public void removeObservers(Observer o)
        {
            int i = observers.IndexOf(o);
            if (i >= 0)
                observers.RemoveAt(i);
        }
        public void notifyObservers()
        {
            foreach (Observer o in observers)
            {
                o.update();
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
    public class ClientStateObject
    {
        // Client socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024 * 5;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        // public StringBuilder sb = new StringBuilder();
        public ExtSrc.ClientData cdt;
    }
    public class AgentStateObject
    {
        // Client socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024 * 100;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];//nn
        public ExtSrc.AgentData ad { get; set; }
    }
}
