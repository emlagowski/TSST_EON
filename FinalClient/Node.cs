#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Xml;
using ExtSrc;
using ExtSrc.Observers;

#endregion

namespace Node
{
    public class Node : Subject, IDisposable
    {
        private const String CloudIp = "127.0.0.1";
        private String AgentIp = "127.6.6.6";
        // Cloud communication port
        private const int CloudPort = 8000;
        // Agent communication port
        private const int AgentPort = 6666;
        // Agent router-online-check port
        private const int AgentOnlinePort = 6667;
        // Domain listening port
        private const int DomainPort = 6668;
        public const int BandPerChar = 100;

        // Domain adress
        private String _domainAddress;

        // Node Form Gui Object
        public NodeForm NodeForm { get; set; }

        public bool Enabled { get; set; }

        // Node Id
        private int Id { get; set; }

        // Node physical wires object
        public ExtSrc.PhysicalWires LocalPhysicalWires { get; set; }

        // Route Table
        public ExtSrc.FrequencySlotSwitchingTable FreqSlotSwitchingTable { get; set; }

        // Messages that are waiting to be sent
        public List<KeyValuePair<int[], ExtSrc.DataAndID>> WaitingMsgs { get; set; }

        private List<UniqueConnection> UniqueConnections { get; set; }

        // History of messeges only for GUI purpose
        public List<KeyValuePair<string, Data>> MessageHistory { get; set; }

        // Node full IP address
        public String address;

        // Socket connected to Cloud of wires
        private IPEndPoint cloudEP;

        private Socket AgentOnline;

        // Is Node active/running ?
        private Boolean IsListening = true;

        // Observers list
        private List<Observer> observers;

        // Is this ClientNode?
        public Boolean isEdge = false;

        // Agent communication socket
        private Socket agentSocket;

        // Agent communication socket
        private Socket domainSocket;

        // Cloud communication event flags
        private ManualResetEvent sendDone = new ManualResetEvent(false);
        private ManualResetEvent receiveDone = new ManualResetEvent(false);
        private ManualResetEvent allDone = new ManualResetEvent(false);
        private ManualResetEvent allReceive = new ManualResetEvent(false);

        // Agent communication event flags
        private ManualResetEvent agentConnectDone = new ManualResetEvent(false);
        private ManualResetEvent agentReceiveDone = new ManualResetEvent(false);
        private ManualResetEvent agentSendDone = new ManualResetEvent(false);

        // Other domain communication event flags
        private ManualResetEvent domainConnectDone = new ManualResetEvent(false);
        private ManualResetEvent domainReceiveDone = new ManualResetEvent(false);
        private ManualResetEvent domainSendDone = new ManualResetEvent(false);

        public class UniqueConnection
        {
            public String UniqueKey { get; set; }
            public String AddressA { get; set; }
            public String AddressB { get; set; }
            public int[] WireAndFsu { get; set; }
            public bool isOnline { get; set; }
        }

        // ############################################################################################################
        // ####################### Constructor's and methods
        // ############################################################################################################

