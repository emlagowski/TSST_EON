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
        const String AgentIp = "127.6.6.6";
        // Agent communication port
        private const int AgentPort = 6666;
        // Agent router-online-check port
        private const int AgentOnlinePort = 6667;

        readonly Action<DijkstraData> _dijkstraDataAdder;

        // Gui Object
        readonly Form _form;

        // Agent socket to communicate with nodes
        readonly Socket _socket;

        // Agent socket to keep nodes online
        readonly Socket _onlineAgentSocket;

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
        Dictionary<String, int[]> EdgeRouterIDs { get; set; }

        // List of received data from Nodes
        List<AgentData> BufferAgentData { get; set; }

        // Response which are needed for now
        AgentData _bufferRouterResponse;

        // Route Table With Dijkstra data
        public BindingList<DijkstraData> DijkstraDataList { get; set; }

        // Map of "routerIpAddres" -> "clientIpAddress"
        public readonly Dictionary<String, String> ClientMap;

        // List of Online Routers, send requests, show in gui
        public List<RouterOnline> OnlineRoutersList { get; set; }

        // Map of{routerAaddress, routerBaddress, hashKey} - > routeHistory (list of {routerID, wireId, FSid} )
        public Dictionary<String[], List<int[]>> RouteHistoryList { get; set; }


        // ############################################################################################################
        // ####################### Constructor's and methods
        // ############################################################################################################

        public SubnetworkController(Form form)
        {
            this._form = form;

            // Initialazing
            OnlineRoutersList = new List<RouterOnline>();
            _dijkstraDataAdder = dd => DijkstraDataList.Add(dd);
            RouteHistoryList = new Dictionary<String[], List<int[]>>(new MyEqualityStringComparer());
            EdgeRouterIDs = new Dictionary<String, int[]>();
            DijkstraDataList = new BindingList<DijkstraData>();
            Sockets = new Dictionary<String, Socket>();
            ClientMap = new Dictionary<String, String>();
            Dijkstra = new Dijkstra(this);
            BufferAgentData = new List<ExtSrc.AgentData>();

            // Run and connect
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Bind(new IPEndPoint(IPAddress.Parse(AgentIp), AgentPort));
            _socket.ReceiveBufferSize = 1024 * 100;
            _onlineAgentSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _onlineAgentSocket.Bind(new IPEndPoint(IPAddress.Parse(AgentIp), AgentOnlinePort));

            new Thread(Run).Start();
            new Thread(ProcessAgentDataRun).Start();
            new Thread(ListeningForOnlineNodes).Start();
            new Thread(SendingOnlineRequests).Start();
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

                Console.WriteLine("R: '{0}'[{1} bytes] from {2}.", state.dt.ToString(), bytesRead, IpToString(handler.RemoteEndPoint));

                //
                // Odbieramy dane od routera dodajemy do bufora,
                // aby odebrac dane od wszystkich i nic nie stracić
                // 
                if (state.dt.Message.Equals(AgentComProtocol.CONNECTION_IS_ON) ||
                    state.dt.Message.Equals(AgentComProtocol.CONNECTION_UNAVAILABLE) ||
                    state.dt.Message.Equals(AgentComProtocol.DISROUTE_IS_DONE) ||
                    state.dt.Message.Equals(AgentComProtocol.DISROUTE_ERROR) ||
                    state.dt.Message.Equals(AgentComProtocol.DISROUTE_ERROR_EDGE) ||
                    state.dt.Message.Equals(AgentComProtocol.DISROUTE_EDGE_IS_DONE)
                    )
                    _bufferRouterResponse = state.dt;
                else
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
                case ExtSrc.AgentComProtocol.REGISTER:
                    Console.WriteLine("Router {0} connected.", agentData.RouterIpAddress);
                    //dijkstra.RoutersNum++;
                    foreach (var dd in agentData.WireIDsList)
                    {
                        _form.Invoke(this._dijkstraDataAdder, dd);
                    }
                    Console.WriteLine("DDL Count:"+DijkstraDataList.Count);
                    //rejestruje sie na liste 
                    break;
                case ExtSrc.AgentComProtocol.SET_ROUTE_FOR_ME:
                    Console.WriteLine("Router asked for route.");
                    //policz droge i odeslij do wszystkich ruterow ktore maja byc droga informacje route-for-you
                    var hashCode = agentData.UniqueKey;
                    //setRoute(agentData.ClientIpAddress, agentData.TargetAddress, agentData.Bitrate, null, hashCode);                               
                    setRouteNew(agentData.OriginatingAddress, agentData.TargetAddress, agentData.Bitrate, null, hashCode);
                    Send(agentData.OriginatingAddress, new AgentData() { Message = AgentComProtocol.U_CAN_SEND, UniqueKey = hashCode });
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
                    ClientMap.Add(agentData.ClientIpAddress, agentData.RouterIpAddress);
                    break;
                case ExtSrc.AgentComProtocol.CLIENT_DISCONNECTED:
                    Console.WriteLine("Client {0} disconnected from router {1}.", agentData.ClientIpAddress, agentData.RouterIpAddress);
                    ClientMap.Remove(agentData.ClientIpAddress);
                    break;
                default:
                    //Console.WriteLine("Zły msg przybył");
                    break;
            }
        }

        // ############################################################################################################
        // ####################### Calculate, set and send route messages
        // ############################################################################################################

        private void setRouteNew(String originatingAddress, String targetAddress, int bitrate, int[] excludedWiresIDs, String hashKey)
        {
            var routeHistory = new List<int[]>();
            var startfrequency = -1;

            var route = calculateRouteNew(originatingAddress, targetAddress, excludedWiresIDs);
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
                    var ar = calculateFSUcountFromTo(route[j], FindWireIdFromTo(route[j], route[j + 1], route[j]), bitrate);
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
                    var ar0 = calculateFSUcountFromTo(route[j], FindWireIdFromTo(route[j - 1], route[j], route[j]), bitrate);
                    var ar = calculateFSUcountFromTo(route[j], FindWireIdFromTo(route[j], route[j + 1], route[j]), bitrate);
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
                    var ar0 = calculateFSUcountFromTo(route[j], FindWireIdFromTo(route[j - 1], route[j], route[j]), bitrate);
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
                    if (disroute(routeHistory, hashKey))
                    {
                        var excWiresNext = new int[excludedWiresIDs.Length + 1];
                        for (var i = 0; i < excludedWiresIDs.Length; i++)
                        {
                            excWiresNext[i] = excludedWiresIDs[i];
                        }
                        excWiresNext[excWiresNext.Length - 1] = FindWireIdFromTo(route[j], route[j + 1], route[j]);
                        // gdy rozłączone
                        //wyjsc z petli for, zapisac ktore kable sa zajete, i wywolac rekurencyjnie
                        setRouteNew(originatingAddress, targetAddress, bitrate, excWiresNext, hashKey);
                    }
                    break;
                }
            }

        }

        private int[] calculateRouteNew(String sourceIP, String destinationIP, int[] excludedWiresIDs)
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

        public void setRouteFromTo(String clientSourceIP, String ClientDestinationIP, int bitrate, int[] excludedWiresIDs, String hashKey, int[] route, int startF = -1)
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
                    ArrayList ar = calculateFSUcountFromTo(route[j], FindWireIdFromTo(route[j], route[j + 1], route[j]), bitrate);
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
                    ArrayList ar0 = calculateFSUcountFromTo(route[j], FindWireIdFromTo(route[j - 1], route[j], route[j]), bitrate);
                    ArrayList ar = calculateFSUcountFromTo(route[j], FindWireIdFromTo(route[j], route[j + 1], route[j]), bitrate);
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
                    ArrayList ar0 = calculateFSUcountFromTo(route[j], FindWireIdFromTo(route[j - 1], route[j], route[j]), bitrate);
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
                    if (disroute(routeHistory, hashKey))
                    {
                        int[] excWiresNext = new int[excludedWiresIDs.Length + 1];
                        for (int i = 0; i < excludedWiresIDs.Length; i++)
                        {
                            excWiresNext[i] = excludedWiresIDs[i];
                        }
                        excWiresNext[excWiresNext.Length - 1] = FindWireIdFromTo(route[j], route[j + 1], route[j]);
                        // gdy rozłączone
                        //wyjsc z petli for, zapisac ktore kable sa zajete, i wywolac rekurencyjnie
                        setRoute(clientSourceIP, ClientDestinationIP, bitrate, excWiresNext, hashKey);
                    }
                    break;
                }
            }

        }

        //metoda uzywana w agencie gdzie recznie podajemy droge
        //niestety zdublowana czesc kodu bo drugi setroute liczy juz senderRouterID itp ale nie chce nic zepsuc wiec niech tak zostanie narazie
        public void setRoute(String clientSourceIP, String ClientDestinationIP, int bitrate, int[] excludedWiresIDs, String hashKey, int[] route, int startF = -1)
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
        //    FSidCounter++;
            for (int j = 0; j < route.Length; j++)
            {
                _bufferRouterResponse = null;

                if (j == 0)
                {
                    //wyslij d source routera
                    //Console.WriteLine("WYSYLAM DO PIERWSZEGO EDGE ROUTERA DANE ROUTINGOWE");
                    ArrayList ar = calculateFSUcount(FindWireId(route[j], route[j + 1]), bitrate);
                    String ip = String.Format("127.0.1." + route[j]);
                    Send(ip, new AgentData()
                    {
                        Message = AgentComProtocol.ROUTE_FOR_U_EDGE, 
                        FsuCount = (int)ar[1], 
                        Mod = (Modulation)ar[0],
                        WireId = FindWireId(route[j], route[j + 1]),
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
                    ArrayList ar0 = calculateFSUcount(FindWireId(route[j - 1], route[j]), bitrate);
                    ArrayList ar = calculateFSUcount(FindWireId(route[j], route[j + 1]), bitrate);
                    if ((int)ar[0] == 0 || (int)ar[1] == 0) return;
                    String ip = String.Format("127.0.1." + route[j]);
                    Send(ip, new AgentData()
                    {
                        Message = AgentComProtocol.ROUTE_FOR_U,
                        LastFsuCount = (int)ar0[1],
                        LastMod = (Modulation)ar0[0],
                        FsuCount = (int)ar[1],
                        Mod = (Modulation)ar[0],
                        FirstWireId = FindWireId(route[j - 1], route[j]),
                        SecondWireId = FindWireId(route[j], route[j + 1]),
                        StartingFreq = startfrequency
                    });
                    //Console.WriteLine("WYSYLALEM DO SRODKOWEGO ROUTERA DANE ROUTINGOWE (" + ip + ")");
                }
                else if (j == route.Length - 1)
                {
                    //wyslij do destinationIP routra
                    //Console.WriteLine("WYSYLAM DO OSTATNIEGO EDGE ROUTERA DANE ROUTINGOWE");
                    ArrayList ar0 = calculateFSUcount(FindWireId(route[j - 1], route[j]), bitrate);
                    String ip = String.Format("127.0.1." + route[j]);
                    Send(ip, new AgentData()
                    {
                        Message = AgentComProtocol.ROUTE_FOR_U_EDGE,
                        FsuCount = (int) ar0[1],
                        Mod = (Modulation) ar0[0],
                        WireId = FindWireId(route[j - 1], route[j]),
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
                        routeHistory.Add(new int[3] { route[j], FindWireId(route[j], route[j + 1]), _bufferRouterResponse.FSid });
                    }
                    if (j == route.Length - 1)
                    {
                        routeHistory.Add(new int[3] { route[j], FindWireId(route[j - 1], route[j]), _bufferRouterResponse.FSid });
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
                            Console.WriteLine("Router {0} wire {1} and slot {2}. ", rh[0], rh[1] , rh[2]);
                        }
                        return;
                    }
                }
                else if (_bufferRouterResponse.Message.Equals(ExtSrc.AgentComProtocol.CONNECTION_UNAVAILABLE))
                {
                    //rozlaczyc to co juz zestawione i zaczac liczyc dijkstre bez kabla ktory nie mial miejsca
                    if (disroute(routeHistory, hashKey))
                    {
                        int[] excWiresNext = new int[excludedWiresIDs.Length + 1];
                        for (int i = 0; i < excludedWiresIDs.Length; i++)
                        {
                            excWiresNext[i] = excludedWiresIDs[i];
                        }
                        excWiresNext[excWiresNext.Length - 1] = FindWireId(route[j], route[j + 1]);
                        // gdy rozłączone
                        //wyjsc z petli for, zapisac ktore kable sa zajete, i wywolac rekurencyjnie
                        setRoute(clientSourceIP, ClientDestinationIP, bitrate, excWiresNext, hashKey);
                    }
                    break;
                }
            }

        }

        private void setRoute(String clientSourceIP, String ClientDestinationIP, int bitrate, int[] excludedWiresIDs, String hashKey)
        {
           
          
            // zamiana ip koncowego klienta na ip jego routera
            String senderRouterIP;
            ClientMap.TryGetValue(clientSourceIP, out senderRouterIP);
            if (senderRouterIP == null)
            {
                Console.WriteLine("Setting route error.");
                return;
            }
            String recipientRouterIP;
            ClientMap.TryGetValue(ClientDestinationIP, out recipientRouterIP);
            if (recipientRouterIP == null)
            {
                Console.WriteLine("Setting route error.");
                return;
            }
            int[] route = calculateRoute(senderRouterIP, recipientRouterIP, excludedWiresIDs);
            if (route == null)
            {
                Console.WriteLine("Can't find route from " + clientSourceIP+ " to "+ ClientDestinationIP );
                return;
            }
            setRoute(clientSourceIP, ClientDestinationIP, bitrate, excludedWiresIDs, hashKey, route);
        }

        public Boolean disroute(List<int[]> routeHistory, String hashKey)
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
                return true;
            }
            return false;
        }

        private int FindWireId(int firstRouterId, int secondRouterId)
        {
            var ar1 = new ArrayList();
            var ar2 = new ArrayList();
            foreach (var dd in DijkstraDataList)
            {
                if (dd.routerID == firstRouterId) ar1.Add(dd.wireID);
                if (dd.routerID == secondRouterId) ar2.Add(dd.wireID);
            }
            //todo nice linq expresion
            foreach (int wireidOne in ar1)
            {
                foreach (int wireidTwo in ar2)
                {
                    if (wireidOne == wireidTwo) return wireidOne;
                }
            }
            return 0;
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

        // Calculating FrequencySlotUnits needed for transport.
        // returns arraylist[modulation, fsucount]
        private ArrayList calculateFSUcount(int wireID, int bitrate)
        {
            Modulation mod = Modulation.NULL;
            double FSUcount = 0;
            foreach (ExtSrc.DijkstraData dt in DijkstraDataList)
            {
                if (dt.wireID == wireID)
                {
                    if (dt.wireDistance > 300)
                    {
                        mod = Modulation.QPSK;

                    }
                    else if (dt.wireDistance > 0 && dt.wireDistance <= 300)
                    {
                        mod = Modulation.SixteenQAM;
                    }
                    int modVal = (int) mod;
                    //todo modulacja wylaczona z obiegu narazie
                    FSUcount = Math.Round((double) (bitrate)/10.0); /** (double)modVal)*/;
                    //Console.WriteLine("bitrate "+bitrate+" fsucnt "+FSUcount+" modvl "+ modVal);
                }

            }
            ArrayList ar = new ArrayList();
            ar.Add(mod);
            ar.Add((int)FSUcount);
            return ar;

        }

        private ArrayList calculateFSUcountFromTo(int routerId, int wireID, int bitrate)
        {
            Modulation mod = Modulation.NULL;
            double FSUcount = 0;
            foreach (ExtSrc.DijkstraData dt in DijkstraDataList)
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
                    int modVal = (int)mod;
                    //todo modulacja wylaczona z obiegu narazie
                    FSUcount = Math.Round((double)(bitrate) / 10.0); /** (double)modVal)*/;
                    //Console.WriteLine("bitrate "+bitrate+" fsucnt "+FSUcount+" modvl "+ modVal);
                }

            }
            ArrayList ar = new ArrayList();
            ar.Add(mod);
            ar.Add((int)FSUcount);
            return ar;

        }

        private int[] calculateRoute(String sourceIP, String destinationIP, int[] excludedWiresIDs)
        {
            int id1 = Int32.Parse(sourceIP.Substring(sourceIP.Length - 1, 1));
            int id2 = Int32.Parse(destinationIP.Substring(destinationIP.Length - 1, 1));
            List<ExtSrc.DijkstraData> duplicates = DijkstraDataList.GroupBy(x => x, new ExtSrc.DijkstraEqualityComparer()).SelectMany(grp => grp.Skip(1)).ToList();
            List<int> wireIdOnline = new List<int>();
            foreach (ExtSrc.DijkstraData dd in duplicates)
            {
                wireIdOnline.Add(dd.wireID);
            }

            //excluding wires
            if (excludedWiresIDs != null)
            {
                foreach(int d in excludedWiresIDs)
                {
                    wireIdOnline.Remove(d);
                }
            }



            List<int[]> routerIds = new List<int[]>();
            foreach (int i in wireIdOnline)
            {
                int[] tmpRoute = new int[3];
                int ite = 0;
                foreach (ExtSrc.DijkstraData dd in DijkstraDataList)
                {
                    if (dd.wireID == i)
                    {
                        tmpRoute[ite] = dd.routerID;
                        ite++;
                        if (ite == 1) tmpRoute[ite + 1] = dd.wireDistance;
                    }
                }
                routerIds.Add(tmpRoute);
            }
            int[][] dataReadyForDijkstra = new int[routerIds.Count][];
            for(int i = 0; i < routerIds.Count; i++){
                dataReadyForDijkstra[i] = new int[]{routerIds.ElementAt(i)[0],routerIds.ElementAt(i)[1],routerIds.ElementAt(i)[2]};
            }
            
            int[,] dta = new int[dataReadyForDijkstra.GetLength(0),3];
            for (int i = 0; i < dataReadyForDijkstra.GetLength(0); i++)
            {
               dta[i, 0] = dataReadyForDijkstra[i][0];
               dta[i, 1] = dataReadyForDijkstra[i][1];
               dta[i, 2] = dataReadyForDijkstra[i][2];   
                 
            }

               int[] route = Dijkstra.evaluate(dta, id1, id2);
              
               return route;
        }

        // Send message to Node with IpAddress
        public void Send(String ip, ExtSrc.AgentData adata)
        {
            //var s = sockets[ip];
            Socket client;
            if (!Sockets.TryGetValue(ip, out client))
            {
                MessageBox.Show("Error in finding socket , method Send().", "ERROR");
                return;
            }
            MemoryStream fs = new MemoryStream();

            BinaryFormatter formatter = new BinaryFormatter();

            formatter.Serialize(fs, adata);

            byte[] buffer = fs.ToArray();


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
            var myValue = ClientMap.Where(x => x.Value == ip).ToList();
            myValue.ForEach(strings => ClientMap.Remove(strings.Key));
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
