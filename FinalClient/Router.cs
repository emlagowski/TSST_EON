using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        public RouterForm RouterForm { get; set; }
        public int ID { get; set; }
        public ExtSrc.PhysicalWires localPhysicalWires { get; set; }
        public String address;
        protected IPEndPoint cloudEP, clientEP;
        protected Socket clientSocket, client; // clientSocket is just for listening
        protected Socket AgentOnline;
        protected Boolean IsListening = true;
        protected String response = String.Empty;
        public ExtSrc.FrequencySlotSwitchingTable freqSlotSwitchingTable { get; set; }
        public ExtSrc.ClientConnectionsTable TOclientConnectionsTable { get; set; }
        public ExtSrc.ClientConnectionsTable FROMclientConnectionsTable { get; set; }
        public Dictionary<int, ClientSocket> clientSocketDictionary { get; set; }
        public List<KeyValuePair<String, ExtSrc.DataAndID>> waitingMessages { get; set; }
        public List<UniqueConnection> UniqueConnections { get; set; }

        public class UniqueConnection
        {
            public String UniqueKey { get; set; }
            public String AddressA { get; set; }
            public String AddressB { get; set; }
            public int[] WireAndFsu { get; set; }
            public bool isOnline { get; set; }
        }

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
            localPhysicalWires = new ExtSrc.PhysicalWires();
            readLocalPhysicalWires();
        }

        public void initialize()
        {
            observers = new List<Observer>();
            freqSlotSwitchingTable = new ExtSrc.FrequencySlotSwitchingTable();
            waitingMessages = new List<KeyValuePair<string, DataAndID>>();
            UniqueConnections = new List<UniqueConnection>();
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
            
            new Thread(Run).Start();

            agentLocalEP = new IPEndPoint(IPAddress.Parse(address), 6666);
            agentEP = new IPEndPoint(IPAddress.Parse("127.6.6.6"), 6666);
            agentSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            agentSocket.Bind(agentLocalEP);
            agentSocket.ReceiveBufferSize = 1024*100;

            agentSocket.BeginConnect(agentEP, AgentConnectCallback, agentSocket);
            agentConnectDone.WaitOne();


            new Thread(agentRun).Start();

            new Thread(delegate()
            {
                while (true)
                {
                    if (UniqueConnections.Count(x => x.isOnline)!=0 && waitingMessages.Count!=0)
                    {
                        var list = UniqueConnections.Where(x => x.isOnline).ToList();
                        foreach (var uc in list)
                        {
                            var tmpList = new List<KeyValuePair<string, DataAndID>>();
                            foreach (var wm in waitingMessages)
                            {
                                if (!uc.UniqueKey.Equals(wm.Key)) continue;
                                var d = wm.Value;
                                //var route = FROMclientConnectionsTable.findRoute(d.ID);
                                var route = UniqueConnections.First(x => x.UniqueKey.Equals(wm.Key)).WireAndFsu;
                                if (route != null)
                                    // wysylamy wiadomosc z kolejki
                                    Send(d.data, route, d.ID);
                                else
                                    // pomimo powolenie od agenta na wysylanie(co ma byc tylko gdy polaczenie zestawione) placzenie nie znalezione
                                    Console.WriteLine("Couldn't find route but got permission to send from NMS (Message Deleted)");

                                // niezaleznie od powodzenia usun wiadomosc z listy( na podstawie kodu od agenta
                                tmpList.Add(wm);
                            }
                            tmpList.ForEach(x=>waitingMessages.Remove(x));
                        }
                    }
                    Thread.Sleep(500);
                }
            }).Start();

            ID = Int32.Parse(address.Substring(address.Length - 1, 1));
            List<DijkstraData> wiresIds = new List<DijkstraData>();
            foreach (NewWire nw in localPhysicalWires.Wires)
            {
                wiresIds.Add(new DijkstraData(ID, nw.ID, nw.distance));
            }
            AgentSend(new ExtSrc.AgentData(ExtSrc.AgentComProtocol.REGISTER, wiresIds){ originatingAddress = address});
            RouterForm.Bind();
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

        void Run()
        {
            Thread.Sleep(10000);
            try
            {
                while (IsListening)
                {
                    allReceive.Reset();                 
                    foreach (var wire in localPhysicalWires.Wires)
                    {
                        foreach (var unit in wire.FrequencySlotUnitList)
                        {
                            ReceiveFromCloud(unit.socket);
                        }
                    }
                    Console.WriteLine("Router is ready.");
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
                    Console.WriteLine("Waiting for client.");
                    // Wait until a connection is made before continuing.
                    allDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
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
            //client = handler;
            //addWires(handler.RemoteEndPoint.ToString());
            //Console.WriteLine("User [{0}] {1} - {2} was added to sockets list", clientSocketDictionary.Count, handler.LocalEndPoint.ToString(), handler.RemoteEndPoint.ToString());
            Console.WriteLine("Client [{0}] {1} was connected.", clientSocketDictionary.Count+1, IpToString(handler.RemoteEndPoint));
            AgentSend(new AgentData(    AgentComProtocol.REGISTER_CLIENT, 
                                        address,IpToString(handler.RemoteEndPoint)));
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
            var state = (ClientStateObject)ar.AsyncState;
            var clientSocket = state.workSocket;
            var id = Int32.Parse(((IPEndPoint)clientSocket.RemoteEndPoint).Address.ToString().
                        Substring(((IPEndPoint)clientSocket.RemoteEndPoint).Address.ToString().Length - 1, 1));
            try
            {
                if (!IsListening) return;
                // Retrieve the state object and the client socket 
                // from the asynchronous state object.
                //ClientStateObject state = (ClientStateObject)ar.AsyncState;
                //Socket client = state.workSocket;

                // Read data from the remote device.
                int bytesRead = clientSocket.EndReceive(ar);
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
                String key;
                var uc = UniqueConnections.FirstOrDefault(w => w.AddressA.Equals(IpToString(clientSocket.RemoteEndPoint)) & w.AddressB.Equals(target));
                if (uc!=null && uc.isOnline)
                {
                    key = uc.UniqueKey;
                }
                else
                {
                    // todo
//                    key = generateUniqueKey();
//                    AgentSend(new ExtSrc.AgentData(ExtSrc.AgentComProtocol.SET_ROUTE_FOR_ME, address, IpToString(clientSocket.RemoteEndPoint), target, key, state.cdt.bandwidthNeeded));
//                    UniqueConnections.Add(new UniqueConnection()
//                    {
//                        UniqueKey = key,
//                        AddressA = (clientSocket.RemoteEndPoint as IPEndPoint).Address.ToString(),
//                        AddressB = target,
//                        isOnline = false
//                    });
                    clientSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, ReceiveFromClientCallback, state);//todo tego ma nie byc jesli odkomentujemy to wyzej, czyli routowanie
                    return;//todo tego ma nie byc jesli odkomentujemy to wyzej, czyli routowanie
                }
                
                
                // dodac na liste oczekujacych wyslan
                waitingMessages.Add(new KeyValuePair<string, DataAndID>(key, new ExtSrc.DataAndID(data,id)));
            }
            catch (SocketException e)
            {
                //todo klient sie rozłączył
                clientSocketDictionary.Remove(id);
                Console.WriteLine("Client disconnected.");
                AgentSend(new ExtSrc.AgentData(ExtSrc.AgentComProtocol.CLIENT_DISCONNECTED, address, IpToString(clientSocket.RemoteEndPoint)));
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            try
            {
                clientSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, ReceiveFromClientCallback, state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void Send(ExtSrc.Data data, int[] route)
        {
            var id = TOclientConnectionsTable.findClient(route[0], route[1]);
            ClientSocket clientSocketToSend;
            if (clientSocketDictionary.TryGetValue(id, out clientSocketToSend))
            {
                var fs = new MemoryStream();
                var formatter = new BinaryFormatter();

                //znajduje adres poczatkowy polaczenia zeby odbiorca wiedzial skad to jest
                var originatingAddr = String.Format("127.0.0." + FROMclientConnectionsTable.findClient(route[0], route[1]));
                var clientdata = new ClientData(data.bandwidthNeeded, data.info, originatingAddr); 

                formatter.Serialize(fs, clientdata);
                var buffer = fs.ToArray();
                clientSocketToSend.socket.BeginSend(buffer, 0, buffer.Length, 0, SendCallback, clientSocketToSend.socket);
                sendDone.WaitOne();
            }
            else
            {                
                var sockets = localPhysicalWires.getSockets(route);
                var units = localPhysicalWires.getFrequencySlotUnits(route);
                
                var fs = new MemoryStream();
                var formatter = new BinaryFormatter();
                formatter.Serialize(fs, data);
                var buffer = fs.ToArray();

                // Begin sending the data to the remote device.
                foreach (var unit in units)
                {
                    unit.socket.BeginSend(buffer, 0, buffer.Length, 0, SendCallback, unit);
                    unit.sendDone.WaitOne();
                }
            }
        }

        public void Send(ExtSrc.Data data, int[] route, int fromID)
        {

            // ############################################ NEW START
            int id = TOclientConnectionsTable.findClient(route[0], route[1]);
            //todo co ten if?
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
                Console.WriteLine("S: {0} bytes from {1} to {2}.", bytesSent, IpToString(client.LocalEndPoint), IpToString(client.RemoteEndPoint));
                //lock (this)
               // addLog("Send", client.LocalEndPoint.ToString(), client.RemoteEndPoint.ToString(), "none");
                // Signal that all bytes have been sent.
                unit.sendDone.Set();
            }
            catch (Exception e)
            {
                //todo co to?
                try
                {
                    Socket client = (Socket)ar.AsyncState;
                    int bytesSent = client.EndSend(ar);
                    Console.WriteLine("S:{0} bytes from {1} to {2}.", bytesSent, IpToString(client.LocalEndPoint), IpToString(client.RemoteEndPoint));
                    sendDone.Set();

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        public string IpToString(EndPoint endPoint)
        {
            return (endPoint as IPEndPoint).Address.ToString();
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

                Console.WriteLine("R: {0} bytes from {1}", bytesRead, client.RemoteEndPoint);
               // String address = (client.LocalEndPoint as IPEndPoint).Address.ToString();
                String port = (client.LocalEndPoint as IPEndPoint).Port.ToString();
                int[] wireAndFreqSlotID = localPhysicalWires.getIDsbyPort(Int32.Parse(port));
                int[] route = freqSlotSwitchingTable.findRoute(wireAndFreqSlotID[0], wireAndFreqSlotID[1]);


                // ###### WYNALAZEK START
                // mialo sprawdzac, czy to skad przyzla wiadomosc 
                // to pierwszy FSU danego FS i tylko wtedy robic send, 
                // jesli to kolejne FSU to juz nie robic send bo pierwszy wyslal.
                Boolean canSend = false;
                foreach (var nw in localPhysicalWires.Wires)
                {
                    if (nw.ID == wireAndFreqSlotID[0])
                    {
                        FrequencySlot fs;
                        nw.FrequencySlotDictionary.TryGetValue(wireAndFreqSlotID[1], out fs);
                        if (fs != null && fs.FSUList.ElementAt(0).port == Int32.Parse(port)) canSend = true;
                    }
                }
                // ###### WYNALAZEK STOP

                if (route == null)
                {
                    route = wireAndFreqSlotID;
                }
                receiveDone.Set();
                allReceive.Set();
//                Console.WriteLine("Socket {0} Read '{1}'[{2} bytes] from socket {3}.", client.LocalEndPoint.ToString(),
//                        state.dt.ToString(), bytesRead, client.RemoteEndPoint.ToString());

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
            localPhysicalWires.Close();
            clientSocket.Close();
            agentSocket.Close();
            AgentOnline.Close();
            RouterForm.Finish();
            System.Windows.Forms.Application.Exit();
            System.Environment.Exit(1);
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
                Console.WriteLine("Router is connected to NMS {0}", IpToString(client.RemoteEndPoint));

                // Signal that the connection has been made.
                agentConnectDone.Set();

                AgentOnline = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                AgentOnline.Bind(new IPEndPoint(IPAddress.Parse(address), 6667));
                AgentOnline.BeginConnect(new IPEndPoint(IPAddress.Parse("127.6.6.6"), 6667), new AsyncCallback(AgentOnlineRequests), AgentOnline);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void AgentOnlineRequests(IAsyncResult ar)
        {
            //var socket = ((Socket) ar.AsyncState);
            //socket.EndConnect(ar);
            try
            {
                var state = new StateObject();
                AgentOnline.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, result =>
                {
                    //Console.WriteLine("ROUTER RECEIVER ONLINE REQUEST AND SENDING RESPONSE");
                    var fs = new MemoryStream();
                    new BinaryFormatter().Serialize(fs, "ONLINE");
                    var buffer = fs.ToArray();
                    try
                    {
                        AgentOnline.BeginSend(buffer, 0, buffer.Length, 0, AgentOnlineRequests, agentSocket);
                    }
                    catch (ObjectDisposedException)
                    {
                        //todo 
                    }
                    catch (SocketException)
                    {
                        //todo
                    }
                }, state);
            }
            catch (SocketException)
            {
                //todo
            }
        }

        private void agentRun()
        {
            try
            {
                while (true)
                {
                    agentReceiveDone.Reset();
                    //Console.WriteLine("Waiting for data from AGENT...");
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
                agentSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,AgentReceiveCallback, state);
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
                Console.WriteLine("Agent Closed.");
            }
        }

        private void ProcessAgentData(ExtSrc.AgentData agentData)
        {
            int id1, id2;
            switch (agentData.message)
            {
                case ExtSrc.AgentComProtocol.ROUTE_FOR_U_EDGE:
                    //Console.WriteLine("ROUTE_FOR_U_EDGE");
                    int startfreqEdge=0;
                    if (agentData.startingFreq == -1)
                    {
                        startfreqEdge =
                            localPhysicalWires.getWireByID(agentData.wireID).findSpaceForFS(agentData.FSUCount);
                        if (startfreqEdge == -1 &
                        localPhysicalWires.getWireByID(agentData.wireID).IsPossibleToSlide(agentData.FSUCount))
                        {
                            localPhysicalWires.getWireByID(agentData.wireID).SlideDown();
                            startfreqEdge = localPhysicalWires.getWireByID(agentData.wireID).findSpaceForFS(agentData.FSUCount);
                        }
                    }
                    else
                        startfreqEdge = agentData.startingFreq;
                    //Console.WriteLine("startfreqEdge = "+ startfreqEdge);
                    id1 = localPhysicalWires.getWireByID(agentData.wireID).addFreqSlot(startfreqEdge, agentData.FSUCount, agentData.mod, agentData.FSid);
                    TOclientConnectionsTable.add(agentData.wireID, id1, agentData.clientSocketID);
                    var id = Int32.Parse(agentData.originatingAddress.Substring(agentData.originatingAddress.Length - 1, 1));
                    FROMclientConnectionsTable.add(agentData.wireID, id1, id);
                    var ucon = UniqueConnections.FirstOrDefault(x => x.UniqueKey.Equals(agentData.uniqueKey));
                    if (ucon == null)
                    {
                        ucon = new UniqueConnection()
                        {
                            AddressA = agentData.originatingAddress,
                            AddressB = agentData.targetAddress,
                            UniqueKey = agentData.uniqueKey,
                            isOnline = true
                        };
                        UniqueConnections.Add(ucon);
                    }
                    ucon.WireAndFsu = new int[] { agentData.wireID , id1};
                    //Console.WriteLine("ROUTE SET, EDGE");
                    AgentSend(new AgentData(ExtSrc.AgentComProtocol.CONNECTION_IS_ON, startfreqEdge, id1));
                    break;
               /* case ExtSrc.AgentComProtocol.ROUTE_FOR_U_EDGE_MANUAL:
                    //Console.WriteLine("ROUTE_FOR_U_EDGE");
                    int startfreqEdge = 0;
                    if (agentData.startingFreq == -1)
                    {
                        startfreqEdge =
                            localPhysicalWires.getWireByID(agentData.wireID).findSpaceForFS(agentData.FSUCount);
                        if (startfreqEdge == -1 &
                        localPhysicalWires.getWireByID(agentData.wireID).IsPossibleToSlide(agentData.FSUCount))
                        {
                            localPhysicalWires.getWireByID(agentData.wireID).SlideDown();
                            startfreqEdge = localPhysicalWires.getWireByID(agentData.wireID).findSpaceForFS(agentData.FSUCount);
                        }
                    }
                    else
                        startfreqEdge = agentData.startingFreq;
                    //Console.WriteLine("startfreqEdge = "+ startfreqEdge);
                    id1 = localPhysicalWires.getWireByID(agentData.wireID).addFreqSlot(startfreqEdge, agentData.FSUCount, agentData.mod);
                    TOclientConnectionsTable.add(agentData.wireID, id1, agentData.clientSocketID);
                    var id = Int32.Parse(agentData.originatingAddress.Substring(agentData.originatingAddress.Length - 1, 1));
                    FROMclientConnectionsTable.add(agentData.wireID, id1, id);
                    var ucon = UniqueConnections.FirstOrDefault(x => x.UniqueKey.Equals(agentData.uniqueKey));
                    if (ucon == null)
                    {
                        ucon = new UniqueConnection()
                        {
                            AddressA = agentData.originatingAddress,
                            AddressB = agentData.targetAddress,
                            UniqueKey = agentData.uniqueKey,
                            isOnline = true
                        };
                        UniqueConnections.Add(ucon);
                    }
                    ucon.WireAndFsu = new int[] { agentData.wireID, id1 };
                    //Console.WriteLine("ROUTE SET, EDGE");
                    AgentSend(new AgentData(ExtSrc.AgentComProtocol.CONNECTION_IS_ON, startfreqEdge, id1));
                    break;*/
                case ExtSrc.AgentComProtocol.ROUTE_FOR_U:
                    ///od agenta: fsucount, mod, firstwireid,secondwireid, startingfreq dla odbierajacego kabla bo juz obliczone w poprzednim roouterze
                    //Console.WriteLine("ROUTE_FOR_U");
                    //sprawdzanie 1 kabla
                    var startfreq1 = agentData.startingFreq;
                    if (!localPhysicalWires.getWireByID(agentData.firstWireID).IsTherePlace(startfreq1, agentData.lastFSUCount) || 
                        startfreq1 == -1 &
                        localPhysicalWires.getWireByID(agentData.firstWireID).IsPossibleToSlide(agentData.lastFSUCount))
                    {
                        localPhysicalWires.getWireByID(agentData.firstWireID).SlideDown();
                        startfreq1 = localPhysicalWires.getWireByID(agentData.firstWireID).findSpaceForFS(agentData.lastFSUCount);
                    }
                    //sprawdzanie 2 kabla
                    //todo sf2=sf1 na potrzeby pierwszego etapu do poprawy
                    var startfreq2 = startfreq1;//localPhysicalWires.getWireByID(agentData.secondWireID).findSpaceForFS(agentData.FSUCount);
                    if (startfreq2 == -1 &
                        localPhysicalWires.getWireByID(agentData.secondWireID).IsPossibleToSlide(agentData.FSUCount))
                    {
                        localPhysicalWires.getWireByID(agentData.secondWireID).SlideDown();
                        startfreq2 = localPhysicalWires.getWireByID(agentData.secondWireID).findSpaceForFS(agentData.FSUCount);
                    }
                    if (startfreq2 >= 0)
                    {
                        id1 = localPhysicalWires.getWireByID(agentData.firstWireID).addFreqSlot(startfreq1, agentData.lastFSUCount, agentData.lastMod, agentData.FSid);
                        id2 = localPhysicalWires.getWireByID(agentData.secondWireID).addFreqSlot(startfreq2, agentData.FSUCount, agentData.mod, agentData.FSid);
                        freqSlotSwitchingTable.add(agentData.firstWireID, id1, agentData.secondWireID, id2);
                        //Console.WriteLine("ROUTE SET, NOT EDGE");
                        AgentSend(new AgentData(ExtSrc.AgentComProtocol.CONNECTION_IS_ON, startfreq2, id2));
                    }
                    else
                    {
                        AgentSend(new AgentData(ExtSrc.AgentComProtocol.CONNECTION_UNAVAILABLE));
                        //Console.WriteLine("CONNECTION UNAVAILABLE, NOT EDGE");
                    }
                    break;
                case ExtSrc.AgentComProtocol.DISROUTE:
                    //Console.WriteLine("DISROUTE MSG ARRIVED, : " + address + " -> remove WIRE_ID : " + agentData.firstWireID + " FSid : " + agentData.FSid);
                    int[] inttab = new int[2];
                    inttab = freqSlotSwitchingTable.findRoute(agentData.firstWireID, agentData.FSid);
                    if (localPhysicalWires.getWireByID(agentData.firstWireID).removeFreqSlot(agentData.FSid) && 
                        localPhysicalWires.getWireByID(inttab[0]).removeFreqSlot(inttab[1]))
                    {
                        freqSlotSwitchingTable.remove(agentData.firstWireID, agentData.FSid, inttab[0], inttab[1]);
                        AgentSend(new AgentData(ExtSrc.AgentComProtocol.DISROUTE_IS_DONE));
                        //Console.WriteLine("DISROUTE DONE");
                    }else{
                        AgentSend(new AgentData(ExtSrc.AgentComProtocol.DISROUTE_ERROR));
                        //Console.WriteLine("DISROUTE ERROR!!!!");
                    }
                    break;
                case ExtSrc.AgentComProtocol.DISROUTE_EDGE:
                    //Console.WriteLine("DISROUTE_EDGE MSG ARRIVED, : " + address + " -> remove WIRE_ID : " + agentData.firstWireID + " FSid : " + agentData.FSid);
                    if (localPhysicalWires.getWireByID(agentData.firstWireID).removeFreqSlot(agentData.FSid))
                    {
                        TOclientConnectionsTable.remove(agentData.firstWireID, agentData.FSid);
                        FROMclientConnectionsTable.remove(agentData.firstWireID, agentData.FSid);
                        UniqueConnection uconnn = null;
                        foreach (var uniqueConnection in UniqueConnections)
                        {
                            if (uniqueConnection.UniqueKey.Equals(agentData.uniqueKey))
                                uconnn = uniqueConnection;
                                

                        }
                        if(uconnn != null)
                            UniqueConnections.Remove(uconnn);
                        AgentSend(new AgentData(ExtSrc.AgentComProtocol.DISROUTE_EDGE_IS_DONE));
                        //Console.WriteLine("DISROUTE EDGE DONE");
                    }
                    else
                    {
                        AgentSend(new AgentData(ExtSrc.AgentComProtocol.DISROUTE_ERROR_EDGE));
                        //Console.WriteLine("DISROUTE EDGE ERROR!!!!");
                    }
                    break;
                case ExtSrc.AgentComProtocol.U_CAN_SEND:
                    //Otrzymano pozwolenie na wyslanie wiadomosci z kolejki
                    //Console.WriteLine("U_CAN_SEND");
                    var uc = UniqueConnections.First(w => w.UniqueKey.Equals(agentData.uniqueKey));
                    if(uc!=null) uc.isOnline = true;
                    break;

                default:
                    //Console.WriteLine("Zły msg przybył");
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
                //Console.WriteLine("Sent {0} bytes to AGENT.", bytesSent);

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