        public Node(string ip, Boolean isEdge, String agentIp)
        {
            this.isEdge = isEdge;
            AgentIp = agentIp;
            address = ip;
            Id = Int32.Parse(address.Substring(address.Length - 1, 1));
            cloudEP = new IPEndPoint(IPAddress.Parse(CloudIp), CloudPort);
            Enabled = true;

            // Initialize
            LocalPhysicalWires = new PhysicalWires();
            ReadLocalPhysicalWires();
            MessageHistory = new List<KeyValuePair<string, Data>>();
            WaitingMsgs = new List<KeyValuePair<int[], DataAndID>>();
            UniqueConnections = new List<UniqueConnection>();
            observers = new List<Observer>();
            FreqSlotSwitchingTable = new FrequencySlotSwitchingTable();
            agentSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            domainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void ConnectAndRun()
        {
            //Dodaje kazda lambde jako observer routera, co pozwoli na latwiejsze notyfikowanie lambd o dzialaniu badz nie routera.
            foreach (var wire in LocalPhysicalWires.Wires)
            {
                registerObservers(wire);
                //Dodatkowo po zarejestrowaniu wszystkich lambd robie inicjalizacje socketow w lambdach
                wire.initWire(address, cloudEP);
            }

            new Thread(Run).Start();

            var agentLocalEndPoint = new IPEndPoint(IPAddress.Parse(address), AgentPort);
            var agentRemoteEndPoint = new IPEndPoint(IPAddress.Parse(AgentIp), AgentPort);
            agentSocket.Bind(agentLocalEndPoint);
            agentSocket.ReceiveBufferSize = 1024 * 100;
            agentSocket.BeginConnect(agentRemoteEndPoint, AgentConnectCallback, agentSocket);
            agentConnectDone.WaitOne();

            new Thread(AgentRun).Start();

            new Thread(delegate()
            {
                while (true)
                {
                    if (WaitingMsgs.Count != 0)
                    {
                        var tmpList = new List<KeyValuePair<int[], DataAndID>>();
                        foreach (var keyValuePair in WaitingMsgs)
                        {
                            var uniqueConnection = UniqueConnections.FirstOrDefault(e => e.UniqueKey.Equals(keyValuePair.Value.uniqueKey));
                            if (uniqueConnection == null || !uniqueConnection.isOnline) continue;
                            Send(keyValuePair.Value.data, keyValuePair.Key, true);
                            tmpList.Add(keyValuePair);
                        }
                        tmpList.ForEach(x => WaitingMsgs.Remove(x));
                    }
                    Thread.Sleep(500);
                }
            }).Start();


            RegisterToAgent();

            NodeForm.Bind();
        }

        void ReadLocalPhysicalWires()
        {
            var xmlString = File.ReadAllText(address + ".xml");
            using (var reader = XmlReader.Create(new StringReader(xmlString)))
            {
                var idx = 1;
                while (reader.ReadToFollowing("wire"))
                {
                    reader.MoveToFirstAttribute();
                    var wireID = reader.Value;
                    reader.MoveToNextAttribute();
                    var wireDistance = reader.Value;
                    reader.MoveToNextAttribute();
                    var maxFreqSlots = reader.Value;
                    reader.MoveToNextAttribute();
                    var portPrefix = reader.Value;
                    reader.MoveToNextAttribute();
                    var spectralWidth = reader.Value;

                    //ExtSrc.NewWire nw = new ExtSrc.NewWire(Int32.Parse(wireID), Int32.Parse(wireDistance), Int32.Parse(maxFreqSlots), Int32.Parse(spectralWidth), Int32.Parse(portPrefix));
                    var nw = new ExtSrc.NewWire(idx, Int32.Parse(wireDistance), Int32.Parse(maxFreqSlots), Int32.Parse(spectralWidth), Int32.Parse(portPrefix));
                    idx++;
                    //stworz nowy wire z otrzymanych danych i dodaj go na liste wires
                    for (var i = 0; i < Int32.Parse(maxFreqSlots); i++)
                    {
                        var port = String.Concat(new String[] { portPrefix, i.ToString() });
                        var freqslotunit = new ExtSrc.FrequencySlotUnit(Int32.Parse(port), i);
                        nw.FrequencySlotUnitList.Add(freqslotunit);
                        //stworz nową lambde z otrzymanych danych i dodaj ją na liste lambd ostatnio utworzonego wire
                    }
                    LocalPhysicalWires.add(nw);
                }
            }
        }

        // ############################################################################################################
        // ####################### Cloud communication methods section
        // ############################################################################################################

        void Run()
        {
            Thread.Sleep(10000);
            try
            {
                while (IsListening)
                {
                    allReceive.Reset();
                    foreach (var wire in LocalPhysicalWires.Wires)
                    {
                        foreach (var unit in wire.FrequencySlotUnitList)
                        {
                            ReceiveFromCloud(unit.socket);
                        }
                    }
                    Log.i("Node is ready.");
                    allReceive.WaitOne();
                }
            }
            catch (Exception e)
            {
                //Log.d(e.ToString());
            }
        }

        void Send(ExtSrc.Data data, int[] route, bool isFirst = false)
        {
            int[] newRoute = FreqSlotSwitchingTable.findRoute(route[0], route[1]);
            if (newRoute == null || newRoute[0] != -1 && newRoute[1] != -1)
            {// router poczatkowy || napewno nie router koncowy

                var sockets = LocalPhysicalWires.getSockets(route);
                var units = LocalPhysicalWires.getFrequencySlotUnits(route);

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
                if (isFirst) MessageHistory.Add(new KeyValuePair<string, Data>("SENT", data));
            }
            else
            {
                Log.d("There is no connection.");
            }
        }

        void SendCallback(IAsyncResult ar)
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
                Log.d(String.Format("S: {0} bytes from {1} to {2}.", bytesSent, IpToString(client.LocalEndPoint), IpToString(client.RemoteEndPoint)));
                //lock (this)
                // addLog("Send", client.LocalEndPoint.ToString(), client.RemoteEndPoint.ToString(), "none");
                // Signal that all bytes have been sent.
                unit.sendDone.Set();
            }
            catch (Exception)
            {
                try
                {
                    Socket client = (Socket)ar.AsyncState;
                    int bytesSent = client.EndSend(ar);
                    Log.d(String.Format("S:{0} bytes from {1} to {2}.", bytesSent, IpToString(client.LocalEndPoint), IpToString(client.RemoteEndPoint)));
                    sendDone.Set();

                }
                catch (Exception ex)
                {
                    //Log.d(ex.ToString());
                }
            }
        }

