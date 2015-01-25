using System.ComponentModel;
using System.Diagnostics;
using System.Security.Cryptography;
using ExtSrc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Threading.Timer;

namespace SubnetworkController
{
    public class SubnetworkController
    {
        const int Timeout = 2000;
        public String AgentIp = "127.6.6.6";
        // Agent communication port
        private const int AgentPort = 6666;
        // Agent router-online-check port
        private const int AgentOnlinePort = 6667;

        readonly Action<DijkstraData> _dijkstraDataAdder;

        // Gui Object
        public Form SubNetForm { get; set; }

        // Agent socket to communicate with nodes
        Socket _socket;

        // Agent socket to keep nodes online
        Socket _onlineAgentSocket;

        // Agent communication event flags
        readonly ManualResetEvent _allDone = new ManualResetEvent(false);
        readonly ManualResetEvent _sendDone = new ManualResetEvent(false);
        readonly ManualResetEvent _allDoneOnline = new ManualResetEvent(false);
        readonly ManualResetEvent _sendDoneOnline = new ManualResetEvent(false);

        // Is agent running?
        bool _running = true;

        // Dijkstra algorithm protocol
        Dijkstra Dijkstra { get; set; }

        // Map of "IpAddress" -> Socket to node
        Dictionary<String, Socket> Sockets { get; set; }

        // Map of "hashKey" -> {ID of edge routers for path}
        internal Dictionary<String, int[]> EdgeRouterIDs { get; set; }
        internal Dictionary<String, int[]> EdgeLocalRouterIDs { get; set; }
        internal Dictionary<String, int[]> EdgeRemoteRouterIDs { get; set; }

        // Map of "RouterID" -> {ID of avaible client nodes in other domain}
        public Dictionary<int, List<int>> OtherDomainInfo { get; set; }

        // List of avaible client nodes in local domain
        public List<int> MyDomainInfo { get; set; }

        // List of received data from Nodes
        List<AgentData> BufferAgentData { get; set; }

        // Response which are needed for now
        AgentData _bufferRouterResponse;

        // Route Table With Dijkstra data
        public BindingList<DijkstraData> DijkstraDataList { get; set; }

        // List of Online Routers, send requests, show in gui
        public List<RouterOnline> OnlineRoutersList { get; set; }

        // Map of{routerAaddress, routerBaddress, hashKey} - > routeHistory (list of {routerID, wireId, FSid} )
        public Dictionary<String[], List<int[]>> RouteHistoryList { get; set; }


        // ############################################################################################################
        // ####################### Constructor's and methods
        // ############################################################################################################

        public SubnetworkController(String ip)
        {
            AgentIp = ip;

            // Initialazing
            OnlineRoutersList = new List<RouterOnline>();
            _dijkstraDataAdder = dd => DijkstraDataList.Add(dd);
            RouteHistoryList = new Dictionary<String[], List<int[]>>(new MyEqualityStringComparer());
            EdgeRouterIDs = new Dictionary<String, int[]>();
            EdgeLocalRouterIDs = new Dictionary<String, int[]>();
            EdgeRemoteRouterIDs = new Dictionary<String, int[]>();
            DijkstraDataList = new BindingList<DijkstraData>();
            Sockets = new Dictionary<String, Socket>();
            Dijkstra = new Dijkstra(this);
            BufferAgentData = new List<ExtSrc.AgentData>();
            OtherDomainInfo = new Dictionary<int, List<int>>();
            MyDomainInfo = new List<int>();
        }

        public void ConnectAndRun()
        {
            // Run and connect
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Bind(new IPEndPoint(IPAddress.Parse(AgentIp), AgentPort));
            _socket.ReceiveBufferSize = 1024 * 100;
            _onlineAgentSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _onlineAgentSocket.Bind(new IPEndPoint(IPAddress.Parse(AgentIp), AgentOnlinePort));

            new Thread(Run).Start();
            new Thread(ProcessAgentDataRun).Start();
            //new Thread(ListeningForOnlineNodes).Start();
            //new Thread(SendingOnlineRequests).Start();
        }

        // ############################################################################################################
        // ####################### Listening and processing protocol messages from Nodes
        // ############################################################################################################