        void ReceiveFromCloud(Socket soc)
        {
            try
            {
                //Log.d("Recieve method from Base Node Class.");
                // Create the state object.
                var state = new StateObject();
                state.WorkSocket = soc;
                //response = String.Empty;
                // Begin receiving the data from the remote device.
                soc.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveFromCloudCallback), state);
            }
            catch (Exception e)
            {
                //Log.d(e.ToString());
            }
        }

        void ReceiveFromCloudCallback(IAsyncResult ar)
        {
            try
            {
                //Log.d("Recieve Callback from Base Node Class.");
                if (!IsListening) return;
                // Boolean flag = false;
                // Retrieve the state object and the client socket 
                // from the asynchronous state object.
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.WorkSocket;
                // adres potrzebny do indentyfikowania lambdy i socketu

                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar);
                BinaryFormatter formattor = new BinaryFormatter();

                MemoryStream ms = new MemoryStream(state.buffer);

                state.Data = (ExtSrc.Data)formattor.Deserialize(ms);

                Log.d(String.Format("R: {0} bytes from {1}", bytesRead, client.RemoteEndPoint));
                // String address = (client.LocalEndPoint as IPEndPoint).Address.ToString();
                String port = (client.LocalEndPoint as IPEndPoint).Port.ToString();
                int[] wireAndFreqSlotID = LocalPhysicalWires.getIDsbyPort(Int32.Parse(port));
                if (wireAndFreqSlotID == null) return;
                int[] route = FreqSlotSwitchingTable.findRoute(wireAndFreqSlotID[0], wireAndFreqSlotID[1]);


                // ###### WYNALAZEK START
                // mialo sprawdzac, czy to skad przyzla wiadomosc 
                // to pierwszy FSU danego FS i tylko wtedy robic send, 
                // jesli to kolejne FSU to juz nie robic send bo pierwszy wyslal.
                Boolean canSend = false;
                foreach (var nw in LocalPhysicalWires.Wires)
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
                //Log.d("Socket {0} Read '{1}'[{2} bytes] from socket {3}.", client.LocalEndPoint.ToString(), state.Data.ToString(), bytesRead, client.RemoteEndPoint.ToString());

                if (canSend)
                {
                    //na pon
                    if (route[0] == -1 && route[1] == -1)
                    {
                        Log.d(String.Format("R: '{0}'[{1} bytes].", state.Data.info, bytesRead));
                        MessageHistory.Add(new KeyValuePair<string, Data>("RECEIVED", state.Data));
                        return;

                    }
                    Send(state.Data, route);
                }

            }
            catch (Exception e)
            {
                //Log.d(e.ToString());
            }
        }

        // ############################################################################################################
        // ####################### Agent communication methods section
        // ############################################################################################################

        void AgentConnectCallback(IAsyncResult ar)
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
                Log.i(String.Format("Node is connected to NMS {0}", IpToString(client.RemoteEndPoint)));

                // Signal that the connection has been made.
                agentConnectDone.Set();

                AgentOnline = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                AgentOnline.Bind(new IPEndPoint(IPAddress.Parse(address), AgentOnlinePort));
                AgentOnline.BeginConnect(new IPEndPoint(IPAddress.Parse(AgentIp), AgentOnlinePort), new AsyncCallback(AgentOnlineRequests), AgentOnline);
            }
            catch (Exception e)
            {
                //Log.d(e.ToString());
            }
        }

        void AgentOnlineRequests(IAsyncResult ar)
        {
            //var socket = ((Socket) ar.AsyncState);
            //socket.EndConnect(ar);
            try
            {
                var state = new StateObject();
                AgentOnline.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, result =>
                {
                    //Log.d("ROUTER RECEIVER ONLINE REQUEST AND SENDING RESPONSE");
                    var fs = new MemoryStream();
                    new BinaryFormatter().Serialize(fs, "ONLINE");
                    var buffer = fs.ToArray();
                    try
                    {
                        AgentOnline.BeginSend(buffer, 0, buffer.Length, 0, AgentOnlineRequests, agentSocket);
                    }
                    catch (ObjectDisposedException)
                    {
                        //todo what if exception?
                    }
                    catch (SocketException)
                    {
                        //todo what if exception?
                    }
                }, state);
            }
            catch (SocketException)
            {
                //todo what if exception?
            }
        }

        void AgentRun()
        {
            try
            {
                while (true)
                {
                    agentReceiveDone.Reset();
                    Log.i("Waiting for data from AGENT...");
                    AgentReceive();
                    agentReceiveDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                //Log.d(e.ToString());
            }
        }

        void AgentReceive()
        {
            Log.i("AgentReceive");
            try
            {
                // Create the state object.
                var state = new AgentStateObject { WorkSocket = agentSocket };
                //response = String.Empty;
                // Begin receiving the data from the remote device.
                agentSocket.BeginReceive(state.buffer, 0, AgentStateObject.BufferSize, 0, AgentReceiveCallback, state);
            }
            catch (Exception e)
            {
                //Log.d(e.ToString());
            }
        }

        void AgentReceiveCallback(IAsyncResult ar)
        {
            Log.d("AgentReceiveCallback");
            try
            {
                // Retrieve the state object and the client socket 
                // from the asynchronous state object.
                var state = (AgentStateObject)ar.AsyncState;
                var client = state.WorkSocket;

                // to bardzo wazne przypisanie socketu, otrzymanego po zestawieniu polaczenia i nasluch ustawiany musi byc na tym sockecie!
                agentSocket = client;

                var bytesRead = client.EndReceive(ar);

                var formattor = new BinaryFormatter();

                var ms = new MemoryStream(state.buffer);

                state.AgentData = (ExtSrc.AgentData)formattor.Deserialize(ms);

                ProcessAgentData(state.AgentData);

                agentReceiveDone.Set();
            }
            catch (Exception)
            {
                Log.CC("Connection to parent CC error.");
            }
        }

        void ProcessAgentData(ExtSrc.AgentData agentData)
        {
            int id1, id2;
            switch (agentData.Message)
            {
                case AgentComProtocol.DOMAIN_REGISTER:
                case AgentComProtocol.DOMAIN_SET_ROUTE_FOR_ME:
                case AgentComProtocol.DOMAIN_CAN_ROUTE:
                case AgentComProtocol.DOMAIN_DISROUTE:
                case AgentComProtocol.DOMAIN_CAN_SEND:
                case AgentComProtocol.DOMAIN_CAN_WE_SET_ROUTE:
                case AgentComProtocol.DOMAIN_INFO:
                    DomainSend(agentData);
                    //Log.d("Received and resend " + agentData.Message);
                    break;
                case AgentComProtocol.MODIFY_UNQCON_AFTER_REPAIR:
                    var uniqConn = UniqueConnections.FirstOrDefault(w => w.AddressA.Equals(agentData.OriginatingAddress) & w.AddressB.Equals(agentData.RouterIpAddress));
                    if (uniqConn != null)
                    {   
                        uniqConn.AddressB = agentData.TargetAddress;
                    }
                    break;
                case AgentComProtocol.ROUTE_UNAVAIBLE:
                    //Log.d("Route unavaible, delete waiting message.");
                    Log.CCC("Call request reject received.");
                    var uniqConn2 = UniqueConnections.FirstOrDefault(w => w.UniqueKey.Equals(agentData.UniqueKey));
                    UniqueConnections.Remove(uniqConn2);
                    var wm = WaitingMsgs.FirstOrDefault(w => w.Value.uniqueKey.Equals(agentData.UniqueKey));
                    WaitingMsgs.Remove(wm);
                    break;
                case AgentComProtocol.AVAIBLE_STARTING_FREQS:
                    if (agentData.IsEndEdge && isEdge) Log.CCC("Call accepted");
                    Log.d("AVAIBLE_STARTING_FREQS asked fsuCount" + agentData.FsuCount + " wireId " + agentData.WireId);
                    var result = LocalPhysicalWires.GetAvaibleFreqSlots(agentData.FsuCount, agentData.WireId);
                    result.ForEach(e => Log.d("AVAIBLE SLOT = <" + e[0] + "," + e[1] + ">"));
                    AgentSend(new AgentData() { Message = AgentComProtocol.MY_FREES_FREQ_SLOTS, StartingFreqs = result });
                    break;
                case ExtSrc.AgentComProtocol.ROUTE_FOR_U_EDGE:
                    //Log.d("ROUTE_FOR_U_EDGE StartFreq=" + agentData.StartingFreq + " fsucount:" + agentData.FsuCount);
                    Log.CC("Connection Request IN");
                    var startfreqEdge = agentData.StartingFreq;
                    id1 = LocalPhysicalWires.getWireByID(agentData.WireId).addFreqSlot(startfreqEdge, agentData.FsuCount, agentData.Mod);
                    var ucon = UniqueConnections.FirstOrDefault(x => x.UniqueKey.Equals(agentData.UniqueKey));

                    if (!agentData.IsStartEdge)
                        FreqSlotSwitchingTable.add(agentData.WireId, id1, -1, -1);

                    if (ucon == null)
                    {
                        ucon = new UniqueConnection()
                        {
                            AddressA = agentData.OriginatingAddress,
                            AddressB = agentData.TargetAddress,
                            UniqueKey = agentData.UniqueKey,
                            isOnline = true
                        };
                        UniqueConnections.Add(ucon);
                    }
                    ucon.WireAndFsu = new int[] { agentData.WireId, id1 };
                    if (agentData.IsStartEdge)
                    {
                        var msg = WaitingMsgs.FirstOrDefault(e => e.Value.uniqueKey.Equals(agentData.UniqueKey));
                        if (msg.Value != null)
                        {
                            var newMsg = new KeyValuePair<int[], DataAndID>(ucon.WireAndFsu, msg.Value);
                            WaitingMsgs.Remove(msg);
                            WaitingMsgs.Add(newMsg);
                        }
                    }
                    Log.CC("Connection confirmed.");
                    //Log.d("ROUTE SET, EDGE");
                    AgentSend(new AgentData()
                    {
                        Message = AgentComProtocol.CONNECTION_IS_ON,
                        StartingFreq = startfreqEdge,
                        FSid = id1
                    });
                    break;

                case ExtSrc.AgentComProtocol.ROUTE_FOR_U:
                    //Log.d("ROUTE_FOR_U StartFreq=" + agentData.StartingFreq + " fsucount:" + agentData.FsuCount);
                    Log.CC("Connection Request IN");
                    id1 = LocalPhysicalWires.getWireByID(agentData.FirstWireId).addFreqSlot(agentData.StartingFreq, agentData.FsuCount, agentData.LastMod);
                    id2 = LocalPhysicalWires.getWireByID(agentData.SecondWireId).addFreqSlot(agentData.StartingFreq, agentData.FsuCount, agentData.Mod);
                    FreqSlotSwitchingTable.add(agentData.FirstWireId, id1, agentData.SecondWireId, id2);
                    //Log.d("ROUTE SET, NOT EDGE");
                    AgentSend(new AgentData()
                    {
                        Message = AgentComProtocol.CONNECTION_IS_ON,
                        StartingFreq = agentData.StartingFreq,
                        FSid = id2
                    });
                    Log.CC("Connection confirmed.");
                    break;
                case ExtSrc.AgentComProtocol.DISROUTE:
                    Log.CC("Connection teardown in.");
                    //Log.d("DISROUTE MSG ARRIVED, : " + address + " -> remove WIRE_ID : " + agentData.FirstWireId + " FSid : " + agentData.FSid);
                    var inttab = new int[2];
                    inttab = FreqSlotSwitchingTable.findReverseRoute(agentData.FirstWireId, agentData.FSid);
                    if (LocalPhysicalWires.getWireByID(agentData.FirstWireId).removeFreqSlot(agentData.FSid) &&
                        LocalPhysicalWires.getWireByID(inttab[0]).removeFreqSlot(inttab[1]))
                    {
                        //freqSlotSwitchingTable.remove(agentData.firstWireID, agentData.FSid, inttab[0], inttab[1]);
                        FreqSlotSwitchingTable.remove(inttab[0], inttab[1], agentData.FirstWireId, agentData.FSid);
                        AgentSend(new AgentData() { Message = AgentComProtocol.DISROUTE_IS_DONE });
                        //Log.d("DISROUTE DONE");
                        
                        Log.CC("Connection teardown confirmed.");
                    }
                    else
                    {
                        AgentSend(new AgentData() { Message = AgentComProtocol.DISROUTE_ERROR });
                        Log.d("DISROUTE ERROR!!!!");
                        Log.CC("Connection teardown failed.");
                    }
                    break;
                case ExtSrc.AgentComProtocol.DISROUTE_EDGE:
                    //Log.d("DISROUTE_EDGE MSG ARRIVED, : " + address + " -> remove WIRE_ID : " + agentData.FirstWireId + " FSid : " + agentData.FSid);
                    if(agentData.IsEndEdge && isEdge) Log.CCC("Call Teardown In");
                    if (LocalPhysicalWires.getWireByID(agentData.FirstWireId).removeFreqSlot(agentData.FSid))
                    {
                        FreqSlotSwitchingTable.removeEdge(agentData.FirstWireId, agentData.FSid);

                        UniqueConnection uconnn = null;
                        foreach (var uniqueConnection in UniqueConnections)
                        {
                            if (uniqueConnection.UniqueKey.Equals(agentData.UniqueKey))
                                uconnn = uniqueConnection;
                        }
                        if (uconnn != null)
                            UniqueConnections.Remove(uconnn);
                        AgentSend(new AgentData() { Message = AgentComProtocol.DISROUTE_EDGE_IS_DONE });
                        //Log.d("DISROUTE EDGE DONE");
                        if (agentData.IsEndEdge && isEdge) Log.CCC("Call Teardown Confirmed");
                        Log.CC("Connection teardown in.");
                        Log.CC("Connection teardown confirmed.");
                    }
                    else
                    {
                        AgentSend(new AgentData() { Message = AgentComProtocol.DISROUTE_ERROR_EDGE });
                        //Log.d("DISROUTE EDGE ERROR!!!!");
                    }
                    break;
                case ExtSrc.AgentComProtocol.U_CAN_SEND:
                    //Otrzymano pozwolenie na wyslanie wiadomosci z kolejki
                    //Log.d("U_CAN_SEND");
                    Log.CC("Call confirmed.");
                    var uc = UniqueConnections.First(w => w.UniqueKey.Equals(agentData.UniqueKey));
                    if (uc != null) uc.isOnline = true;
                    break;

                default:
                    //Log.d("Zły msg przybył");
                    break;
            }
        }

        void AgentSend(ExtSrc.AgentData conn)
        {
            var fs = new MemoryStream();

            var formatter = new BinaryFormatter();

            formatter.Serialize(fs, conn);

            var buffer = fs.ToArray();

            // Begin sending the data to the remote device.
            agentSocket.BeginSend(buffer, 0, buffer.Length, 0, AgentSendCallback, agentSocket);
            agentSendDone.WaitOne();
        }

        void AgentSendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                var client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                var bytesSent = client.EndSend(ar);
                //Log.d("Sent {0} bytes to AGENT.", bytesSent);

                // Signal that all bytes have been sent.
                agentSendDone.Set();
            }
            catch (Exception e)
            {
                //Log.d(e.ToString());
            }
        }

        public void MessageToSend(String target, Data data)
        {
            String key;
            var wireandfsid = new int[] { };
            var uc = UniqueConnections.FirstOrDefault(w => w.AddressA.Equals(address) & w.AddressB.Equals(target));
            if (uc != null && uc.isOnline)
            {
                key = uc.UniqueKey;
                wireandfsid = uc.WireAndFsu;
            }
            else
            {
                key = GenerateUniqueKey();
                Log.CCC("Call request.");
                AgentSend(new AgentData()
                {
                    Message = AgentComProtocol.SET_ROUTE_FOR_ME,
                    OriginatingAddress = address,
                    TargetAddress = target,
                    UniqueKey = key,
                    Bitrate = data.bandwidthNeeded
                });

                UniqueConnections.Add(new UniqueConnection()
                {
                    UniqueKey = key,
                    AddressA = address,
                    AddressB = target,
                    isOnline = false
                });
            }
            // dodac na liste oczekujacych wyslan
            WaitingMsgs.Add(new KeyValuePair<int[], DataAndID>(wireandfsid, new ExtSrc.DataAndID(data, Id, key)));
        }

        public void RegisterToAgent()
        {
            var wiresIds = LocalPhysicalWires.Wires.Select(nw => new DijkstraData(Id, nw.ID, nw.distance, nw.RouterIds)).ToList();
            AgentSend(new AgentData()
            {
                Message = AgentComProtocol.REGISTER,
                WireIDsList = wiresIds,
                RouterIpAddress = address,/*
                RouterID = Int32.Parse(address.Substring(address.Length - 1, 1)),*/
                IsStartEdge = isEdge
            });
        }

        public void UnregisterToAgent()
        {
            var wiresIds = LocalPhysicalWires.Wires.Select(nw => new DijkstraData(Id, nw.ID, nw.distance, nw.RouterIds)).ToList();
            AgentSend(new AgentData()
            {
                Message = AgentComProtocol.UNREGISTER,
                WireIDsList = wiresIds,
                RouterIpAddress = address,
                RouterID = Int32.Parse(address.Substring(address.Length - 1, 1)),
                IsStartEdge = isEdge
            });
        }

        // ############################################################################################################
        // ####################### Other provider domain listening
        // ############################################################################################################

        public void ConnectDomain(String otherDomainAddress)
        {
            try
            {
                _domainAddress = otherDomainAddress;
                var domainRemoteEndPoint = new IPEndPoint(IPAddress.Parse(_domainAddress), DomainPort);
                if (!domainSocket.IsBound)
                {
                    var domainLocalEndPoint = new IPEndPoint(IPAddress.Parse(address), DomainPort);
                    domainSocket.Bind(domainLocalEndPoint);
                    domainSocket.ReceiveBufferSize = 1024 * 100;
                }
                domainSocket.BeginConnect(domainRemoteEndPoint, DomainConnectCallback, domainSocket);
                domainConnectDone.WaitOne();
            }
            catch (SocketException)
            {
                Log.d("Connected from other side. " + domainSocket.Connected);
                Log.CC("Inter domain connection succeeded");
                domainConnectDone.Set();
                new Thread(DomainListening).Start();
            }
        }

        void DomainConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                var client = (Socket)ar.AsyncState;

                // Complete the connection.
                lock (this)
                {
                    client.EndConnect(ar);
                }
                //Log.d(String.Format("Node is connected to other Domain {0}", IpToString(client.RemoteEndPoint)));
                Log.CC("Inter domain connection succeeded");
                // Signal that the connection has been made.
                domainConnectDone.Set();

                new Thread(DomainListening).Start();
            }
            catch (Exception)
            {
                Log.CC("Inter domain connection failed");
                //Log.d("Connecting to other domain failed.");
                Thread.Sleep(1000);
                ConnectDomain(_domainAddress);
            }
        }

        void DomainListening()
        {
            DomainSend(new AgentData() { Message = AgentComProtocol.DOMAIN_REGISTER });
            try
            {
                while (true)
                {
                    domainReceiveDone.Reset();
                    Log.i("Waiting for data from Other provider...");
                    DomainReceie();
                    domainReceiveDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                //Log.d(e.ToString());
            }
        }

        void DomainReceie()
        {
            Log.i("DomainReceie");
            try
            {
                // Create the state object.
                var state = new AgentStateObject { WorkSocket = domainSocket };
                //response = String.Empty;
                // Begin receiving the data from the remote device.
                domainSocket.BeginReceive(state.buffer, 0, AgentStateObject.BufferSize, 0, DomainReceiveCallback, state);
            }
            catch (Exception e)
            {
                //Log.d(e.ToString());
            }
        }

        void DomainReceiveCallback(IAsyncResult ar)
        {
            Log.d("DomainReceiveCallback");
            try
            {
                // Retrieve the state object and the client socket 
                // from the asynchronous state object.
                var state = (AgentStateObject)ar.AsyncState;
                var client = state.WorkSocket;

                // to bardzo wazne przypisanie socketu, otrzymanego po zestawieniu polaczenia i nasluch ustawiany musi byc na tym sockecie!
                domainSocket = client;

                var bytesRead = client.EndReceive(ar);

                var formattor = new BinaryFormatter();

                var ms = new MemoryStream(state.buffer);

                state.AgentData = (ExtSrc.AgentData)formattor.Deserialize(ms);

                //ProcessAgentData(state.AgentData);
                AgentSend(state.AgentData);

                domainReceiveDone.Set();
            }
            catch (Exception)
            {
                Log.d("Domain Node Closed.");
            }
        }

        void DomainSend(ExtSrc.AgentData conn)
        {
            if (domainSocket == null)
            {
                Log.d("Domain connection does not exist.");
                return;
            }

            var fs = new MemoryStream();

            var formatter = new BinaryFormatter();

            formatter.Serialize(fs, conn);

            var buffer = fs.ToArray();

            domainSocket.BeginSend(buffer, 0, buffer.Length, 0, DomainSendCallback, domainSocket);
            domainSendDone.WaitOne();
        }

        void DomainSendCallback(IAsyncResult ar)
        {
            try
            {
                var client = (Socket)ar.AsyncState;
                var bytesSent = client.EndSend(ar);
                domainSendDone.Set();
            }
            catch (Exception e)
            {
                //Log.d(e.ToString());
            }
        }

        // ############################################################################################################
        // ####################### Tools methods
        // ############################################################################################################

        internal void Closing()
        {
            IsListening = false;
            notifyObservers();
            LocalPhysicalWires.Close();
            agentSocket.Close();
            AgentOnline.Close();
            NodeForm.Finish();
            System.Windows.Forms.Application.Exit();
            System.Environment.Exit(1);
        }

        private String GenerateUniqueKey()
        {
            Guid g = Guid.NewGuid();
            String str = Convert.ToBase64String(g.ToByteArray());
            str = str.Replace("=", "");
            str = str.Replace("+", "");
            str = str.Replace("/", "");
            return str;
        }

        private static string IpToString(EndPoint endPoint)
        {
            var ipEndPoint = endPoint as IPEndPoint;
            return ipEndPoint != null ? ipEndPoint.Address.ToString() : null;
        }

        // ############################################################################################################
        // ####################### Overriden methods
        // ############################################################################################################

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
                o.Update();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            // dispose managed resources
            AgentOnline.Close();
            agentSocket.Close();
            domainSocket.Close();

            sendDone.Close();
            receiveDone.Close();
            allDone.Close();
            allReceive.Close();
            agentConnectDone.Close();
            agentReceiveDone.Close();
            agentSendDone.Close();
            domainConnectDone.Close();
            domainReceiveDone.Close();
            domainSendDone.Close();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Disroute(string ipAddress)
        {
            var uc = UniqueConnections.FirstOrDefault(w => w.AddressA.Equals(address) & w.AddressB.Equals(ipAddress));
            if (uc != null)
            {
                Log.CCC("Call teardown out.");
                AgentSend(new AgentData(){ Message = AgentComProtocol.DISROUTE_REQUEST, UniqueKey = uc.UniqueKey});
            }
        }
    }

    public class StateObject
    {
        // Client socket.
        public Socket WorkSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024 * 5;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        // public StringBuilder sb = new StringBuilder();
        public Data Data;
    }

    public class AgentStateObject
    {
        // Client socket.
        public Socket WorkSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024 * 100;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        public AgentData AgentData { get; set; }
    }
}