        // Listening for connections from Nodes thread
        void Run()
        {
            try
            {
                _socket.Listen(100);
                while (true)
                {
                    // Set the event to nonsignaled state.
                    _allDone.Reset();

                    // Start an asynchronous socket to listen for connections.
                    //Console.WriteLine("Waiting for a connection...");
                    _socket.BeginAccept(AcceptCallback, _socket);

                    // Wait until a connection is made before continuing.
                    _allDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        // Accept connections from Nodes, add to Socket list and start to receive protocol messages
        void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            _allDone.Set();

            // Get the socket that handles the client request.
            var listener = (Socket)ar.AsyncState;
            var handler = listener.EndAccept(ar);
            Sockets.Add(Convert.ToString((handler.RemoteEndPoint as IPEndPoint).Address), handler);
            //addConnection(handler.RemoteEndPoint.ToString());
            //Console.WriteLine("Socket [{0}] {1} - {2} was added to sockets list", sockets.Count, handler.LocalEndPoint.ToString(), handler.RemoteEndPoint.ToString());

            // Create the state object.
            var state = new StateObject {workSocket = handler};
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, ReadCallback, state);
        }

        // Recursive receive protocol messages from Nodes and start receiving again. Add message to buffor or as response.
        void ReadCallback(IAsyncResult ar)
        {
            try
            {
                var content = String.Empty;

                // Retrieve the state object and the handler socket
                // from the asynchronous state object.
                var state = (StateObject)ar.AsyncState;
                var handler = state.workSocket;

                // Read data from the client socket. 
                var bytesRead = handler.EndReceive(ar);
                var formattor = new BinaryFormatter();

                var ms = new MemoryStream(state.buffer);

                state.dt = (ExtSrc.AgentData) formattor.Deserialize(ms);
                var addr = IpToString(handler.RemoteEndPoint);
                Console.WriteLine("R: '{0}'[{1} bytes] from {2}.", state.dt.ToString(), bytesRead, addr);

                //
                // Odbieramy dane od routera dodajemy do bufora,
                // aby odebrac dane od wszystkich i nic nie stracić
                // 
                if (state.dt.Message.Equals(AgentComProtocol.CONNECTION_IS_ON) ||
                    state.dt.Message.Equals(AgentComProtocol.CONNECTION_UNAVAILABLE) ||
                    state.dt.Message.Equals(AgentComProtocol.DISROUTE_IS_DONE) ||
                    state.dt.Message.Equals(AgentComProtocol.DISROUTE_ERROR) ||
                    state.dt.Message.Equals(AgentComProtocol.DISROUTE_ERROR_EDGE) ||
                    state.dt.Message.Equals(AgentComProtocol.DOMAIN_CAN_ROUTE) ||
                    state.dt.Message.Equals(AgentComProtocol.DOMAIN_CAN_SEND) ||
                    state.dt.Message.Equals(AgentComProtocol.MY_FREES_FREQ_SLOTS) ||
                    state.dt.Message.Equals(AgentComProtocol.DISROUTE_EDGE_IS_DONE)
                    )
                    _bufferRouterResponse = state.dt;
                else if (state.dt.Message.Equals(AgentComProtocol.DOMAIN_INFO) ||
                    state.dt.Message.Equals(AgentComProtocol.DOMAIN_REGISTER) ||
                    state.dt.Message.Equals(AgentComProtocol.DOMAIN_SET_ROUTE_FOR_ME) ||
                    state.dt.Message.Equals(AgentComProtocol.DOMAIN_CAN_WE_SET_ROUTE) ||
                    state.dt.Message.Equals(AgentComProtocol.REGISTER))
                {
                    state.dt.RouterID = Int32.Parse(addr.Substring(addr.Length - 1, 1));
                    BufferAgentData.Add(state.dt);
                } else
                    BufferAgentData.Add(state.dt);


                var newState = new StateObject {workSocket = handler};
                handler.BeginReceive(newState.buffer, 0, StateObject.BufferSize, 0, ReadCallback, newState);
            }
            catch (SocketException e)
            {
                //int line = (new StackTrace(e, true)).GetFrame(0).GetFileLineNumber();
                //Console.WriteLine("Router probably closed (ERROR LINE: "+line+")");
            }
        }

        // Processing thread, get message from buffer, do the job and remove
        void ProcessAgentDataRun()
        {
            while (_running)
            {
                if (BufferAgentData.Count == 0) continue;
                ProcessAgentData(BufferAgentData.First());
                BufferAgentData.RemoveAt(0);
            }
        }
       
        // Work to do after receving protocol messages
        void ProcessAgentData(ExtSrc.AgentData agentData)
        {
            switch (agentData.Message)
            {
                case AgentComProtocol.DOMAIN_REGISTER:
                    Console.WriteLine("Received " + agentData.Message + " " + agentData.RouterID);
                    OtherDomainInfo.Add(agentData.RouterID, new List<int>());
                    SendDomainInfoToOtherDomains();
                    break;
                case AgentComProtocol.DOMAIN_INFO:
                    //OtherDomainInfo.Add(agentData.RouterID, agentData.DomainInfo);
                    OtherDomainInfo[agentData.RouterID] = agentData.DomainInfo;
                    Console.WriteLine("Received " + agentData.Message + " " + agentData.RouterID + " " + agentData.DomainInfo.Count);
                    break;
                case AgentComProtocol.DOMAIN_CAN_WE_SET_ROUTE:
                    //OtherDomainInfo.Add(agentData.RouterID, agentData.DomainInfo);
                    //OtherDomainInfo[agentData.RouterID] = agentData.DomainInfo;
                    Console.WriteLine("Other domain asked for route");
                    // todo tutaj sprawdzic czy mozna zestawic polaczenie dla zewnetrznej sieci
                    var route = CalculateRoute("127.0.1." + agentData.RouterID, agentData.TargetAddress, null);
                    var startingFreqs = CalculateAvaibleStartingFreqs(route, agentData.FsuCount);
                    Send("127.0.1."+agentData.RouterID, new AgentData()
                    {
                        Message = AgentComProtocol.DOMAIN_CAN_ROUTE,
                        DomainRouterID = DomainToTargetConnector(agentData.DomainRouterID),
                        StartingFreqsPool = startingFreqs 
                    });
                    break;
                case AgentComProtocol.DOMAIN_SET_ROUTE_FOR_ME:
                    Console.WriteLine("Received " + agentData.Message + " " + agentData.RouterID);
                    //OtherDomainInfo.Add(agentData.RouterID, new List<int>());
                    //SendDomainInfoToOtherDomains();
                    SetRemoteRoute(agentData.OriginatingAddress, agentData.TargetAddress, agentData.Bitrate, null,
                        agentData.UniqueKey, agentData.StartingFreq, agentData.DomainRouterID);
                    Send("127.0.1." + agentData.RouterID, new AgentData()
                    {
                        Message = AgentComProtocol.DOMAIN_CAN_SEND
                    });
                    break;
                case ExtSrc.AgentComProtocol.REGISTER:
                    Console.WriteLine("Router {0} connected.", agentData.RouterIpAddress);
                    //dijkstra.RoutersNum++;
                    foreach (var dd in agentData.WireIDsList)
                    {
                        SubNetForm.Invoke(this._dijkstraDataAdder, dd);
                    }
                    if (agentData.IsStartEdge) UpdateClientList(agentData.RouterID, true);
                    Console.WriteLine("DDL Count:"+DijkstraDataList.Count);
                    //rejestruje sie na liste 
                    break;
                case ExtSrc.AgentComProtocol.SET_ROUTE_FOR_ME:
                    Console.WriteLine("Router asked for route.");
                    var targetId = Int32.Parse(agentData.TargetAddress.Substring(agentData.TargetAddress.Length - 1, 1));
                    if (MyDomainInfo.Contains(targetId))
                    {
                        // Local domain routing
                        //policz droge i odeslij do wszystkich ruterow ktore maja byc droga informacje route-for-you
                        var hashCode = agentData.UniqueKey;
                        //setRoute(agentData.ClientIpAddress, agentData.TargetAddress, agentData.Bitrate, null, hashCode);                               
                        SetRoute(agentData.OriginatingAddress, agentData.TargetAddress, agentData.Bitrate, null, hashCode);
                        Send(agentData.OriginatingAddress, new AgentData() { Message = AgentComProtocol.U_CAN_SEND, UniqueKey = hashCode });
                    }
                    else
                    {
                        Console.WriteLine("Target not found in my domain. id="+targetId);
                        // Other domain routing
                        if (TargetExists(targetId))
                        {
                            Console.WriteLine("Target found in other domain.");
                            var hashCode = agentData.UniqueKey;                              
                            SetDomainRoute(agentData.OriginatingAddress, agentData.TargetAddress, agentData.Bitrate, null, hashCode);
                            Send(agentData.OriginatingAddress, new AgentData() { Message = AgentComProtocol.U_CAN_SEND, UniqueKey = hashCode });
                        }
                        else
                        {
                            Console.WriteLine("There not target found at all.");
                        }
                    }
                    break;
                case ExtSrc.AgentComProtocol.MSG_DELIVERED:
                    //todo info o tym ze jakas wiadomosc dotarla na koniec drogi
                    break;
                case ExtSrc.AgentComProtocol.CONNECTION_IS_ON:
                    //todo zestawianie zakonczone w danym routerze
                    break;
                case ExtSrc.AgentComProtocol.REGISTER_CLIENT:
                    //dodawanie do mapu adresow ip router-klient
                    Console.WriteLine("Client {0} connected to router {1}.", agentData.ClientIpAddress, agentData.RouterIpAddress);
                    //ClientMap.Add(agentData.ClientIpAddress, agentData.RouterIpAddress);
                    break;
                case ExtSrc.AgentComProtocol.CLIENT_DISCONNECTED:
                    Console.WriteLine("Client {0} disconnected from router {1}.", agentData.ClientIpAddress, agentData.RouterIpAddress);
                    //ClientMap.Remove(agentData.ClientIpAddress);
                    break;
                default:
                    //Console.WriteLine("Zły msg przybył");
                    break;
            }
        }

        bool TargetExists(int id)
        {
            //return OtherDomainInfo.SelectMany(kvp => kvp.Value).Any(i => id == i);
            foreach (var kvp in OtherDomainInfo)
            {
                foreach (var i in kvp.Value)
                {
                    if (i == id) return true;
                }
            }
            return false;
        }

        int DomainToTargetConnector(int id)
        {
            //return (from kvp in OtherDomainInfo from i in kvp.Value where id == i select kvp.Key).FirstOrDefault();
            foreach (var kvp in OtherDomainInfo)
            {
                foreach (var i in kvp.Value)
                {
                    if (i == id) return kvp.Key;
                }
            }
            return 0;
        }

        // ############################################################################################################
        // ####################### Calculate, set and send route messages
        // ############################################################################################################

        private void SetRoute(String originatingAddress, String targetAddress, int bitrate, int[] excludedWiresIDs, String hashKey)
        {
            var routeHistory = new List<int[]>();
            var startfrequency = -1;

            var route = CalculateRoute(originatingAddress, targetAddress, excludedWiresIDs);
            if (route == null)
            {
                Console.WriteLine("Can't find route from " + originatingAddress + " to " + targetAddress);
                return;
            }
            
            for (var j = 0; j < route.Length; j++)
            {
                _bufferRouterResponse = null;

                if (j == 0)
                {
                    //wyslij d source routera
                    //Console.WriteLine("WYSYLAM DO PIERWSZEGO EDGE ROUTERA DANE ROUTINGOWE");
                    var ar = CalculateFsUcountFromTo(route[j], FindWireIdFromTo(route[j], route[j + 1], route[j]), bitrate);
                    var ip = String.Format("127.0.1." + route[j]);
                    Send(ip, new AgentData()
                    {
                        Message = AgentComProtocol.ROUTE_FOR_U_EDGE,
                        FsuCount = (int)ar[1],
                        Mod = (Modulation)ar[0],
                        WireId = FindWireIdFromTo(route[j], route[j + 1], route[j]),
                        OriginatingAddress = originatingAddress,
                        TargetAddress = targetAddress,
                        UniqueKey = hashKey,
                        StartingFreq = startfrequency,
                        IsStartEdge = true
                    });
                    var rSid = Int32.Parse(originatingAddress.Substring(originatingAddress.Length - 1, 1));
                    EdgeRouterIDs.Add(hashKey, new int[2] { rSid, -1 });

                    //Console.WriteLine("WYSYLALEM DO PIERWSZEGO EDGE ROUTERA DANE ROUTINGOWE (" + ip + ")");
                }
                else if (j > 0 && j < route.Length - 1)
                {
                    //fsucount, mod, firstwireid,secondwireid, startingfreq dla odbierajacego kabla bo juz obliczone w poprzednim roouterze
                    //wyslij do zwyklych routerow
                    //Console.WriteLine("WYSYLAM DO SRODKOWEGO ROUTERA DANE ROUTINGOWE");
                    var ar0 = CalculateFsUcountFromTo(route[j], FindWireIdFromTo(route[j - 1], route[j], route[j]), bitrate);
                    var ar = CalculateFsUcountFromTo(route[j], FindWireIdFromTo(route[j], route[j + 1], route[j]), bitrate);
                    if ((int)ar[0] == 0 || (int)ar[1] == 0) return;
                    var ip = String.Format("127.0.1." + route[j]);
                    Send(ip, new AgentData()
                    {
                        Message = AgentComProtocol.ROUTE_FOR_U,
                        LastFsuCount = (int)ar0[1],
                        LastMod = (Modulation)ar0[0],
                        FsuCount = (int)ar[1],
                        Mod = (Modulation)ar[0],
                        FirstWireId = FindWireIdFromTo(route[j - 1], route[j], route[j]),
                        SecondWireId = FindWireIdFromTo(route[j], route[j + 1], route[j]),
                        StartingFreq = startfrequency
                    });
                    //Console.WriteLine("WYSYLALEM DO SRODKOWEGO ROUTERA DANE ROUTINGOWE (" + ip + ")");
                }
                else if (j == route.Length - 1)
                {
                    //wyslij do destinationIP routra
                    //Console.WriteLine("WYSYLAM DO OSTATNIEGO EDGE ROUTERA DANE ROUTINGOWE");
                    var ar0 = CalculateFsUcountFromTo(route[j], FindWireIdFromTo(route[j - 1], route[j], route[j]), bitrate);
                    var ip = String.Format("127.0.1." + route[j]);
                    Send(ip, new AgentData()
                    {
                        Message = AgentComProtocol.ROUTE_FOR_U_EDGE,
                        FsuCount = (int)ar0[1],
                        Mod = (Modulation)ar0[0],
                        WireId = FindWireIdFromTo(route[j - 1], route[j], route[j]),
                        OriginatingAddress = originatingAddress,
                        TargetAddress = targetAddress,
                        UniqueKey = hashKey,
                        StartingFreq = startfrequency,
                        IsStartEdge = false
                    });
                    //Console.WriteLine("WYSYLALEM DO OSTATNIEGO EDGE ROUTERA DANE ROUTINGOWE (" + ip + ")");
                }
                //sprawdz czy dostepne bitrejtyy
                var waitTime = 0;
                while (_bufferRouterResponse == null)
                {
                    //todo a co jak sie zatnie? coś nie dojdzie? router padnie?

                    //w8 na odp od routera
                    Thread.Sleep(50);
                    //stestowac to
                    /*waitTime += 50;
                    if(waitTime > 10000)
                        return;*/
                }

                if (_bufferRouterResponse.Message.Equals(ExtSrc.AgentComProtocol.CONNECTION_IS_ON))
                {
                    startfrequency = _bufferRouterResponse.StartingFreq;
                    //dodawanie do routeHistory
                    if (j < route.Length - 1)
                    {
                        //Console.WriteLine("Add to route {0} wire {1} and slot {2} ", route[j], FindWireId(route[j], route[j + 1]), bufferRouterResponse.FSid);
                        routeHistory.Add(new int[3] { route[j], FindWireIdFromTo(route[j], route[j + 1], route[j]), _bufferRouterResponse.FSid });
                    }
                    if (j == route.Length - 1)
                    {
                        routeHistory.Add(new int[3] { route[j], FindWireIdFromTo(route[j - 1], route[j], route[j]), _bufferRouterResponse.FSid });
                        RouteHistoryList.Add(new String[3] { originatingAddress, targetAddress, hashKey }, routeHistory);

                        var rRid = Int32.Parse(targetAddress.Substring(targetAddress.Length - 1, 1));
                        // edgeRouterIDs.Add(hashKey, new int[2] { rSid, rRid });

                        var tmp = EdgeRouterIDs[hashKey];
                        //todo tutaj do edgeRouterIDs dopisuje rRid, a co gdy CONNECTION_UNAVAILABLE ? 
                        //todo wpis zostanie nie kompletny, gdzie usuwanie? w disroute brak tego
                        EdgeRouterIDs[hashKey] = new int[] { tmp[0], rRid };
                        Console.WriteLine("Route set.");
                        foreach (var rh in routeHistory)
                        {
                            Console.WriteLine("Router {0} wire {1} and slot {2}. ", rh[0], rh[1], rh[2]);
                        }
                        return;
                    }
                }
                else if (_bufferRouterResponse.Message.Equals(ExtSrc.AgentComProtocol.CONNECTION_UNAVAILABLE))
                {
                    //rozlaczyc to co juz zestawione i zaczac liczyc dijkstre bez kabla ktory nie mial miejsca
                    if (Disroute(routeHistory, hashKey))
                    {
                        var excWiresNext = new int[excludedWiresIDs.Length + 1];
                        for (var i = 0; i < excludedWiresIDs.Length; i++)
                        {
                            excWiresNext[i] = excludedWiresIDs[i];
                        }
                        excWiresNext[excWiresNext.Length - 1] = FindWireIdFromTo(route[j], route[j + 1], route[j]);
                        // gdy rozłączone
                        //wyjsc z petli for, zapisac ktore kable sa zajete, i wywolac rekurencyjnie
                        SetRoute(originatingAddress, targetAddress, bitrate, excWiresNext, hashKey);
                    }
                    break;
                }
            }

        }

        // Set route for local domain and return startingfreq for other domain
        private int SetLocalRoute(string originatingAddress, string targetAddress, int bitrate, int[] excludedWiresIDs, string hashKey, int startingFreq, int routerId)
        {
            var routeHistory = new List<int[]>();
            var startfrequency = startingFreq;

            var route = CalculateRoute(originatingAddress, targetAddress, excludedWiresIDs);
            if (route == null)
            {
                Console.WriteLine("Can't find route from " + originatingAddress + " to " + targetAddress);
                return -1;
            }

            for (var j = 0; j < route.Length; j++)
            {
                _bufferRouterResponse = null;

                if (j == 0)
                {
                    //wyslij d source routera
                    //Console.WriteLine("WYSYLAM DO PIERWSZEGO EDGE ROUTERA DANE ROUTINGOWE");
                    var ar = CalculateFsUcountFromTo(route[j], FindWireIdFromTo(route[j], route[j + 1], route[j]), bitrate);
                    var ip = String.Format("127.0.1." + route[j]);
                    Send(ip, new AgentData()
                    {
                        Message = AgentComProtocol.ROUTE_FOR_U_EDGE,
                        FsuCount = (int)ar[1],
                        Mod = (Modulation)ar[0],
                        WireId = FindWireIdFromTo(route[j], route[j + 1], route[j]),
                        OriginatingAddress = originatingAddress,
                        TargetAddress = targetAddress,
                        UniqueKey = hashKey,
                        StartingFreq = startfrequency,
                        IsStartEdge = true
                    });
                    var rSid = Int32.Parse(originatingAddress.Substring(originatingAddress.Length - 1, 1));
                    EdgeLocalRouterIDs.Add(hashKey, new int[2] { rSid, -1 });

                    //Console.WriteLine("WYSYLALEM DO PIERWSZEGO EDGE ROUTERA DANE ROUTINGOWE (" + ip + ")");
                }
                else if (j > 0 && j < route.Length - 1)
                {
                    //fsucount, mod, firstwireid,secondwireid, startingfreq dla odbierajacego kabla bo juz obliczone w poprzednim roouterze
                    //wyslij do zwyklych routerow
                    //Console.WriteLine("WYSYLAM DO SRODKOWEGO ROUTERA DANE ROUTINGOWE");
                    var ar0 = CalculateFsUcountFromTo(route[j], FindWireIdFromTo(route[j - 1], route[j], route[j]), bitrate);
                    var ar = CalculateFsUcountFromTo(route[j], FindWireIdFromTo(route[j], route[j + 1], route[j]), bitrate);
                    if ((int)ar[0] == 0 || (int)ar[1] == 0) return -1;
                    var ip = String.Format("127.0.1." + route[j]);
                    Send(ip, new AgentData()
                    {
                        Message = AgentComProtocol.ROUTE_FOR_U,
                        LastFsuCount = (int)ar0[1],
                        LastMod = (Modulation)ar0[0],
                        FsuCount = (int)ar[1],
                        Mod = (Modulation)ar[0],
                        FirstWireId = FindWireIdFromTo(route[j - 1], route[j], route[j]),
                        SecondWireId = FindWireIdFromTo(route[j], route[j + 1], route[j]),
                        StartingFreq = startfrequency
                    });
                    //Console.WriteLine("WYSYLALEM DO SRODKOWEGO ROUTERA DANE ROUTINGOWE (" + ip + ")");
                }
                else if (j == route.Length - 1)
                {
                    //wyslij do destinationIP routra
                    //Console.WriteLine("WYSYLAM DO OSTATNIEGO EDGE ROUTERA DANE ROUTINGOWE");
                    var ar0 = CalculateFsUcountFromTo(route[j], FindWireIdFromTo(route[j - 1], route[j], route[j]), bitrate);
                    var ar = CalculateFsUcountFromTo(route[j], FindWireIdFromTo(route[j], routerId, route[j]), bitrate);
                    if ((int)ar[0] == 0 || (int)ar[1] == 0) return -1;
                    var ip = String.Format("127.0.1." + route[j]);
                    Send(ip, new AgentData()
                    {
                        Message = AgentComProtocol.ROUTE_FOR_U,
                        LastFsuCount = (int)ar0[1],
                        LastMod = (Modulation)ar0[0],
                        FsuCount = (int)ar[1],
                        Mod = (Modulation)ar[0],
                        FirstWireId = FindWireIdFromTo(route[j - 1], route[j], route[j]),
                        SecondWireId = FindWireIdFromTo(route[j], routerId, route[j]),
                        StartingFreq = startfrequency
                    });
                    //Console.WriteLine("WYSYLALEM DO OSTATNIEGO EDGE ROUTERA DANE ROUTINGOWE (" + ip + ")");
                }
                //sprawdz czy dostepne bitrejtyy
                var waitTime = 0;
                while (_bufferRouterResponse == null)
                {
                    //todo a co jak sie zatnie? coś nie dojdzie? router padnie?

                    //w8 na odp od routera
                    Thread.Sleep(50);
                    //stestowac to
                    /*waitTime += 50;
                    if(waitTime > 10000)
                        return;*/
                }

                if (_bufferRouterResponse.Message.Equals(ExtSrc.AgentComProtocol.CONNECTION_IS_ON))
                {
                    //startfrequency = _bufferRouterResponse.StartingFreq;
                    //dodawanie do routeHistory
                    if (j < route.Length - 1)
                    {
                        //Console.WriteLine("Add to route {0} wire {1} and slot {2} ", route[j], FindWireId(route[j], route[j + 1]), bufferRouterResponse.FSid);
                        routeHistory.Add(new int[3] { route[j], FindWireIdFromTo(route[j], route[j + 1], route[j]), _bufferRouterResponse.FSid });
                    }
                    if (j == route.Length - 1)
                    {
                        //routeHistory.Add(new int[3] { route[j], FindWireIdFromTo(route[j - 1], route[j], route[j]), _bufferRouterResponse.FSid });
                        routeHistory.Add(new int[3] { route[j], FindWireIdFromTo(route[j], routerId, route[j]), _bufferRouterResponse.FSid });
                        RouteHistoryList.Add(new String[3] { originatingAddress, targetAddress, hashKey }, routeHistory);

                        var rRid = Int32.Parse(targetAddress.Substring(targetAddress.Length - 1, 1));
                        // edgeRouterIDs.Add(hashKey, new int[2] { rSid, rRid });

                        var tmp = EdgeLocalRouterIDs[hashKey];
                        //todo tutaj do edgeRouterIDs dopisuje rRid, a co gdy CONNECTION_UNAVAILABLE ? 
                        //todo wpis zostanie nie kompletny, gdzie usuwanie? w disroute brak tego
                        EdgeLocalRouterIDs[hashKey] = new int[] { tmp[0], rRid };
                        Console.WriteLine("Route local set.");
                        foreach (var rh in routeHistory)
                        {
                            Console.WriteLine("Router {0} wire {1} and slot {2}. ", rh[0], rh[1], rh[2]);
                        }
                        return startfrequency;
                    }
                }
                else if (_bufferRouterResponse.Message.Equals(ExtSrc.AgentComProtocol.CONNECTION_UNAVAILABLE))
                {
                    //rozlaczyc to co juz zestawione i zaczac liczyc dijkstre bez kabla ktory nie mial miejsca
                    if (Disroute(routeHistory, hashKey))
                    {
                        var excWiresNext = new int[excludedWiresIDs.Length + 1];
                        for (var i = 0; i < excludedWiresIDs.Length; i++)
                        {
                            excWiresNext[i] = excludedWiresIDs[i];
                        }
                        excWiresNext[excWiresNext.Length - 1] = FindWireIdFromTo(route[j], route[j + 1], route[j]);
                        // gdy rozłączone
                        //wyjsc z petli for, zapisac ktore kable sa zajete, i wywolac rekurencyjnie
                        SetLocalRoute(originatingAddress, targetAddress, bitrate, excWiresNext, hashKey, startfrequency, routerId);
                    }
                    break;
                }
            }
            return -1;
        }

        // Set route for remote request from other domain
        private int SetRemoteRoute(string originatingAddr, string targetAddress, int bitrate, int[] excludedWiresIDs, string hashKey, int startingFreq, int routerId)
        {
            var originatingId = Int32.Parse(originatingAddr.Substring(originatingAddr.Length - 1, 1));
            // set local path
            var localTargetId = DomainToTargetConnector(originatingId);
            var originatingAddress = "127.0.1." + localTargetId;

            var routeHistory = new List<int[]>();
            var startfrequency = startingFreq;

            var route = CalculateRoute(originatingAddress, targetAddress, excludedWiresIDs);
            if (route == null)
            {
                Console.WriteLine("Can't find route from " + originatingAddress + " to " + targetAddress);
                return -1;
            }

            for (var j = 0; j < route.Length; j++)
            {
                _bufferRouterResponse = null;

                if (j == 0)
                {
                    //wyslij d source routera
                    //Console.WriteLine("WYSYLAM DO PIERWSZEGO EDGE ROUTERA DANE ROUTINGOWE");
                    var ar0 = CalculateFsUcountFromTo(route[j], FindWireIdFromTo(routerId, route[j], route[j]), bitrate);
                    var ar = CalculateFsUcountFromTo(route[j], FindWireIdFromTo(route[j], route[j + 1], route[j]), bitrate);
                    if ((int)ar[0] == 0 || (int)ar[1] == 0) return -1;
                    var ip = String.Format("127.0.1." + route[j]);
                    Send(ip, new AgentData()
                    {
                        Message = AgentComProtocol.ROUTE_FOR_U,
                        LastFsuCount = (int)ar0[1],
                        LastMod = (Modulation)ar0[0],
                        FsuCount = (int)ar[1],
                        Mod = (Modulation)ar[0],
                        FirstWireId = FindWireIdFromTo(routerId, route[j], route[j]),
                        SecondWireId = FindWireIdFromTo(route[j], route[j + 1], route[j]),
                        StartingFreq = startfrequency
                    });
                    var rSid = Int32.Parse(originatingAddress.Substring(originatingAddress.Length - 1, 1));
                    EdgeRemoteRouterIDs.Add(hashKey, new int[2] { rSid, -1 });
                    //Console.WriteLine("WYSYLALEM DO PIERWSZEGO EDGE ROUTERA DANE ROUTINGOWE (" + ip + ")");
                }
                else if (j > 0 && j < route.Length - 1)
                {
                    //fsucount, mod, firstwireid,secondwireid, startingfreq dla odbierajacego kabla bo juz obliczone w poprzednim roouterze
                    //wyslij do zwyklych routerow
                    //Console.WriteLine("WYSYLAM DO SRODKOWEGO ROUTERA DANE ROUTINGOWE");
                    var ar0 = CalculateFsUcountFromTo(route[j], FindWireIdFromTo(route[j - 1], route[j], route[j]), bitrate);
                    var ar = CalculateFsUcountFromTo(route[j], FindWireIdFromTo(route[j], route[j + 1], route[j]), bitrate);
                    if ((int)ar[0] == 0 || (int)ar[1] == 0) return -1;
                    var ip = String.Format("127.0.1." + route[j]);
                    Send(ip, new AgentData()
                    {
                        Message = AgentComProtocol.ROUTE_FOR_U,
                        LastFsuCount = (int)ar0[1],
                        LastMod = (Modulation)ar0[0],
                        FsuCount = (int)ar[1],
                        Mod = (Modulation)ar[0],
                        FirstWireId = FindWireIdFromTo(route[j - 1], route[j], route[j]),
                        SecondWireId = FindWireIdFromTo(route[j], route[j + 1], route[j]),
                        StartingFreq = startfrequency
                    });
                    //Console.WriteLine("WYSYLALEM DO SRODKOWEGO ROUTERA DANE ROUTINGOWE (" + ip + ")");
                }
                else if (j == route.Length - 1)
                {
                    //wyslij do destinationIP routra
                    //Console.WriteLine("WYSYLAM DO OSTATNIEGO EDGE ROUTERA DANE ROUTINGOWE");
                    var ar0 = CalculateFsUcountFromTo(route[j], FindWireIdFromTo(route[j - 1], route[j], route[j]), bitrate);
                    var ip = String.Format("127.0.1." + route[j]);
                    Send(ip, new AgentData()
                    {
                        Message = AgentComProtocol.ROUTE_FOR_U_EDGE,
                        FsuCount = (int)ar0[1],
                        Mod = (Modulation)ar0[0],
                        WireId = FindWireIdFromTo(route[j - 1], route[j], route[j]),
                        OriginatingAddress = originatingAddress,
                        TargetAddress = targetAddress,
                        UniqueKey = hashKey,
                        StartingFreq = startfrequency,
                        IsStartEdge = false
                    });
                    //Console.WriteLine("WYSYLALEM DO OSTATNIEGO EDGE ROUTERA DANE ROUTINGOWE (" + ip + ")");
                }
                //sprawdz czy dostepne bitrejtyy
                var waitTime = 0;
                while (_bufferRouterResponse == null)
                {
                    //todo a co jak sie zatnie? coś nie dojdzie? router padnie?

                    //w8 na odp od routera
                    Thread.Sleep(50);
                    //stestowac to
                    /*waitTime += 50;
                    if(waitTime > 10000)
                        return;*/
                }

                if (_bufferRouterResponse.Message.Equals(ExtSrc.AgentComProtocol.CONNECTION_IS_ON))
                {
                    //startfrequency = _bufferRouterResponse.StartingFreq;
                    //dodawanie do routeHistory
                    if (j < route.Length - 1)
                    {
                        //Console.WriteLine("Add to route {0} wire {1} and slot {2} ", route[j], FindWireId(route[j], route[j + 1]), bufferRouterResponse.FSid);
                        routeHistory.Add(new int[3] { route[j], FindWireIdFromTo(route[j], route[j + 1], route[j]), _bufferRouterResponse.FSid });
                    }
                    if (j == route.Length - 1)
                    {
                        routeHistory.Add(new int[3] { route[j], FindWireIdFromTo(route[j - 1], route[j], route[j]), _bufferRouterResponse.FSid });
                        RouteHistoryList.Add(new String[3] { originatingAddress, targetAddress, hashKey }, routeHistory);

                        var rRid = Int32.Parse(targetAddress.Substring(targetAddress.Length - 1, 1));
                        // edgeRouterIDs.Add(hashKey, new int[2] { rSid, rRid });

                        var tmp = EdgeRemoteRouterIDs[hashKey];
                        //todo tutaj do edgeRouterIDs dopisuje rRid, a co gdy CONNECTION_UNAVAILABLE ? 
                        //todo wpis zostanie nie kompletny, gdzie usuwanie? w disroute brak tego
                        EdgeRemoteRouterIDs[hashKey] = new int[] { tmp[0], rRid };
                        Console.WriteLine("Route local set.");
                        foreach (var rh in routeHistory)
                        {
                            Console.WriteLine("Router {0} wire {1} and slot {2}. ", rh[0], rh[1], rh[2]);
                        }
                        return startfrequency;
                    }
                }
                else if (_bufferRouterResponse.Message.Equals(ExtSrc.AgentComProtocol.CONNECTION_UNAVAILABLE))
                {
                    //rozlaczyc to co juz zestawione i zaczac liczyc dijkstre bez kabla ktory nie mial miejsca
                    if (Disroute(routeHistory, hashKey))
                    {
                        var excWiresNext = new int[excludedWiresIDs.Length + 1];
                        for (var i = 0; i < excludedWiresIDs.Length; i++)
                        {
                            excWiresNext[i] = excludedWiresIDs[i];
                        }
                        excWiresNext[excWiresNext.Length - 1] = FindWireIdFromTo(route[j], route[j + 1], route[j]);
                        // gdy rozłączone
                        //wyjsc z petli for, zapisac ktore kable sa zajete, i wywolac rekurencyjnie
                        SetRemoteRoute(originatingAddress, targetAddress, bitrate, excWiresNext, hashKey, startfrequency, routerId);
                    }
                    break;
                }
            }
            return -1;
        }

        private void SetDomainRoute(String originatingAddress, String targetAddress, int bitrate, int[] excludedWiresIDs, String hashKey)
        {
            var targetId = Int32.Parse(targetAddress.Substring(targetAddress.Length - 1, 1));
            // set local path
            var localTargetId = DomainToTargetConnector(targetId);
            Console.WriteLine("we are going to send :" + "127.0.1." + localTargetId+" ask about permission to route.");
            var fsuCount = (int) Math.Round((double) (bitrate)/Convert.ToDouble(NewWire.FREQ_SLOT_UNIT));
            _bufferRouterResponse = null;
            Send("127.0.1." + localTargetId, new AgentData()
            {
                Message = AgentComProtocol.DOMAIN_CAN_WE_SET_ROUTE,
                DomainRouterID = Int32.Parse(originatingAddress.Substring(originatingAddress.Length - 1, 1)),
                TargetAddress = targetAddress,
                FsuCount = fsuCount,
                UniqueKey = hashKey,
            });
            while (_bufferRouterResponse == null) Thread.Sleep(50);
            var tmpList = _bufferRouterResponse.StartingFreqsPool;
            var tmpRouterId = _bufferRouterResponse.DomainRouterID;
            var route = CalculateRoute(originatingAddress, "127.0.1." + localTargetId, null);
            var startingFreqs = CalculateAvaibleStartingFreqs(route, fsuCount);
            startingFreqs.AddRange(tmpList);
            var startingFreqFinal = FindBestStartingFreq(startingFreqs, fsuCount);
            Console.WriteLine("Domain response we can set route. startingFreqFinal=" + startingFreqFinal);
            SetLocalRoute(originatingAddress, "127.0.1." + localTargetId, bitrate, excludedWiresIDs, hashKey, startingFreqFinal, tmpRouterId);
            // set remote path
            Console.WriteLine("Local route set.");
            _bufferRouterResponse = null;
            Send("127.0.1."+localTargetId, new AgentData()
            {
                Message = AgentComProtocol.DOMAIN_SET_ROUTE_FOR_ME,
                DomainRouterID = localTargetId,
                OriginatingAddress = originatingAddress,
                TargetAddress = targetAddress,
                Bitrate = bitrate,
                UniqueKey = hashKey,
                StartingFreq = startingFreqFinal
            });
            while (_bufferRouterResponse == null) Thread.Sleep(50);
            Console.WriteLine("Remote domain route set.");
        }

        private static int FindBestStartingFreq(List<List<int[]>> startingFreqs, int fsuCount)
        {
            int result=0, counter=0;
            var spectralWidth = Enumerable.Repeat(1 , 1000).ToArray();
            foreach (var listOfRanges in startingFreqs)
            {
                foreach (var range in listOfRanges)
                {
                    for (var i = 0; i < 1000; i++)
                    {
                        if (i >= range[0] && i <= range[1])
                        {
                            spectralWidth[i] = NewWire.EMPTY_VALUE;
                        }
                    }
                }
            }
            for (var i = 0; i < spectralWidth.Length; i++)
            {
                if (spectralWidth[i] == NewWire.EMPTY_VALUE)
                {
                    counter++;
                    if (counter == 1)
                        result = i;
                }
                else counter = 0;

                if (counter == fsuCount)
                    return result;
            }
            return -1;
        }

        private List<List<int[]>> CalculateAvaibleStartingFreqs(int[] route, int fsucount)
        {
            Console.WriteLine("CalculateAvaibleStartingFreqs route size" + route.GetLength(0)+" fsucount "+fsucount);
            var result = new List<List<int[]>>();
            for (var j = 0; j < route.GetLength(0); j++)
            {
                _bufferRouterResponse = null;
                if (j == route.GetLength(0) - 1)
                {
                    Send("127.0.1." + route[j], new AgentData()
                    {
                        Message = AgentComProtocol.AVAIBLE_STARTING_FREQS,
                        WireId = FindWireIdFromTo(route[j], route[j-1], route[j]),
                        FsuCount = fsucount
                    });
                }
                else
                {
                    Send("127.0.1." + route[j], new AgentData()
                    {
                        Message = AgentComProtocol.AVAIBLE_STARTING_FREQS,
                        WireId = FindWireIdFromTo(route[j], route[j + 1], route[j]),
                        FsuCount = fsucount
                    });
                }
                
                while (_bufferRouterResponse == null) Thread.Sleep(50);
                Console.WriteLine("Got response from node with ranges, size " + _bufferRouterResponse.StartingFreqs.Count);
                result.Add(_bufferRouterResponse.StartingFreqs);
            }
            return result;
        }

        private int[] CalculateRoute(String sourceIP, String destinationIP, int[] excludedWiresIDs)
        {
            var id1 = Int32.Parse(sourceIP.Substring(sourceIP.Length - 1, 1));
            var id2 = Int32.Parse(destinationIP.Substring(destinationIP.Length - 1, 1));
            var distinctData = DijkstraDataList.GroupBy(x => x, new DijkstraEqualityComparer()).SelectMany(grp => grp.Skip(1)).ToList();
//            List<int> wireIdOnline = new List<int>();
//            foreach (ExtSrc.DijkstraData dd in distinctData)
//            {
//                wireIdOnline.Add(dd.wireID);
//            }
            List<int[]> wireIdOnline2 = new List<int[]>();
            foreach (ExtSrc.DijkstraData dd in distinctData)
            {
                wireIdOnline2.Add(new int[]{dd.RouterIds[0], dd.RouterIds[1], dd.wireDistance});
            }
            // todo excluded wires musze miec cos wiecej niz samo ID lokalne
            //excluding wires
//            if (excludedWiresIDs != null)
//            {
//                foreach (int d in excludedWiresIDs)
//                {
//                    wireIdOnline2.Remove(d);
//                }
//            }



//            List<int[]> routerIds = new List<int[]>();
//            foreach (int i in wireIdOnline)
//            {
//                int[] tmpRoute = new int[3];
//                int ite = 0;
//                foreach (ExtSrc.DijkstraData dd in DijkstraDataList)
//                {
//                    if (dd.wireID == i)
//                    {
//                        tmpRoute[ite] = dd.routerID;
//                        ite++;
//                        if (ite == 1) tmpRoute[ite + 1] = dd.wireDistance;
//                    }
//                }
//                routerIds.Add(tmpRoute);
//            }
//            int[][] dataReadyForDijkstra = new int[routerIds.Count][];
//            for (int i = 0; i < routerIds.Count; i++)
//            {
//                dataReadyForDijkstra[i] = new int[] { routerIds.ElementAt(i)[0], routerIds.ElementAt(i)[1], routerIds.ElementAt(i)[2] };
//            }

            int[][] dataReadyForDijkstra = new int[wireIdOnline2.Count][];
            for (int i = 0; i < wireIdOnline2.Count; i++)
            {
                dataReadyForDijkstra[i] = new int[] { wireIdOnline2.ElementAt(i)[0], wireIdOnline2.ElementAt(i)[1], wireIdOnline2.ElementAt(i)[2] };
            }

            int[,] dta = new int[dataReadyForDijkstra.GetLength(0), 3];
            for (int i = 0; i < dataReadyForDijkstra.GetLength(0); i++)
            {
                dta[i, 0] = dataReadyForDijkstra[i][0];
                dta[i, 1] = dataReadyForDijkstra[i][1];
                dta[i, 2] = dataReadyForDijkstra[i][2];

            }

            int[] route = Dijkstra.evaluate(dta, id1, id2);

            return route;
        }

        public void SetRouteManually(String clientSourceIP, String ClientDestinationIP, int bitrate, int[] excludedWiresIDs, String hashKey, int[] route, int startF = -1)
        {
            List<int[]> routeHistory = new List<int[]>();
            int startfrequency = -1;
            // zamiana ip koncowego klienta na ip jego routera
            /* String senderRouterIP;
             clientMap.TryGetValue(clientSourceIP, out senderRouterIP);
             if (senderRouterIP == null)
             {
                 Console.WriteLine("Setting route error.");
                 return;
             }
             String recipientRouterIP;
             clientMap.TryGetValue(ClientDestinationIP, out recipientRouterIP);
             if (recipientRouterIP == null)
             {
                 Console.WriteLine("Setting route error.");
                 return;
             }*/
            //String Client = Int32.Parse(sourceIP.Substring(sourceIP.Length - 1, 1));
            int ClientSenderID = Int32.Parse(clientSourceIP.Substring(clientSourceIP.Length - 1, 1));
            //int RouterRecipientID = Int32.Parse(destinationIP.Substring(destinationIP.Length - 1, 1));
            int ClientRecipientID = Int32.Parse(ClientDestinationIP.Substring(ClientDestinationIP.Length - 1, 1));
            /* int[] route = calculateRoute(senderRouterIP, recipientRouterIP, excludedWiresIDs);
             if (route == null)
             {
                 Console.WriteLine("AKTUALNIE NIE MOŻNA ZNALEŹĆ DROGI Z " + clientSourceIP + " DO " + ClientDestinationIP);
                 return;
             }*/
           // FSidCounter++;
            for (int j = 0; j < route.Length; j++)
            {
                _bufferRouterResponse = null;

                if (j == 0)
                {
                    //wyslij d source routera
                    //Console.WriteLine("WYSYLAM DO PIERWSZEGO EDGE ROUTERA DANE ROUTINGOWE");
                    ArrayList ar = CalculateFsUcountFromTo(route[j], FindWireIdFromTo(route[j], route[j + 1], route[j]), bitrate);
                    String ip = String.Format("127.0.1." + route[j]);
                    Send(ip, new ExtSrc.AgentData(){
                        Message = AgentComProtocol.ROUTE_FOR_U_EDGE, 
                        FsuCount = (int)ar[1], 
                        Mod = (Modulation)ar[0],
                        WireId = FindWireIdFromTo(route[j], route[j + 1], route[j]),
                        ClientSocketId = ClientSenderID,
                        OriginatingAddress = clientSourceIP,
                        TargetAddress = ClientDestinationIP,
                        UniqueKey = hashKey,
                        StartingFreq = startF,
                        IsStartEdge = true
                    });
                    int rSid = Int32.Parse(clientSourceIP.Substring(clientSourceIP.Length - 1, 1));
                    EdgeRouterIDs.Add(hashKey, new int[2] { rSid, -1 });

                    //Console.WriteLine("WYSYLALEM DO PIERWSZEGO EDGE ROUTERA DANE ROUTINGOWE (" + ip + ")");
                }
                else if (j > 0 && j < route.Length - 1)
                {
                    //fsucount, mod, firstwireid,secondwireid, startingfreq dla odbierajacego kabla bo juz obliczone w poprzednim roouterze
                    //wyslij do zwyklych routerow
                    //Console.WriteLine("WYSYLAM DO SRODKOWEGO ROUTERA DANE ROUTINGOWE");
                    ArrayList ar0 = CalculateFsUcountFromTo(route[j], FindWireIdFromTo(route[j - 1], route[j], route[j]), bitrate);
                    ArrayList ar = CalculateFsUcountFromTo(route[j], FindWireIdFromTo(route[j], route[j + 1], route[j]), bitrate);
                    if ((int)ar[0] == 0 || (int)ar[1] == 0) return;
                    String ip = String.Format("127.0.1." + route[j]);
                    Send(ip, new AgentData()
                    {
                        Message = AgentComProtocol.ROUTE_FOR_U,
                        LastFsuCount = (int) ar0[1],
                        LastMod = (Modulation) ar0[0],
                        FsuCount = (int) ar[1],
                        Mod = (Modulation) ar[0],
                        FirstWireId = FindWireIdFromTo(route[j - 1], route[j], route[j]),
                        SecondWireId = FindWireIdFromTo(route[j], route[j + 1], route[j]),
                        StartingFreq = startfrequency
                    });
                    //Console.WriteLine("WYSYLALEM DO SRODKOWEGO ROUTERA DANE ROUTINGOWE (" + ip + ")");
                }
                else if (j == route.Length - 1)
                {
                    //wyslij do destinationIP routra
                    //Console.WriteLine("WYSYLAM DO OSTATNIEGO EDGE ROUTERA DANE ROUTINGOWE");
                    ArrayList ar0 = CalculateFsUcountFromTo(route[j], FindWireIdFromTo(route[j - 1], route[j], route[j]), bitrate);
                    String ip = String.Format("127.0.1." + route[j]);
                    Send(ip, new ExtSrc.AgentData()
                    {
                        Message = AgentComProtocol.ROUTE_FOR_U_EDGE, 
                        FsuCount = (int)ar0[1], 
                        Mod = (Modulation)ar0[0],
                        WireId = FindWireIdFromTo(route[j - 1], route[j], route[j]),
                        ClientSocketId = ClientRecipientID,
                        OriginatingAddress = clientSourceIP,
                        TargetAddress = ClientDestinationIP,
                        UniqueKey = hashKey,
                        StartingFreq = startfrequency,
                        IsStartEdge = false

                    });
                    //Console.WriteLine("WYSYLALEM DO OSTATNIEGO EDGE ROUTERA DANE ROUTINGOWE (" + ip + ")");
                }
                //sprawdz czy dostepne bitrejtyy
                var waitTime = 0;
                while (_bufferRouterResponse == null)
                {
                    //todo a co jak sie zatnie? coś nie dojdzie? router padnie?

                    //w8 na odp od routera
                    Thread.Sleep(50);
                    //stestowac to
                    /*waitTime += 50;
                    if(waitTime > 10000)
                        return;*/
                }

                if (_bufferRouterResponse.Message.Equals(ExtSrc.AgentComProtocol.CONNECTION_IS_ON))
                {
                    startfrequency = _bufferRouterResponse.StartingFreq;
                    //dodawanie do routeHistory
                    if (j < route.Length - 1)
                    {
                        //Console.WriteLine("Add to route {0} wire {1} and slot {2} ", route[j], FindWireId(route[j], route[j + 1]), bufferRouterResponse.FSid);
                        routeHistory.Add(new int[3] { route[j], FindWireIdFromTo(route[j], route[j + 1], route[j]), _bufferRouterResponse.FSid });
                    }
                    if (j == route.Length - 1)
                    {
                        routeHistory.Add(new int[3] { route[j], FindWireIdFromTo(route[j - 1], route[j], route[j]), _bufferRouterResponse.FSid });
                        RouteHistoryList.Add(new String[3] { clientSourceIP, ClientDestinationIP, hashKey }, routeHistory);

                        int rRid = Int32.Parse(ClientDestinationIP.Substring(ClientDestinationIP.Length - 1, 1));
                        // edgeRouterIDs.Add(hashKey, new int[2] { rSid, rRid });

                        int[] tmp = EdgeRouterIDs[hashKey];
                        //todo tutaj do edgeRouterIDs dopisuje rRid, a co gdy CONNECTION_UNAVAILABLE ? 
                        //todo wpis zostanie nie kompletny, gdzie usuwanie? w disroute brak tego
                        EdgeRouterIDs[hashKey] = new int[] { tmp[0], rRid };
                        Console.WriteLine("Route set.");
                        foreach (int[] rh in routeHistory)
                        {
                            Console.WriteLine("Router {0} wire {1} and slot {2}. ", rh[0], rh[1], rh[2]);
                        }
                        return;
                    }
                }
                else if (_bufferRouterResponse.Message.Equals(ExtSrc.AgentComProtocol.CONNECTION_UNAVAILABLE))
                {
                    //rozlaczyc to co juz zestawione i zaczac liczyc dijkstre bez kabla ktory nie mial miejsca
                    if (Disroute(routeHistory, hashKey))
                    {
                        int[] excWiresNext = new int[excludedWiresIDs.Length + 1];
                        for (int i = 0; i < excludedWiresIDs.Length; i++)
                        {
                            excWiresNext[i] = excludedWiresIDs[i];
                        }
                        excWiresNext[excWiresNext.Length - 1] = FindWireIdFromTo(route[j], route[j + 1], route[j]);
                        // gdy rozłączone
                        //wyjsc z petli for, zapisac ktore kable sa zajete, i wywolac rekurencyjnie
                        SetRoute(clientSourceIP, ClientDestinationIP, bitrate, excWiresNext, hashKey);
                    }
                    break;
                }
            }

        }

        public bool Disroute(String uniKey)
        {
            var routeHist = RouteHistoryList.Where(d => d.Key[2].Equals(uniKey)).Select(d => d.Value).FirstOrDefault();
            if (routeHist != null)
            {
                if (EdgeRouterIDs.ContainsKey(uniKey))
                {
                    Console.WriteLine("EdgeRouterIDs");
                    Disroute(routeHist, uniKey);
                }
                else if (EdgeLocalRouterIDs.ContainsKey(uniKey))
                {
                    Console.WriteLine("EdgeLocalRouterIDs");
                    DisrouteLocal(routeHist, uniKey);
                }
                else if (EdgeRemoteRouterIDs.ContainsKey(uniKey))
                {
                    Console.WriteLine("EdgeRemoteRouterIDs");
                    DisrouteRemote(routeHist, uniKey);
                }
                var key = RouteHistoryList.FirstOrDefault(d => d.Value.Equals(routeHist)).Key;
                RouteHistoryList.Remove(key);
                return true;
            }
            return false;
        }

        private bool Disroute(List<int[]> routeHistory, String hashKey)
        {
            int[] edgeRouters;
            if (EdgeRouterIDs.TryGetValue(hashKey, out edgeRouters))
            {
                for (int i = routeHistory.Count - 1; i >= 0; i--)
                {
                    Console.WriteLine("ONE DISROUTE MSG IS GOING TO BE SENT : " + routeHistory.ElementAt(i)[0] + " -> " + routeHistory.ElementAt(i)[1] + " -> " + routeHistory.ElementAt(i)[2]);
                    if ((edgeRouters[0] != routeHistory.ElementAt(i)[0]) && (edgeRouters[1] != routeHistory.ElementAt(i)[0]))
                    {
                        //   agentData.firstWireID, agentData.FSid, agentData.secondWireID, agentData.secondFSid
                        _bufferRouterResponse = null;
                        Send(String.Format("127.0.1." + routeHistory.ElementAt(i)[0]), new AgentData()
                        {
                            Message = AgentComProtocol.DISROUTE, 
                            FirstWireId = routeHistory.ElementAt(i)[1], 
                            FSid = routeHistory.ElementAt(i)[2]
                        });
                        var waitTime = 0;
                        while (_bufferRouterResponse == null)
                        {
                            //w8 na odp od routera
                            Thread.Sleep(50);
                            waitTime += 50;
                            if (waitTime > Timeout) break;
                        }
                        if (_bufferRouterResponse != null && _bufferRouterResponse.Message.Equals(ExtSrc.AgentComProtocol.DISROUTE_IS_DONE))
                            //Console.WriteLine("DISROUTE FOR " + String.Format("127.0.1." + routeHistory.ElementAt(i)[0]) + " IS DONE");
                            Console.WriteLine("Disoute done.");
                        else if (_bufferRouterResponse == null || _bufferRouterResponse.Message.Equals(ExtSrc.AgentComProtocol.DISROUTE_ERROR))
                        {
                            //Console.WriteLine("DISROUTE FOR " + String.Format("127.0.1." + routeHistory.ElementAt(i)[0]) + " ERROR!!!");
                            Console.WriteLine("Disroute error.");
                            continue; //todo tu był return test aby disroutowało sie do konca nawet jak jest ktorys router off
                        }
                    }
                    else
                    {
                        _bufferRouterResponse = null;
                        Send(String.Format("127.0.1." + routeHistory.ElementAt(i)[0]), new AgentData()
                        {
                            Message = AgentComProtocol.DISROUTE_EDGE,
                            FirstWireId = routeHistory.ElementAt(i)[1],
                            FSid = routeHistory.ElementAt(i)[2],
                            UniqueKey = hashKey
                        });
                        var waitTime = 0;
                        while (_bufferRouterResponse == null)
                        {
                            //w8 na odp od routera
                            Thread.Sleep(50);
                            waitTime += 50;
                            if (waitTime > Timeout) break;
                        }

                        if (_bufferRouterResponse != null && _bufferRouterResponse.Message.Equals(ExtSrc.AgentComProtocol.DISROUTE_EDGE_IS_DONE))
                            //Console.WriteLine("DISROUTE EDGE FOR " + String.Format("127.0.1." + routeHistory.ElementAt(0)[0]) + " IS DONE");
                            Console.WriteLine("Disoute done.");
                        else if (_bufferRouterResponse == null || _bufferRouterResponse.Message.Equals(ExtSrc.AgentComProtocol.DISROUTE_ERROR_EDGE))
                        {
                            //Console.WriteLine("DISROUTE EDGE FOR " + String.Format("127.0.1." + routeHistory.ElementAt(0)[0]) + " ERROR!!!");
                            Console.WriteLine("Disroute error.");
                            continue; //todo tu był return test aby disroutowało sie do konca nawet jak jest ktorys router off
                        }

                    }

                }
                EdgeRouterIDs.Remove(hashKey);
                return true;
            }
            EdgeRouterIDs.Remove(hashKey);
            return false;
        }

        private bool DisrouteLocal(List<int[]> routeHistory, String hashKey)
        {
            int[] edgeRouters;
            if (EdgeLocalRouterIDs.TryGetValue(hashKey, out edgeRouters))
            {
                for (int i = routeHistory.Count - 1; i >= 0; i--)
                {
                    Console.WriteLine("ONE DISROUTE MSG IS GOING TO BE SENT : " + routeHistory.ElementAt(i)[0] + " -> " + routeHistory.ElementAt(i)[1] + " -> " + routeHistory.ElementAt(i)[2]);
                    if ((edgeRouters[0] != routeHistory.ElementAt(i)[0]))
                    {
                        //   agentData.firstWireID, agentData.FSid, agentData.secondWireID, agentData.secondFSid
                        _bufferRouterResponse = null;
                        Send(String.Format("127.0.1." + routeHistory.ElementAt(i)[0]), new AgentData()
                        {
                            Message = AgentComProtocol.DISROUTE,
                            FirstWireId = routeHistory.ElementAt(i)[1],
                            FSid = routeHistory.ElementAt(i)[2]
                        });
                        var waitTime = 0;
                        while (_bufferRouterResponse == null)
                        {
                            //w8 na odp od routera
                            Thread.Sleep(50);
                            waitTime += 50;
                            if (waitTime > Timeout) break;
                        }
                        if (_bufferRouterResponse != null && _bufferRouterResponse.Message.Equals(ExtSrc.AgentComProtocol.DISROUTE_IS_DONE))
                            //Console.WriteLine("DISROUTE FOR " + String.Format("127.0.1." + routeHistory.ElementAt(i)[0]) + " IS DONE");
                            Console.WriteLine("Disoute done.");
                        else if (_bufferRouterResponse == null || _bufferRouterResponse.Message.Equals(ExtSrc.AgentComProtocol.DISROUTE_ERROR))
                        {
                            //Console.WriteLine("DISROUTE FOR " + String.Format("127.0.1." + routeHistory.ElementAt(i)[0]) + " ERROR!!!");
                            Console.WriteLine("Disroute error.");
                            continue; //todo tu był return test aby disroutowało sie do konca nawet jak jest ktorys router off
                        }
                    }
                    else
                    {
                        _bufferRouterResponse = null;
                        Send(String.Format("127.0.1." + routeHistory.ElementAt(i)[0]), new AgentData()
                        {
                            Message = AgentComProtocol.DISROUTE_EDGE,
                            FirstWireId = routeHistory.ElementAt(i)[1],
                            FSid = routeHistory.ElementAt(i)[2],
                            UniqueKey = hashKey
                        });
                        var waitTime = 0;
                        while (_bufferRouterResponse == null)
                        {
                            //w8 na odp od routera
                            Thread.Sleep(50);
                            waitTime += 50;
                            if (waitTime > Timeout) break;
                        }

                        if (_bufferRouterResponse != null && _bufferRouterResponse.Message.Equals(ExtSrc.AgentComProtocol.DISROUTE_EDGE_IS_DONE))
                            //Console.WriteLine("DISROUTE EDGE FOR " + String.Format("127.0.1." + routeHistory.ElementAt(0)[0]) + " IS DONE");
                            Console.WriteLine("Disoute done.");
                        else if (_bufferRouterResponse == null || _bufferRouterResponse.Message.Equals(ExtSrc.AgentComProtocol.DISROUTE_ERROR_EDGE))
                        {
                            //Console.WriteLine("DISROUTE EDGE FOR " + String.Format("127.0.1." + routeHistory.ElementAt(0)[0]) + " ERROR!!!");
                            Console.WriteLine("Disroute error.");
                            continue; //todo tu był return test aby disroutowało sie do konca nawet jak jest ktorys router off
                        }

                    }

                }
                EdgeLocalRouterIDs.Remove(hashKey);
                return true;
            }
            EdgeLocalRouterIDs.Remove(hashKey);
            return false;
        }

        private bool DisrouteRemote(List<int[]> routeHistory, String hashKey)
        {
            int[] edgeRouters;
            if (EdgeRemoteRouterIDs.TryGetValue(hashKey, out edgeRouters))
            {
                for (int i = routeHistory.Count - 1; i >= 0; i--)
                {
                    Console.WriteLine("ONE DISROUTE MSG IS GOING TO BE SENT : " + routeHistory.ElementAt(i)[0] + " -> " + routeHistory.ElementAt(i)[1] + " -> " + routeHistory.ElementAt(i)[2]);
                    if ((edgeRouters[1] != routeHistory.ElementAt(i)[0]))
                    {
                        //   agentData.firstWireID, agentData.FSid, agentData.secondWireID, agentData.secondFSid
                        _bufferRouterResponse = null;
                        Send(String.Format("127.0.1." + routeHistory.ElementAt(i)[0]), new AgentData()
                        {
                            Message = AgentComProtocol.DISROUTE,
                            FirstWireId = routeHistory.ElementAt(i)[1],
                            FSid = routeHistory.ElementAt(i)[2]
                        });
                        var waitTime = 0;
                        while (_bufferRouterResponse == null)
                        {
                            //w8 na odp od routera
                            Thread.Sleep(50);
                            waitTime += 50;
                            if (waitTime > Timeout) break;
                        }
                        if (_bufferRouterResponse != null && _bufferRouterResponse.Message.Equals(ExtSrc.AgentComProtocol.DISROUTE_IS_DONE))
                            //Console.WriteLine("DISROUTE FOR " + String.Format("127.0.1." + routeHistory.ElementAt(i)[0]) + " IS DONE");
                            Console.WriteLine("Disoute done.");
                        else if (_bufferRouterResponse == null || _bufferRouterResponse.Message.Equals(ExtSrc.AgentComProtocol.DISROUTE_ERROR))
                        {
                            //Console.WriteLine("DISROUTE FOR " + String.Format("127.0.1." + routeHistory.ElementAt(i)[0]) + " ERROR!!!");
                            Console.WriteLine("Disroute error.");
                            continue; //todo tu był return test aby disroutowało sie do konca nawet jak jest ktorys router off
                        }
                    }
                    else
                    {
                        _bufferRouterResponse = null;
                        Send(String.Format("127.0.1." + routeHistory.ElementAt(i)[0]), new AgentData()
                        {
                            Message = AgentComProtocol.DISROUTE_EDGE,
                            FirstWireId = routeHistory.ElementAt(i)[1],
                            FSid = routeHistory.ElementAt(i)[2],
                            UniqueKey = hashKey
                        });
                        var waitTime = 0;
                        while (_bufferRouterResponse == null)
                        {
                            //w8 na odp od routera
                            Thread.Sleep(50);
                            waitTime += 50;
                            if (waitTime > Timeout) break;
                        }

                        if (_bufferRouterResponse != null && _bufferRouterResponse.Message.Equals(ExtSrc.AgentComProtocol.DISROUTE_EDGE_IS_DONE))
                            //Console.WriteLine("DISROUTE EDGE FOR " + String.Format("127.0.1." + routeHistory.ElementAt(0)[0]) + " IS DONE");
                            Console.WriteLine("Disoute done.");
                        else if (_bufferRouterResponse == null || _bufferRouterResponse.Message.Equals(ExtSrc.AgentComProtocol.DISROUTE_ERROR_EDGE))
                        {
                            //Console.WriteLine("DISROUTE EDGE FOR " + String.Format("127.0.1." + routeHistory.ElementAt(0)[0]) + " ERROR!!!");
                            Console.WriteLine("Disroute error.");
                            continue; //todo tu był return test aby disroutowało sie do konca nawet jak jest ktorys router off
                        }

                    }

                }
                EdgeRemoteRouterIDs.Remove(hashKey);
                return true;
            }
            EdgeRemoteRouterIDs.Remove(hashKey);
            return false;
        }


        private int FindWireIdFromTo(int firstRouterId, int secondRouterId, int forRouter)
        {
            foreach (var dd in DijkstraDataList)
            {
                if (dd.routerID == forRouter && 
                    (dd.RouterIds[0] == firstRouterId && dd.RouterIds[1] == secondRouterId ||
                    dd.RouterIds[0] == secondRouterId && dd.RouterIds[1] == firstRouterId))
                    return dd.wireID;
            }
            return -1;
        }

        private ArrayList CalculateFsUcountFromTo(int routerId, int wireID, int bitrate)
        {
            var mod = Modulation.NULL;
            double FSUcount = 0;
            foreach (DijkstraData dt in DijkstraDataList)
            {
                if (dt.routerID == routerId && dt.wireID == wireID)
                {
                    if (dt.wireDistance > 300)
                    {
                        mod = Modulation.QPSK;

                    }
                    else if (dt.wireDistance > 0 && dt.wireDistance <= 300)
                    {
                        mod = Modulation.SixteenQAM;
                    }
                    var modVal = (int)mod;
                    //todo modulacja wylaczona z obiegu narazie
                    FSUcount = Math.Round((double)(bitrate) / NewWire.FREQ_SLOT_UNIT); /** (double)modVal)*/;
                    //Console.WriteLine("bitrate "+bitrate+" fsucnt "+FSUcount+" modvl "+ modVal);
                }

            }
            var ar = new ArrayList {mod, (int) FSUcount};
            return ar;
        }

//        //metoda uzywana w agencie gdzie recznie podajemy droge
//        //niestety zdublowana czesc kodu bo drugi setroute liczy juz senderRouterID itp ale nie chce nic zepsuc wiec niech tak zostanie narazie
//        public void setRoute(String clientSourceIP, String ClientDestinationIP, int bitrate, int[] excludedWiresIDs, String hashKey, int[] route, int startF = -1)
//        {
//            List<int[]> routeHistory = new List<int[]>();
//            int startfrequency = -1;
//            // zamiana ip koncowego klienta na ip jego routera
//           /* String senderRouterIP;
//            clientMap.TryGetValue(clientSourceIP, out senderRouterIP);
//            if (senderRouterIP == null)
//            {
//                Console.WriteLine("Setting route error.");
//                return;
//            }
//            String recipientRouterIP;
//            clientMap.TryGetValue(ClientDestinationIP, out recipientRouterIP);
//            if (recipientRouterIP == null)
//            {
//                Console.WriteLine("Setting route error.");
//                return;
//            }*/
//            //String Client = Int32.Parse(sourceIP.Substring(sourceIP.Length - 1, 1));
//            int ClientSenderID = Int32.Parse(clientSourceIP.Substring(clientSourceIP.Length - 1, 1));
//            //int RouterRecipientID = Int32.Parse(destinationIP.Substring(destinationIP.Length - 1, 1));
//            int ClientRecipientID = Int32.Parse(ClientDestinationIP.Substring(ClientDestinationIP.Length - 1, 1));
//           /* int[] route = calculateRoute(senderRouterIP, recipientRouterIP, excludedWiresIDs);
//            if (route == null)
//            {
//                Console.WriteLine("AKTUALNIE NIE MOŻNA ZNALEŹĆ DROGI Z " + clientSourceIP + " DO " + ClientDestinationIP);
//                return;
//            }*/
//        //    FSidCounter++;
//            for (int j = 0; j < route.Length; j++)
//            {
//                _bufferRouterResponse = null;
//
//                if (j == 0)
//                {
//                    //wyslij d source routera
//                    //Console.WriteLine("WYSYLAM DO PIERWSZEGO EDGE ROUTERA DANE ROUTINGOWE");
//                    ArrayList ar = calculateFSUcount(FindWireId(route[j], route[j + 1]), bitrate);
//                    String ip = String.Format("127.0.1." + route[j]);
//                    Send(ip, new AgentData()
//                    {
//                        Message = AgentComProtocol.ROUTE_FOR_U_EDGE, 
//                        FsuCount = (int)ar[1], 
//                        Mod = (Modulation)ar[0],
//                        WireId = FindWireId(route[j], route[j + 1]),
//                        ClientSocketId = ClientSenderID,
//                        OriginatingAddress = clientSourceIP,
//                        TargetAddress = ClientDestinationIP,
//                        UniqueKey = hashKey,
//                        StartingFreq = startF,
//                        IsStartEdge = true
//                    });
//                    int rSid = Int32.Parse(clientSourceIP.Substring(clientSourceIP.Length - 1, 1));
//                    EdgeRouterIDs.Add(hashKey, new int[2] { rSid, -1 });
//
//                    //Console.WriteLine("WYSYLALEM DO PIERWSZEGO EDGE ROUTERA DANE ROUTINGOWE (" + ip + ")");
//                }
//                else if (j > 0 && j < route.Length - 1)
//                {
//                    //fsucount, mod, firstwireid,secondwireid, startingfreq dla odbierajacego kabla bo juz obliczone w poprzednim roouterze
//                    //wyslij do zwyklych routerow
//                    //Console.WriteLine("WYSYLAM DO SRODKOWEGO ROUTERA DANE ROUTINGOWE");
//                    ArrayList ar0 = calculateFSUcount(FindWireId(route[j - 1], route[j]), bitrate);
//                    ArrayList ar = calculateFSUcount(FindWireId(route[j], route[j + 1]), bitrate);
//                    if ((int)ar[0] == 0 || (int)ar[1] == 0) return;
//                    String ip = String.Format("127.0.1." + route[j]);
//                    Send(ip, new AgentData()
//                    {
//                        Message = AgentComProtocol.ROUTE_FOR_U,
//                        LastFsuCount = (int)ar0[1],
//                        LastMod = (Modulation)ar0[0],
//                        FsuCount = (int)ar[1],
//                        Mod = (Modulation)ar[0],
//                        FirstWireId = FindWireId(route[j - 1], route[j]),
//                        SecondWireId = FindWireId(route[j], route[j + 1]),
//                        StartingFreq = startfrequency
//                    });
//                    //Console.WriteLine("WYSYLALEM DO SRODKOWEGO ROUTERA DANE ROUTINGOWE (" + ip + ")");
//                }
//                else if (j == route.Length - 1)
//                {
//                    //wyslij do destinationIP routra
//                    //Console.WriteLine("WYSYLAM DO OSTATNIEGO EDGE ROUTERA DANE ROUTINGOWE");
//                    ArrayList ar0 = calculateFSUcount(FindWireId(route[j - 1], route[j]), bitrate);
//                    String ip = String.Format("127.0.1." + route[j]);
//                    Send(ip, new AgentData()
//                    {
//                        Message = AgentComProtocol.ROUTE_FOR_U_EDGE,
//                        FsuCount = (int) ar0[1],
//                        Mod = (Modulation) ar0[0],
//                        WireId = FindWireId(route[j - 1], route[j]),
//                        ClientSocketId = ClientRecipientID,
//                        OriginatingAddress = clientSourceIP,
//                        TargetAddress = ClientDestinationIP,
//                        UniqueKey = hashKey,
//                        StartingFreq = startfrequency,
//                        IsStartEdge = false
//                    });
//                    //Console.WriteLine("WYSYLALEM DO OSTATNIEGO EDGE ROUTERA DANE ROUTINGOWE (" + ip + ")");
//                }
//                //sprawdz czy dostepne bitrejtyy
//                var waitTime = 0;
//                while (_bufferRouterResponse == null)
//                {
//                    //todo a co jak sie zatnie? coś nie dojdzie? router padnie?
//
//                    //w8 na odp od routera
//                    Thread.Sleep(50);
//                    //stestowac to
//                    /*waitTime += 50;
//                    if(waitTime > 10000)
//                        return;*/
//                }
//
//                if (_bufferRouterResponse.Message.Equals(ExtSrc.AgentComProtocol.CONNECTION_IS_ON))
//                {
//                    startfrequency = _bufferRouterResponse.StartingFreq;
//                    //dodawanie do routeHistory
//                    if (j < route.Length - 1)
//                    {
//                        //Console.WriteLine("Add to route {0} wire {1} and slot {2} ", route[j], FindWireId(route[j], route[j + 1]), bufferRouterResponse.FSid);
//                        routeHistory.Add(new int[3] { route[j], FindWireId(route[j], route[j + 1]), _bufferRouterResponse.FSid });
//                    }
//                    if (j == route.Length - 1)
//                    {
//                        routeHistory.Add(new int[3] { route[j], FindWireId(route[j - 1], route[j]), _bufferRouterResponse.FSid });
//                        RouteHistoryList.Add(new String[3] { clientSourceIP, ClientDestinationIP, hashKey }, routeHistory);
//
//                        int rRid = Int32.Parse(ClientDestinationIP.Substring(ClientDestinationIP.Length - 1, 1));
//                        // edgeRouterIDs.Add(hashKey, new int[2] { rSid, rRid });
//
//                        int[] tmp = EdgeRouterIDs[hashKey];
//                        //todo tutaj do edgeRouterIDs dopisuje rRid, a co gdy CONNECTION_UNAVAILABLE ? 
//                        //todo wpis zostanie nie kompletny, gdzie usuwanie? w disroute brak tego
//                        EdgeRouterIDs[hashKey] = new int[] { tmp[0], rRid };
//                        Console.WriteLine("Route set.");
//                        foreach (int[] rh in routeHistory)
//                        {
//                            Console.WriteLine("Router {0} wire {1} and slot {2}. ", rh[0], rh[1] , rh[2]);
//                        }
//                        return;
//                    }
//                }
//                else if (_bufferRouterResponse.Message.Equals(ExtSrc.AgentComProtocol.CONNECTION_UNAVAILABLE))
//                {
//                    //rozlaczyc to co juz zestawione i zaczac liczyc dijkstre bez kabla ktory nie mial miejsca
//                    if (disroute(routeHistory, hashKey))
//                    {
//                        int[] excWiresNext = new int[excludedWiresIDs.Length + 1];
//                        for (int i = 0; i < excludedWiresIDs.Length; i++)
//                        {
//                            excWiresNext[i] = excludedWiresIDs[i];
//                        }
//                        excWiresNext[excWiresNext.Length - 1] = FindWireId(route[j], route[j + 1]);
//                        // gdy rozłączone
//                        //wyjsc z petli for, zapisac ktore kable sa zajete, i wywolac rekurencyjnie
//                        setRoute(clientSourceIP, ClientDestinationIP, bitrate, excWiresNext, hashKey);
//                    }
//                    break;
//                }
//            }
//
//        }

//        private void setRoute(String clientSourceIP, String ClientDestinationIP, int bitrate, int[] excludedWiresIDs, String hashKey)
//        {
//           
//          
//            // zamiana ip koncowego klienta na ip jego routera
//            String senderRouterIP;
//            ClientMap.TryGetValue(clientSourceIP, out senderRouterIP);
//            if (senderRouterIP == null)
//            {
//                Console.WriteLine("Setting route error.");
//                return;
//            }
//            String recipientRouterIP;
//            ClientMap.TryGetValue(ClientDestinationIP, out recipientRouterIP);
//            if (recipientRouterIP == null)
//            {
//                Console.WriteLine("Setting route error.");
//                return;
//            }
//            int[] route = calculateRoute(senderRouterIP, recipientRouterIP, excludedWiresIDs);
//            if (route == null)
//            {
//                Console.WriteLine("Can't find route from " + clientSourceIP+ " to "+ ClientDestinationIP );
//                return;
//            }
//            setRoute(clientSourceIP, ClientDestinationIP, bitrate, excludedWiresIDs, hashKey, route);
//        }
//        private int FindWireId(int firstRouterId, int secondRouterId)
//        {
//            var ar1 = new ArrayList();
//            var ar2 = new ArrayList();
//            foreach (var dd in DijkstraDataList)
//            {
//                if (dd.routerID == firstRouterId) ar1.Add(dd.wireID);
//                if (dd.routerID == secondRouterId) ar2.Add(dd.wireID);
//            }
//            //todo nice linq expresion
//            foreach (int wireidOne in ar1)
//            {
//                foreach (int wireidTwo in ar2)
//                {
//                    if (wireidOne == wireidTwo) return wireidOne;
//                }
//            }
//            return 0;
//        }
// Calculating FrequencySlotUnits needed for transport.
        // returns arraylist[modulation, fsucount]
//        private ArrayList calculateFSUcount(int wireID, int bitrate)
//        {
//            Modulation mod = Modulation.NULL;
//            double FSUcount = 0;
//            foreach (ExtSrc.DijkstraData dt in DijkstraDataList)
//            {
//                if (dt.wireID == wireID)
//                {
//                    if (dt.wireDistance > 300)
//                    {
//                        mod = Modulation.QPSK;
//
//                    }
//                    else if (dt.wireDistance > 0 && dt.wireDistance <= 300)
//                    {
//                        mod = Modulation.SixteenQAM;
//                    }
//                    int modVal = (int) mod;
//                    //todo modulacja wylaczona z obiegu narazie
//                    FSUcount = Math.Round((double) (bitrate)/10.0); /** (double)modVal)*/;
//                    //Console.WriteLine("bitrate "+bitrate+" fsucnt "+FSUcount+" modvl "+ modVal);
//                }
//
//            }
//            ArrayList ar = new ArrayList();
//            ar.Add(mod);
//            ar.Add((int)FSUcount);
//            return ar;
//
//        }
//        private int[] calculateRoute(String sourceIP, String destinationIP, int[] excludedWiresIDs)
//        {
//            int id1 = Int32.Parse(sourceIP.Substring(sourceIP.Length - 1, 1));
//            int id2 = Int32.Parse(destinationIP.Substring(destinationIP.Length - 1, 1));
//            List<ExtSrc.DijkstraData> duplicates = DijkstraDataList.GroupBy(x => x, new ExtSrc.DijkstraEqualityComparer()).SelectMany(grp => grp.Skip(1)).ToList();
//            List<int> wireIdOnline = new List<int>();
//            foreach (ExtSrc.DijkstraData dd in duplicates)
//            {
//                wireIdOnline.Add(dd.wireID);
//            }
//
//            //excluding wires
//            if (excludedWiresIDs != null)
//            {
//                foreach(int d in excludedWiresIDs)
//                {
//                    wireIdOnline.Remove(d);
//                }
//            }
//
//
//
//            List<int[]> routerIds = new List<int[]>();
//            foreach (int i in wireIdOnline)
//            {
//                int[] tmpRoute = new int[3];
//                int ite = 0;
//                foreach (ExtSrc.DijkstraData dd in DijkstraDataList)
//                {
//                    if (dd.wireID == i)
//                    {
//                        tmpRoute[ite] = dd.routerID;
//                        ite++;
//                        if (ite == 1) tmpRoute[ite + 1] = dd.wireDistance;
//                    }
//                }
//                routerIds.Add(tmpRoute);
//            }
//            int[][] dataReadyForDijkstra = new int[routerIds.Count][];
//            for(int i = 0; i < routerIds.Count; i++){
//                dataReadyForDijkstra[i] = new int[]{routerIds.ElementAt(i)[0],routerIds.ElementAt(i)[1],routerIds.ElementAt(i)[2]};
//            }
//            
//            int[,] dta = new int[dataReadyForDijkstra.GetLength(0),3];
//            for (int i = 0; i < dataReadyForDijkstra.GetLength(0); i++)
//            {
//               dta[i, 0] = dataReadyForDijkstra[i][0];
//               dta[i, 1] = dataReadyForDijkstra[i][1];
//               dta[i, 2] = dataReadyForDijkstra[i][2];   
//                 
//            }
//
//               int[] route = Dijkstra.evaluate(dta, id1, id2);
//              
//               return route;
//        }

        // Send message to Node with IpAddress

        public void Send(String ip, ExtSrc.AgentData adata)
        {
            Console.WriteLine("SEND :"+adata.Message);
            //var s = sockets[ip];
            Socket client;
            if (!Sockets.TryGetValue(ip, out client))
            {
                MessageBox.Show("Error in finding socket , method Send().", "ERROR");
                return;
            }
            var fs = new MemoryStream();

            var formatter = new BinaryFormatter();

            formatter.Serialize(fs, adata);

            var buffer = fs.ToArray();

            try
            {
                // Begin sending the data to the remote device.
                client.BeginSend(buffer, 0, buffer.Length, 0, SendCallback, client);
                _sendDone.WaitOne();
            }
            catch (SocketException)
            {
                Console.WriteLine("Router offline.");
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("S: {0} bytes to {1}.", bytesSent, client.RemoteEndPoint);

                // Signal that all bytes have been sent.
                _sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        void UpdateClientList(int id, bool add)
        {
            if (add) MyDomainInfo.Add(id);
            else MyDomainInfo.Remove(id);

            SendDomainInfoToOtherDomains();
        }

        void SendDomainInfoToOtherDomains()
        {
            Console.WriteLine("SendDomainInfoToOtherDomains");
            var data = new AgentData() {Message = AgentComProtocol.DOMAIN_INFO, DomainInfo = MyDomainInfo};
            foreach (var kvp in OtherDomainInfo)
            {
                var ip = kvp.Key.ToString();
                Console.WriteLine("send domain info to 127.0.1."+ip);
                Socket client;
                if (!Sockets.TryGetValue("127.0.1."+ip, out client))
                {
                    MessageBox.Show("Error in finding socket , method Send().", "ERROR");
                    return;
                }
                var fs = new MemoryStream();

                var formatter = new BinaryFormatter();

                formatter.Serialize(fs, data);

                var buffer = fs.ToArray();

                try
                {
                    // Begin sending the data to the remote device.
                    client.BeginSend(buffer, 0, buffer.Length, 0,
                        new AsyncCallback(SendCallback), client);
                    _sendDone.WaitOne();
                }
                catch (SocketException)
                {
                    Console.WriteLine("Router offline.");
                }
            }
        }

        // ############################################################################################################
        // ####################### Keep Nodes Online
        // ############################################################################################################

        void SendingOnlineRequests()
        {
            while (true)
            {
                lock (((ICollection)OnlineRoutersList).SyncRoot)
                {
                    if (OnlineRoutersList.Count != 0)
                    {
                        OnlineRoutersList.ForEach(s => SendOnlineRequest(s.Socket));
                    }
                }
                Thread.Sleep(1000);
            }
        }

        void ListeningForOnlineNodes()
        {
            try
            {
                _onlineAgentSocket.Listen(100);
                while (true)
                {
                    _allDoneOnline.Reset();
                    _onlineAgentSocket.BeginAccept(delegate(IAsyncResult ar)
                    {
                        _allDoneOnline.Set();
                        lock (((ICollection)OnlineRoutersList).SyncRoot)
                        {
                            var ro = new RouterOnline() { Socket = ((Socket)ar.AsyncState).EndAccept(ar) };
                            var adrress = ro.Socket.RemoteEndPoint as IPEndPoint;
                            OnlineRoutersList.Add(ro);
                            new Thread(() =>
                            {
                                // Check if Node is online THREAD, one thread for each Online Node
                                var ip = adrress;
                                while (true)
                                {
                                    Thread.Sleep(1000);
                                    var x = GetTimestamp(DateTime.Now) - ro.TimeStamp;
                                    if (x > 30000000)
                                    {
                                        Console.WriteLine("CLOSING");
                                        //todo closing routers
                                        CloseRouterSocket(ip);
                                        return;
                                    }
                                    //Console.WriteLine("NOT CLOSING");
                                }
                            }).Start();
                        }
                    }, _onlineAgentSocket);
                    _allDoneOnline.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        void SendOnlineRequest(Socket socketRouterOnline)
        {
            try
            {
                var fs = new MemoryStream();
                new BinaryFormatter().Serialize(fs, "IS_ONLINE");
                var buffer = fs.ToArray();
                socketRouterOnline.BeginSend(buffer, 0, buffer.Length, 0, SendOnlineRequestCallback, socketRouterOnline);
                _sendDone.WaitOne(); //todo?
            }
            catch (SocketException e)
            {
//                int line = (new StackTrace(e, true)).GetFrame(0).GetFileLineNumber();
//                Console.WriteLine("Router not responding (ERROR LINE: " + line + ")");
            }
        }

        void SendOnlineRequestCallback(IAsyncResult ar)
        {
            try
            {
                var client = (Socket)ar.AsyncState;
                var bytesSent = client.EndSend(ar);
                _sendDone.Set();//todo?
                var state = new StateObject { workSocket = client };
                var ro = OnlineRoutersList.FirstOrDefault(s => s.Socket == client);
                
                IAsyncResult asyncResult = client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, delegate(IAsyncResult ar1)
                {
                    var handler = ((StateObject)ar1.AsyncState).workSocket;
                    var routerOnline = OnlineRoutersList.FirstOrDefault(s => s.Socket == handler);
                    
                    if (routerOnline == null) return;
                    routerOnline.TimeStamp = GetTimestamp(DateTime.Now);
                    //Console.WriteLine(GetTimestamp(DateTime.Now));
                    routerOnline.IsOnline = true;
                    //Console.WriteLine("ROUTER ONLINE " + routerOnline.Socket.RemoteEndPoint);
                }, state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        void CloseRouterSocket(IPEndPoint ipEndPoint)
        {
            Console.WriteLine("Router {0} closed.", ipEndPoint);
            var router = OnlineRoutersList.FirstOrDefault(s => Equals(s.Socket.RemoteEndPoint, ipEndPoint));
            if (router == null) return;
            router.IsOnline = false;
            var ip = ipEndPoint.Address.ToString();
            var routerId = Int32.Parse(ipEndPoint.Address.ToString().Substring(ipEndPoint.Address.ToString().Length - 1, 1));
            Sockets.Remove(Convert.ToString((ipEndPoint).Address));
            router.Socket.Close();
            lock (((ICollection) OnlineRoutersList).SyncRoot)
            {
                OnlineRoutersList.Remove(router);
            }
            var dijkstraDataRemoveList = DijkstraDataList.Where(dd => dd.routerID == routerId).ToList();
            {
                //Console.WriteLine("DD REMOVED");
                dijkstraDataRemoveList.ForEach(dijkstraData => DijkstraDataList.Remove(dijkstraData));
            }
            //var myValue = ClientMap.Where(x => x.Value == ip).ToList();
            //myValue.ForEach(strings => ClientMap.Remove(strings.Key));
            //todo usuwanie polaczen po usunieciu routera, szukanie alternatywnej drogi??
        }

        // ############################################################################################################
        // ####################### Tools methods
        // ############################################################################################################

        static long GetTimestamp(DateTime value)
        {
            return value.Ticks;
        }

        public void Close()
        {
            _running = false;
            _socket.Close();
            Sockets.Values.ToList().ForEach(s => s.Close());
            _onlineAgentSocket.Close();
            System.Windows.Forms.Application.Exit();
            System.Environment.Exit(1);
        }

        private static string IpToString(EndPoint endPoint)
        {
            var ipEndPoint = endPoint as IPEndPoint;
            return ipEndPoint != null ? ipEndPoint.Address.ToString() : null;
        }
    }

    public class StateObject
    {
        // Client  socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024 * 100;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public ExtSrc.AgentData dt;
    }

    public class MyEqualityStringComparer : IEqualityComparer<String[]>
    {
        public bool Equals(String[] x, String[] y)
        {
            if (x.Length != y.Length)
            {
                return false;
            }
            for (int i = 0; i < x.Length; i++)
            {
                if (!x[i].Equals(y[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public int GetHashCode(String[] obj)
        {
            int result = 17;
            for (int i = 0; i < obj.Length; i++)
            {
                unchecked
                {
                    result = result * 23 + obj[i].GetHashCode();
                }
            }
            return result;
        }
    }

    public class RouterOnline
    {
        public long TimeStamp { get; set; }
        public Socket Socket { get; set; }
        public Boolean IsOnline { get; set; }
        public Timer ClosingTimer { get; set; }
    }
}
