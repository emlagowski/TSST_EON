#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows.Forms;
using ExtSrc;
using Timer = System.Threading.Timer;

#endregion

namespace SubnetworkController
{
    public class SubnetworkController : IDisposable
    {
        const int Timeout = 2000;
        public String AgentIp = "127.6.6.6";
        // Agent communication port
        private const int AgentPort = 6666;
        // Agent router-online-check port
        private const int AgentOnlinePort = 6667;

        readonly Action<DijkstraData> _dijkstraDataAdder;
        readonly Action<DijkstraData> _dijkstraDataRemover;

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

        // Map of{routerAaddress, routerBaddress, hashKey, route type (ownDomain, local, remote, manual), bitrate as string} - > routeHistory (list of {routerID, wireId, FSid} )
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
            _dijkstraDataRemover = dd => DijkstraDataList.Remove(dd);
            RouteHistoryList = new Dictionary<String[], List<int[]>>(new MyEqualityStringComparer());
            EdgeRouterIDs = new Dictionary<String, int[]>();
            EdgeLocalRouterIDs = new Dictionary<String, int[]>();
            EdgeRemoteRouterIDs = new Dictionary<String, int[]>();
            DijkstraDataList = new BindingList<DijkstraData>();
            Sockets = new Dictionary<String, Socket>();
            Dijkstra = new Dijkstra();
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
                    //Log.d("Waiting for a connection...");
                    _socket.BeginAccept(AcceptCallback, _socket);

                    // Wait until a connection is made before continuing.
                    _allDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                //Log.d(e.ToString());
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
            //Log.d("Socket [{0}] {1} - {2} was added to sockets list", sockets.Count, handler.LocalEndPoint.ToString(), handler.RemoteEndPoint.ToString());

            // Create the state object.
            var state = new StateObject { workSocket = handler };
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

                state.dt = (ExtSrc.AgentData)formattor.Deserialize(ms);
                var addr = IpToString(handler.RemoteEndPoint);
                Log.d(String.Format("R: '{0}'[{1} bytes] from {2}.", state.dt.ToString(), bytesRead, addr));

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
                    state.dt.Message.Equals(AgentComProtocol.DOMAIN_CAN_NOT_ROUTE) ||
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
                }
                else
                    BufferAgentData.Add(state.dt);


                var newState = new StateObject { workSocket = handler };
                handler.BeginReceive(newState.buffer, 0, StateObject.BufferSize, 0, ReadCallback, newState);
            }
            catch (SocketException)
            {
                //int line = (new StackTrace(e, true)).GetFrame(0).GetFileLineNumber();
                //Log.d("Router probably closed (ERROR LINE: "+line+")");
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
                    Log.d("Received " + agentData.Message + " " + agentData.RouterID);
                    OtherDomainInfo.Add(agentData.RouterID, new List<int>());
                    SendDomainInfoToOtherDomains();
                    break;
                case AgentComProtocol.DOMAIN_INFO:
                    //OtherDomainInfo.Add(agentData.RouterID, agentData.DomainInfo);
                    OtherDomainInfo[agentData.RouterID] = agentData.DomainInfo;
                    Log.d("Received " + agentData.Message + " " + agentData.RouterID + " " + agentData.DomainInfo.Count);
                    break;
                case AgentComProtocol.DOMAIN_CAN_WE_SET_ROUTE:
                    //Log.d("Other domain asked for route");
                    Log.NCC("Call indication");
                    CheckIfConnectionAvaibleRemote("127.0.1." + agentData.RouterID, agentData.TargetAddress, agentData.FsuCount, null, agentData.DomainRouterID);
                    // todo tutaj sprawdzic czy mozna zestawic polaczenie dla zewnetrznej sieci
//                    var route = CalculateRoute("127.0.1." + agentData.RouterID, agentData.TargetAddress, null);
//                    if (route.Count() != 0)
//                    {
//                        var startingFreqs = CalculateAvaibleStartingFreqs(route, agentData.FsuCount);
//                        Send("127.0.1." + agentData.RouterID, new AgentData()
//                        {
//                            Message = AgentComProtocol.DOMAIN_CAN_ROUTE,
//                            DomainRouterID = DomainToTargetConnector(agentData.DomainRouterID),
//                            StartingFreqsPool = startingFreqs
//                        });
//                    }
//                    else
//                    {
//                        Send("127.0.1." + agentData.RouterID, new AgentData()
//                        {
//                            Message = AgentComProtocol.DOMAIN_CAN_NOT_ROUTE
//                        });
//                    }
                    break;
                case AgentComProtocol.DOMAIN_DISROUTE:
                    //Log.d("Received " + agentData.Message + " for " + agentData.UniqueKey);
                    Log.NCC("Call Teardown in");
                    Log.CC("Connection Teardown in");
                    Disroute(agentData.UniqueKey);
                    Log.CC("Connection Teardown confirmed");
                    break;
                case AgentComProtocol.DOMAIN_SET_ROUTE_FOR_ME:
                    //Log.d("Received " + agentData.Message + " " + agentData.RouterID);
                    Log.CC("Connection request IN");
                    Log.CC("Route Table Query");
                    Log.RC("Ordered list of SNPPs");
                    Log.CC("Link Connection request");
                    Log.LRM("return Link Connection");
                    //OtherDomainInfo.Add(agentData.RouterID, new List<int>());
                    //SendDomainInfoToOtherDomains();
                    var res = SetRemoteRoute(agentData.OriginatingAddress, agentData.TargetAddress, agentData.Bitrate, agentData.Excluded,
                        agentData.UniqueKey, agentData.StartingFreq, agentData.DomainRouterID);
                    if (res != -1)
                    {
                        Log.CC("Connection confirmed.");
                        Send("127.0.1." + agentData.RouterID, new AgentData()
                        {
                            Message = AgentComProtocol.DOMAIN_CAN_SEND
                        });
                    }
                    else
                        Send("127.0.1." + agentData.RouterID, new AgentData()
                        {
                            Message = AgentComProtocol.DOMAIN_CAN_NOT_ROUTE
                        });
                    break;
                case ExtSrc.AgentComProtocol.REGISTER:
                    Log.d(String.Format("Router {0} connected.", agentData.RouterIpAddress));
                    //dijkstra.RoutersNum++;
                    foreach (var dd in agentData.WireIDsList)
                    {
                        SubNetForm.Invoke(this._dijkstraDataAdder, dd);
                    }
                    if (agentData.IsStartEdge) UpdateClientList(agentData.RouterID, true);
                    Log.d("DDL Count:" + DijkstraDataList.Count);
                    //rejestruje sie na liste 
                    break;
                case ExtSrc.AgentComProtocol.UNREGISTER:
                    Log.d(String.Format("Router {0} unconnected.", agentData.RouterIpAddress));
                    //dijkstra.RoutersNum++;
                    Log.d("UREGISTER BEFORE COUNT " + DijkstraDataList.Count);
                    //DijkstraDataList = new BindingList<DijkstraData>(DijkstraDataList.Union(agentData.WireIDsList, new DijkstraEqualityComparer()).ToList());
                    var toRemove = new List<DijkstraData>();
                    foreach (var dd in DijkstraDataList)
                    {
                        //foreach (var dd2 in agentData.WireIDsList)
                        //{
                        if (dd.routerID == agentData.RouterID)
                            toRemove.Add(dd);
                        //}
                    }
                    //toRemove = toRemove.GroupBy(x => x, new DijkstraEqualityComparer()).SelectMany(grp => grp.Skip(1)).ToList();
                    toRemove.ForEach(dd => SubNetForm.Invoke(this._dijkstraDataRemover, dd));
                    Log.d("UREGISTER AFTER COUNT " + DijkstraDataList.Count);
                    if (agentData.IsStartEdge) UpdateClientList(agentData.RouterID, false);
                    Log.d("DDL Count:" + DijkstraDataList.Count);
                    RecalculatePaths(agentData.RouterID);
                    break;
                case ExtSrc.AgentComProtocol.SET_ROUTE_FOR_ME:
                    //Log.d("Router asked for route.");
                    Log.NCC("Call coordination");
                    var result = SetRouteForMe(agentData.OriginatingAddress, agentData.TargetAddress, agentData.Bitrate, agentData.UniqueKey);
                    if (!result)
                        Send(agentData.OriginatingAddress, new AgentData() { Message = AgentComProtocol.ROUTE_UNAVAIBLE, UniqueKey = agentData.UniqueKey });
                    break;
                case ExtSrc.AgentComProtocol.MSG_DELIVERED:
                    //todo info o tym ze jakas wiadomosc dotarla na koniec drogi
                    break;
                case ExtSrc.AgentComProtocol.CONNECTION_IS_ON:
                    //todo zestawianie zakonczone w danym routerze
                    break;
                case ExtSrc.AgentComProtocol.REGISTER_CLIENT:
                    //dodawanie do mapu adresow ip router-klient
                    Log.d(String.Format("Client {0} connected to router {1}.", agentData.ClientIpAddress, agentData.RouterIpAddress));
                    //ClientMap.Add(agentData.ClientIpAddress, agentData.RouterIpAddress);
                    break;
                case ExtSrc.AgentComProtocol.CLIENT_DISCONNECTED:
                    Log.d(String.Format("Client {0} disconnected from router {1}.", agentData.ClientIpAddress, agentData.RouterIpAddress));
                    //ClientMap.Remove(agentData.ClientIpAddress);
                    break;
                case AgentComProtocol.DISROUTE_REQUEST:
                    DisrouteFullDomains(agentData.UniqueKey);
                    break;
                default:
                    //Log.d("Zły msg przybył");
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

        bool CheckIfConnectionAvaibleRemote(String originatingAddress, String targetAddress, int fsuCount, int[] excluded, int domainRouterId)
        {
            // todo not tested method
            var route = CalculateRoute(originatingAddress, targetAddress, excluded);
            if (route.Count() != 0)
            {
                var startingFreqs = CalculateAvaibleStartingFreqs(route, fsuCount);
                var newExcluded = GetExcluded(startingFreqs, fsuCount);
                if (newExcluded != -1)
                {
                    excluded = AddExcluded(excluded, route[newExcluded]);
                    var res = CheckIfConnectionAvaibleRemote(originatingAddress, targetAddress, fsuCount, excluded, domainRouterId);
                    return res;
                }
                Log.NCC("Call confirmed.");
                Send(originatingAddress, new AgentData()
                {
                    Message = AgentComProtocol.DOMAIN_CAN_ROUTE,
                    DomainRouterID = DomainToTargetConnector(domainRouterId),
                    StartingFreqsPool = startingFreqs,
                    Excluded = excluded
                });
            }
            else
            {
                Send(originatingAddress, new AgentData()
                {
                    Message = AgentComProtocol.DOMAIN_CAN_NOT_ROUTE
                });
            }
            return false;
        }

        private int GetExcluded(List<List<int[]>> startingFreqs, int fsuCount)
        {
            foreach (var startingFreq in startingFreqs)
            {
                bool isAvaible = false;
                foreach (var intse in startingFreq)
                {
                    if (intse[1] - intse[0] > fsuCount * NewWire.FREQ_SLOT_UNIT)
                    {
                        isAvaible = true;
                        break;
                    }
                }
                if (!isAvaible) return startingFreqs.IndexOf(startingFreq)+1;
            }
            return -1;
        }

        int[] AddExcluded(int[] excluded, int newExcluded)
        {
            if (excluded == null)
            {
                return new int[]{newExcluded};
            }
            var tmp = excluded;
            excluded = new int[excluded.GetLength(0) + 1];
            for (var i = 0; i < tmp.GetLength(0); i++)
            {
                excluded[i] = tmp[i];
            }
            excluded[excluded.GetLength(0) - 1] = newExcluded;
            return excluded;
        }

        bool SetRouteForMe(String originatingAddress, String targetAddress, int bitrate, String uniqueKey)
        {
            var targetId = Int32.Parse(targetAddress.Substring(targetAddress.Length - 1, 1));
            if (MyDomainInfo.Contains(targetId))
            {
                // Local domain routing
                //policz droge i odeslij do wszystkich ruterow ktore maja byc droga informacje route-for-you
                var hashCode = uniqueKey;
                //setRoute(agentData.ClientIpAddress, agentData.TargetAddress, agentData.Bitrate, null, hashCode);                               
                var res = SetRoute(originatingAddress, targetAddress, bitrate, null, hashCode);
                if (res)
                {
                    Send(originatingAddress,
                        new AgentData() { Message = AgentComProtocol.U_CAN_SEND, UniqueKey = hashCode });
                    return true;
                }
                Log.d("Can not set route.");
                return false;
            }
            else
            {
                Log.d("Target not found in my domain. id=" + targetId);
                // Other domain routing
                if (TargetExists(targetId))
                {
                    Log.d("Target found in other domain.");
                    var hashCode = uniqueKey;
                    var res = SetDomainRoute(originatingAddress, targetAddress, bitrate, null, hashCode);
                    if (res)
                    {
                        Send(originatingAddress,
                            new AgentData() { Message = AgentComProtocol.U_CAN_SEND, UniqueKey = hashCode });
                        return true;
                    }
                    Log.d("Can not set route.");
                    return false;
                }
                Log.d("There not target found at all.");
            }
            return false;
        }

        private bool SetRoute(String originatingAddress, String targetAddress, int bitrate, int[] excludedWiresIDs, String hashKey)
        {
            var fsuCount = (int)Math.Round((double)(bitrate) / Convert.ToDouble(NewWire.FREQ_SLOT_UNIT));

            var route = CalculateRoute(originatingAddress, targetAddress, excludedWiresIDs);
            if (route.Count() == 0) return false;
            var startingFreqs = CalculateAvaibleStartingFreqs(route, fsuCount);
            var newExcluded = GetExcluded(startingFreqs, fsuCount);
            if (newExcluded != -1)
            {
                Log.d("Excluded router = "+ newExcluded);
                excludedWiresIDs = AddExcluded(excludedWiresIDs, route[newExcluded]);
                return SetRoute(originatingAddress, targetAddress, bitrate, excludedWiresIDs, hashKey);
            }
            var startingFreqFinal = FindBestStartingFreq(startingFreqs, fsuCount);
            if (startingFreqFinal == -1)
            {
                Log.d("There is no space for connection with this bandwidth.");
                return false;
            }
            //Log.d("Domain response we can set route. startingFreqFinal=" + startingFreqFinal);
            //SetLocalRoute(originatingAddress, "127.0.1." + localTargetId, bitrate, excludedWiresIDs, hashKey, startingFreqFinal, tmpRouterId);

            var routeHistory = new List<int[]>();
            var startfrequency = startingFreqFinal;

            //var route = CalculateRoute(originatingAddress, targetAddress, excludedWiresIDs);
            if (route == null)
            {
                Log.d("Can't find route from " + originatingAddress + " to " + targetAddress);
                return false;
            }

            for (var j = 0; j < route.Length; j++)
            {
                _bufferRouterResponse = null;

                if (j == 0)
                {
                    //wyslij d source routera
                    //Log.d("WYSYLAM DO PIERWSZEGO EDGE ROUTERA DANE ROUTINGOWE");
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

                    //Log.d("WYSYLALEM DO PIERWSZEGO EDGE ROUTERA DANE ROUTINGOWE (" + ip + ")");
                }
                else if (j > 0 && j < route.Length - 1)
                {
                    //fsucount, mod, firstwireid,secondwireid, startingfreq dla odbierajacego kabla bo juz obliczone w poprzednim roouterze
                    //wyslij do zwyklych routerow
                    //Log.d("WYSYLAM DO SRODKOWEGO ROUTERA DANE ROUTINGOWE");
                    var ar0 = CalculateFsUcountFromTo(route[j], FindWireIdFromTo(route[j - 1], route[j], route[j]), bitrate);
                    var ar = CalculateFsUcountFromTo(route[j], FindWireIdFromTo(route[j], route[j + 1], route[j]), bitrate);
                    if ((int)ar[0] == 0 || (int)ar[1] == 0) return false;
                    var ip = String.Format("127.0.1." + route[j]);
                    Send(ip, new AgentData()
                    {
                        Message = AgentComProtocol.ROUTE_FOR_U,
                        /*LastFsuCount = (int)ar0[1],*/
                        LastMod = (Modulation)ar0[0],
                        FsuCount = (int)ar[1],
                        Mod = (Modulation)ar[0],
                        FirstWireId = FindWireIdFromTo(route[j - 1], route[j], route[j]),
                        SecondWireId = FindWireIdFromTo(route[j], route[j + 1], route[j]),
                        StartingFreq = startfrequency
                    });
                    //Log.d("WYSYLALEM DO SRODKOWEGO ROUTERA DANE ROUTINGOWE (" + ip + ")");
                }
                else if (j == route.Length - 1)
                {
                    //wyslij do destinationIP routra
                    //Log.d("WYSYLAM DO OSTATNIEGO EDGE ROUTERA DANE ROUTINGOWE");
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
                    //Log.d("WYSYLALEM DO OSTATNIEGO EDGE ROUTERA DANE ROUTINGOWE (" + ip + ")");
                }
                //sprawdz czy dostepne bitrejtyy
                if (!WaitForAnswerWithTimeout()) return false;

                if (_bufferRouterResponse.Message.Equals(ExtSrc.AgentComProtocol.CONNECTION_IS_ON))
                {
                    //startfrequency = _bufferRouterResponse.StartingFreq;
                    //dodawanie do routeHistory
                    if (j < route.Length - 1)
                    {
                        //Log.d("Add to route {0} wire {1} and slot {2} ", route[j], FindWireId(route[j], route[j + 1]), bufferRouterResponse.FSid);
                        routeHistory.Add(new int[3] { route[j], FindWireIdFromTo(route[j], route[j + 1], route[j]), _bufferRouterResponse.FSid });
                    }
                    if (j == route.Length - 1)
                    {
                        routeHistory.Add(new int[3] { route[j], FindWireIdFromTo(route[j - 1], route[j], route[j]), _bufferRouterResponse.FSid });
                        RouteHistoryList.Add(new String[5] { originatingAddress, targetAddress, hashKey, "ownDomain", bitrate.ToString() }, routeHistory);

                        var rRid = Int32.Parse(targetAddress.Substring(targetAddress.Length - 1, 1));
                        // edgeRouterIDs.Add(hashKey, new int[2] { rSid, rRid });

                        var tmp = EdgeRouterIDs[hashKey];
                        //todo tutaj do edgeRouterIDs dopisuje rRid, a co gdy CONNECTION_UNAVAILABLE ? 
                        //todo wpis zostanie nie kompletny, gdzie usuwanie? w disroute brak tego
                        EdgeRouterIDs[hashKey] = new int[] { tmp[0], rRid };
                        Log.d("Route set.");
                        foreach (var rh in routeHistory)
                        {
                            Log.d(String.Format("Router {0} wire {1} and slot {2}. ", rh[0], rh[1], rh[2]));
                        }
                        return true;
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
                        return SetRoute(originatingAddress, targetAddress, bitrate, excWiresNext, hashKey);
                    }
                    break;
                }
            }
            return false;
        }

        // Set route for local domain and return startingfreq for other domain
        private int SetLocalRoute(string originatingAddress, string targetAddress, int bitrate, int[] excludedWiresIDs, string hashKey, int startingFreq, int routerId, string remoteTarget)
        {
            var routeHistory = new List<int[]>();
            var startfrequency = startingFreq;

            var route = CalculateRoute(originatingAddress, targetAddress, excludedWiresIDs);
            if (route == null || route.Count() == 0)
            {
                Log.d("Can't find route from " + originatingAddress + " to " + targetAddress);
                return -1;
            }

            for (var j = 0; j < route.Length; j++)
            {
                _bufferRouterResponse = null;

                if (j == 0)
                {
                    Log.CC("Connection Request Out");
                    //wyslij d source routera
                    //Log.d("WYSYLAM DO PIERWSZEGO EDGE ROUTERA DANE ROUTINGOWE");
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
                    //Log.d("WYSYLALEM DO PIERWSZEGO EDGE ROUTERA DANE ROUTINGOWE (" + ip + ")");
                }
                else if (j > 0 && j < route.Length - 1)
                {
                    Log.CC("Connection Request Out");
                    //fsucount, mod, firstwireid,secondwireid, startingfreq dla odbierajacego kabla bo juz obliczone w poprzednim roouterze
                    //wyslij do zwyklych routerow
                    //Log.d("WYSYLAM DO SRODKOWEGO ROUTERA DANE ROUTINGOWE");
                    var ar0 = CalculateFsUcountFromTo(route[j], FindWireIdFromTo(route[j - 1], route[j], route[j]), bitrate);
                    var ar = CalculateFsUcountFromTo(route[j], FindWireIdFromTo(route[j], route[j + 1], route[j]), bitrate);
                    if ((int)ar[0] == 0 || (int)ar[1] == 0) return -1;
                    var ip = String.Format("127.0.1." + route[j]);
                    Send(ip, new AgentData()
                    {
                        Message = AgentComProtocol.ROUTE_FOR_U,
                        /*LastFsuCount = (int)ar0[1],*/
                        LastMod = (Modulation)ar0[0],
                        FsuCount = (int)ar[1],
                        Mod = (Modulation)ar[0],
                        FirstWireId = FindWireIdFromTo(route[j - 1], route[j], route[j]),
                        SecondWireId = FindWireIdFromTo(route[j], route[j + 1], route[j]),
                        StartingFreq = startfrequency
                    });
                    //Log.d("WYSYLALEM DO SRODKOWEGO ROUTERA DANE ROUTINGOWE (" + ip + ")");
                }
                else if (j == route.Length - 1)
                {
                    Log.CC("Connection Request Out");
                    //wyslij do destinationIP routra
                    //Log.d("WYSYLAM DO OSTATNIEGO EDGE ROUTERA DANE ROUTINGOWE");
                    var ar0 = CalculateFsUcountFromTo(route[j], FindWireIdFromTo(route[j - 1], route[j], route[j]), bitrate);
                    var ar = CalculateFsUcountFromTo(route[j], FindWireIdFromTo(route[j], routerId, route[j]), bitrate);
                    if ((int)ar[0] == 0 || (int)ar[1] == 0) return -1;
                    var ip = String.Format("127.0.1." + route[j]);
                    Send(ip, new AgentData()
                    {
                        Message = AgentComProtocol.ROUTE_FOR_U,
                        /*LastFsuCount = (int)ar0[1],*/
                        LastMod = (Modulation)ar0[0],
                        FsuCount = (int)ar[1],
                        Mod = (Modulation)ar[0],
                        FirstWireId = FindWireIdFromTo(route[j - 1], route[j], route[j]),
                        SecondWireId = FindWireIdFromTo(route[j], routerId, route[j]),
                        StartingFreq = startfrequency
                    });
                    //Log.d("WYSYLALEM DO OSTATNIEGO EDGE ROUTERA DANE ROUTINGOWE (" + ip + ")");
                }
                //sprawdz czy dostepne bitrejtyy
                if (!WaitForAnswerWithTimeout()) return -1;

                if (_bufferRouterResponse.Message.Equals(ExtSrc.AgentComProtocol.CONNECTION_IS_ON))
                {
                    Log.CC("Connection Set");
                    //startfrequency = _bufferRouterResponse.StartingFreq;
                    //dodawanie do routeHistory
                    if (j < route.Length - 1)
                    {
                        //Log.d("Add to route {0} wire {1} and slot {2} ", route[j], FindWireId(route[j], route[j + 1]), bufferRouterResponse.FSid);
                        routeHistory.Add(new int[3] { route[j], FindWireIdFromTo(route[j], route[j + 1], route[j]), _bufferRouterResponse.FSid });
                    }
                    if (j == route.Length - 1)
                    {
                        //routeHistory.Add(new int[3] { route[j], FindWireIdFromTo(route[j - 1], route[j], route[j]), _bufferRouterResponse.FSid });
                        routeHistory.Add(new int[3] { route[j], FindWireIdFromTo(route[j], routerId, route[j]), _bufferRouterResponse.FSid });
                        RouteHistoryList.Add(new String[5] { originatingAddress, remoteTarget, hashKey, "local", bitrate.ToString() }, routeHistory);

                        var rRid = Int32.Parse(targetAddress.Substring(targetAddress.Length - 1, 1));
                        // edgeRouterIDs.Add(hashKey, new int[2] { rSid, rRid });

                        var tmp = EdgeLocalRouterIDs[hashKey];
                        //todo tutaj do edgeRouterIDs dopisuje rRid, a co gdy CONNECTION_UNAVAILABLE ? 
                        //todo wpis zostanie nie kompletny, gdzie usuwanie? w disroute brak tego
                        EdgeLocalRouterIDs[hashKey] = new int[] { tmp[0], rRid };
                        Log.d("Route local set.");
                        foreach (var rh in routeHistory)
                        {
                            Log.d(String.Format("Router {0} wire {1} and slot {2}. ", rh[0], rh[1], rh[2]));
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
                        SetLocalRoute(originatingAddress, targetAddress, bitrate, excWiresNext, hashKey, startfrequency, routerId, remoteTarget);
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
            if (route == null || route.Count() == 0)
            {
                Log.d("Can't find route from " + originatingAddress + " to " + targetAddress);
                return -1;
            }

            for (var j = 0; j < route.Length; j++)
            {
                _bufferRouterResponse = null;

                if (j == 0)
                {
                    Log.CC("Connection Request Out");
                    //wyslij d source routera
                    //Log.d("WYSYLAM DO PIERWSZEGO EDGE ROUTERA DANE ROUTINGOWE");
                    var ar0 = CalculateFsUcountFromTo(route[j], FindWireIdFromTo(routerId, route[j], route[j]), bitrate);
                    var ar = CalculateFsUcountFromTo(route[j], FindWireIdFromTo(route[j], route[j + 1], route[j]), bitrate);
                    if ((int)ar[0] == 0 || (int)ar[1] == 0) return -1;
                    var ip = String.Format("127.0.1." + route[j]);
                    Send(ip, new AgentData()
                    {
                        Message = AgentComProtocol.ROUTE_FOR_U,
                        /*LastFsuCount = (int)ar0[1],*/
                        LastMod = (Modulation)ar0[0],
                        FsuCount = (int)ar[1],
                        Mod = (Modulation)ar[0],
                        FirstWireId = FindWireIdFromTo(routerId, route[j], route[j]),
                        SecondWireId = FindWireIdFromTo(route[j], route[j + 1], route[j]),
                        StartingFreq = startfrequency
                    });
                    var rSid = Int32.Parse(originatingAddress.Substring(originatingAddress.Length - 1, 1));
                    EdgeRemoteRouterIDs.Add(hashKey, new int[2] { rSid, -1 });
                    //Log.d("WYSYLALEM DO PIERWSZEGO EDGE ROUTERA DANE ROUTINGOWE (" + ip + ")");
                }
                else if (j > 0 && j < route.Length - 1)
                {
                    Log.CC("Connection Request Out");
                    //fsucount, mod, firstwireid,secondwireid, startingfreq dla odbierajacego kabla bo juz obliczone w poprzednim roouterze
                    //wyslij do zwyklych routerow
                    //Log.d("WYSYLAM DO SRODKOWEGO ROUTERA DANE ROUTINGOWE");
                    var ar0 = CalculateFsUcountFromTo(route[j], FindWireIdFromTo(route[j - 1], route[j], route[j]), bitrate);
                    var ar = CalculateFsUcountFromTo(route[j], FindWireIdFromTo(route[j], route[j + 1], route[j]), bitrate);
                    if ((int)ar[0] == 0 || (int)ar[1] == 0) return -1;
                    var ip = String.Format("127.0.1." + route[j]);
                    Send(ip, new AgentData()
                    {
                        Message = AgentComProtocol.ROUTE_FOR_U,
                        /*LastFsuCount = (int)ar0[1],*/
                        LastMod = (Modulation)ar0[0],
                        FsuCount = (int)ar[1],
                        Mod = (Modulation)ar[0],
                        FirstWireId = FindWireIdFromTo(route[j - 1], route[j], route[j]),
                        SecondWireId = FindWireIdFromTo(route[j], route[j + 1], route[j]),
                        StartingFreq = startfrequency
                    });
                    //Log.d("WYSYLALEM DO SRODKOWEGO ROUTERA DANE ROUTINGOWE (" + ip + ")");
                }
                else if (j == route.Length - 1)
                {
                    Log.CC("Connection Request Out");
                    //wyslij do destinationIP routra
                    //Log.d("WYSYLAM DO OSTATNIEGO EDGE ROUTERA DANE ROUTINGOWE");
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
                    });
                    //Log.d("WYSYLALEM DO OSTATNIEGO EDGE ROUTERA DANE ROUTINGOWE (" + ip + ")");
                }
                //sprawdz czy dostepne bitrejtyy
                if (!WaitForAnswerWithTimeout()) return -1;

                if (_bufferRouterResponse.Message.Equals(ExtSrc.AgentComProtocol.CONNECTION_IS_ON))
                {
                    Log.CC("Connection Set");
                    //startfrequency = _bufferRouterResponse.StartingFreq;
                    //dodawanie do routeHistory
                    if (j < route.Length - 1)
                    {
                        //Log.d("Add to route {0} wire {1} and slot {2} ", route[j], FindWireId(route[j], route[j + 1]), bufferRouterResponse.FSid);
                        routeHistory.Add(new int[3] { route[j], FindWireIdFromTo(route[j], route[j + 1], route[j]), _bufferRouterResponse.FSid });
                    }
                    if (j == route.Length - 1)
                    {
                        routeHistory.Add(new int[3] { route[j], FindWireIdFromTo(route[j - 1], route[j], route[j]), _bufferRouterResponse.FSid });
                        RouteHistoryList.Add(new String[5] { originatingAddr, targetAddress, hashKey, "remote", bitrate.ToString() }, routeHistory);

                        var rRid = Int32.Parse(targetAddress.Substring(targetAddress.Length - 1, 1));
                        // edgeRouterIDs.Add(hashKey, new int[2] { rSid, rRid });

                        var tmp = EdgeRemoteRouterIDs[hashKey];
                        //todo tutaj do edgeRouterIDs dopisuje rRid, a co gdy CONNECTION_UNAVAILABLE ? 
                        //todo wpis zostanie nie kompletny, gdzie usuwanie? w disroute brak tego
                        EdgeRemoteRouterIDs[hashKey] = new int[] { tmp[0], rRid };
                        Log.d("Route local set.");
                        foreach (var rh in routeHistory)
                        {
                            Log.d(String.Format("Router {0} wire {1} and slot {2}. ", rh[0], rh[1], rh[2]));
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

        private bool SetDomainRoute(String originatingAddress, String targetAddress, int bitrate, int[] excludedWiresIDs, String hashKey)
        {
            var targetId = Int32.Parse(targetAddress.Substring(targetAddress.Length - 1, 1));
            // set local path
            var localTargetId = DomainToTargetConnector(targetId);
            Log.d("we are going to send :" + "127.0.1." + localTargetId + " ask about permission to route.");
            var fsuCount = (int)Math.Round((double)(bitrate) / Convert.ToDouble(NewWire.FREQ_SLOT_UNIT));
            _bufferRouterResponse = null;
            Send("127.0.1." + localTargetId, new AgentData()
            {
                Message = AgentComProtocol.DOMAIN_CAN_WE_SET_ROUTE,
                DomainRouterID = Int32.Parse(originatingAddress.Substring(originatingAddress.Length - 1, 1)),
                TargetAddress = targetAddress,
                FsuCount = fsuCount,
                UniqueKey = hashKey,
            });
            if (!WaitForAnswerWithTimeout()) return false;
            if (_bufferRouterResponse.Message == AgentComProtocol.DOMAIN_CAN_NOT_ROUTE)
            {
                Log.d("Other Domain cannot set route.");
                return false;
            }
            Log.NCC("Connection request.");
            Log.CC("Connection request in.");
            Log.CC("Route Table Query");
            var tmpList = _bufferRouterResponse.StartingFreqsPool;
            var tmpRouterId = _bufferRouterResponse.DomainRouterID;
            var externalExcluded = _bufferRouterResponse.Excluded;
            var route = CalculateRoute(originatingAddress, "127.0.1." + localTargetId, excludedWiresIDs);
            if (route.Count() == 0)
            {
                Log.d("This domain cannot set route.");
                return false;
            }
            var startingFreqs = CalculateAvaibleStartingFreqs(route, fsuCount);

            //
            var newExcluded = GetExcluded(startingFreqs, fsuCount);
            if (newExcluded != -1)
            {
                excludedWiresIDs = AddExcluded(excludedWiresIDs, route[newExcluded]);
                route = CalculateRoute(originatingAddress, "127.0.1." + localTargetId, excludedWiresIDs);
            }
            //

            startingFreqs.AddRange(tmpList);
            var startingFreqFinal = FindBestStartingFreq(startingFreqs, fsuCount);
//            Log.d("Domain response we can set route. startingFreqFinal=" + startingFreqFinal);
            Log.RC("Ordered list of SNPPS");
            if (startingFreqFinal == -1)
            {
                Log.d("There is no space for connection with this bandwidth.");
                return false;
            }
            Log.CC("Link Connection request.");
            Log.LRM("return Link Connection.");
            var res = SetLocalRoute(originatingAddress, "127.0.1." + localTargetId, bitrate, excludedWiresIDs, hashKey, startingFreqFinal, tmpRouterId, targetAddress);
            if (res == -1)
            {
                Log.d("This domain cannot set route.");
                return false;
            }
            // set remote path
            Log.d("Local route set.");
            Log.CC("Connection request out.");
            _bufferRouterResponse = null;
            Send("127.0.1." + localTargetId, new AgentData()
            {
                Message = AgentComProtocol.DOMAIN_SET_ROUTE_FOR_ME,
                DomainRouterID = localTargetId,
                OriginatingAddress = originatingAddress,
                TargetAddress = targetAddress,
                Bitrate = bitrate,
                UniqueKey = hashKey,
                StartingFreq = startingFreqFinal,
                Excluded = externalExcluded
            });
            if (!WaitForAnswerWithTimeout()) return false;
            if (_bufferRouterResponse.Message == AgentComProtocol.DOMAIN_CAN_NOT_ROUTE)
            {
                Log.d("Other Domain cannot set route.");
                return false;
            }
            else
            {
                //Log.d("Remote domain route set.");
                Log.CC("Connection confirmed.");
            }
            return true;
        }

        private static int FindBestStartingFreq(List<List<int[]>> startingFreqs, int fsuCount)
        {
            int result = 0, counter = 0;
            var spectralWidth = Enumerable.Repeat(NewWire.EMPTY_VALUE, 1000).ToArray();
            foreach (var listOfRanges in startingFreqs)
            {
                var tmpSpectralWidth = Enumerable.Repeat(1, 1000).ToArray();
                foreach (var range in listOfRanges)
                {
                    for (var i = 0; i < 1000; i++)
                    {
                        if (i >= range[0] && i <= range[1])
                        {
                            tmpSpectralWidth[i] = NewWire.EMPTY_VALUE;
                        }
                    }
                }
                for (var i = 0; i < 1000; i++)
                {
                    if (tmpSpectralWidth[i] == 1)
                    {
                        spectralWidth[i] = 1;
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

                if (counter == fsuCount * NewWire.FREQ_SLOT_UNIT)
                    return result;
            }
            return -1;
        }

        private List<List<int[]>> CalculateAvaibleStartingFreqs(int[] route, int fsucount)
        {
            Log.d("CalculateAvaibleStartingFreqs route size" + route.GetLength(0) + " fsucount " + fsucount);
            var result = new List<List<int[]>>();
            for (var j = 0; j < route.GetLength(0); j++)
            {
                _bufferRouterResponse = null;
                if (j == route.GetLength(0) - 1)
                {
                    Send("127.0.1." + route[j], new AgentData()
                    {
                        Message = AgentComProtocol.AVAIBLE_STARTING_FREQS,
                        WireId = FindWireIdFromTo(route[j], route[j - 1], route[j]),
                        FsuCount = fsucount,
                        IsEndEdge = true
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

                if (!WaitForAnswerWithTimeout()) return null;
                Log.d("Got response from node with ranges, size " + _bufferRouterResponse.StartingFreqs.Count);
                result.Add(_bufferRouterResponse.StartingFreqs);
            }
            return result;
        }

        private int[] CalculateRoute(String sourceIp, String destinationIp, int[] excludedWiresIDs)
        {
            var id1 = Int32.Parse(sourceIp.Substring(sourceIp.Length - 1, 1));
            var id2 = Int32.Parse(destinationIp.Substring(destinationIp.Length - 1, 1));
            var distinctData = DijkstraDataList.GroupBy(x => x, new DijkstraEqualityComparer()).SelectMany(grp => grp.Skip(1)).ToList();

            var wireIdOnline = distinctData.Select(dd => new[] { dd.RouterIds[0], dd.RouterIds[1], dd.wireDistance }).ToList();

            // todo excluded wires musze miec cos wiecej niz samo ID lokalne
            //excluding wires
            
            if (excludedWiresIDs != null)
            {
                var toRemove = new List<int>();
                for (var i = 0; i < wireIdOnline.Count; i++)
                {
                    if (excludedWiresIDs.Contains(wireIdOnline[i][0]))
                    {
                        toRemove.Add(i);
                        continue;
                    }
                    if (excludedWiresIDs.Contains(wireIdOnline[i][1]))
                    {
                        toRemove.Add(i);
                        continue;
                    }
                }
                var tmp = toRemove.OrderByDescending(i => i);
                foreach (var i in tmp)
                {
                    wireIdOnline.RemoveAt(i);
                }
            }

            var dataReadyForDijkstra = new int[wireIdOnline.Count][];
            for (var i = 0; i < wireIdOnline.Count; i++)
            {
                dataReadyForDijkstra[i] = new int[] { wireIdOnline.ElementAt(i)[0], wireIdOnline.ElementAt(i)[1], wireIdOnline.ElementAt(i)[2] };
            }

            var dta = new int[dataReadyForDijkstra.GetLength(0), 3];
            for (var i = 0; i < dataReadyForDijkstra.GetLength(0); i++)
            {
                dta[i, 0] = dataReadyForDijkstra[i][0];
                dta[i, 1] = dataReadyForDijkstra[i][1];
                dta[i, 2] = dataReadyForDijkstra[i][2];

            }
            var route = new int[] { };
            try
            {
                route = Dijkstra.Evaluate(dta, id1, id2);
            }
            catch (IndexOutOfRangeException)
            {
                // there is no way
                Log.d("There is no path avaible.");
            }
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
                 Log.d("Setting route error.");
                 return;
             }
             String recipientRouterIP;
             clientMap.TryGetValue(ClientDestinationIP, out recipientRouterIP);
             if (recipientRouterIP == null)
             {
                 Log.d("Setting route error.");
                 return;
             }*/
            //String Client = Int32.Parse(sourceIP.Substring(sourceIP.Length - 1, 1));
            int ClientSenderID = Int32.Parse(clientSourceIP.Substring(clientSourceIP.Length - 1, 1));
            //int RouterRecipientID = Int32.Parse(destinationIP.Substring(destinationIP.Length - 1, 1));
            int ClientRecipientID = Int32.Parse(ClientDestinationIP.Substring(ClientDestinationIP.Length - 1, 1));
            /* int[] route = calculateRoute(senderRouterIP, recipientRouterIP, excludedWiresIDs);
             if (route == null)
             {
                 Log.d("AKTUALNIE NIE MOŻNA ZNALEŹĆ DROGI Z " + clientSourceIP + " DO " + ClientDestinationIP);
                 return;
             }*/
            // FSidCounter++;
            for (int j = 0; j < route.Length; j++)
            {
                _bufferRouterResponse = null;

                if (j == 0)
                {
                    //wyslij d source routera
                    //Log.d("WYSYLAM DO PIERWSZEGO EDGE ROUTERA DANE ROUTINGOWE");
                    ArrayList ar = CalculateFsUcountFromTo(route[j], FindWireIdFromTo(route[j], route[j + 1], route[j]), bitrate);
                    String ip = String.Format("127.0.1." + route[j]);
                    Send(ip, new ExtSrc.AgentData()
                    {
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

                    //Log.d("WYSYLALEM DO PIERWSZEGO EDGE ROUTERA DANE ROUTINGOWE (" + ip + ")");
                }
                else if (j > 0 && j < route.Length - 1)
                {
                    //fsucount, mod, firstwireid,secondwireid, startingfreq dla odbierajacego kabla bo juz obliczone w poprzednim roouterze
                    //wyslij do zwyklych routerow
                    //Log.d("WYSYLAM DO SRODKOWEGO ROUTERA DANE ROUTINGOWE");
                    ArrayList ar0 = CalculateFsUcountFromTo(route[j], FindWireIdFromTo(route[j - 1], route[j], route[j]), bitrate);
                    ArrayList ar = CalculateFsUcountFromTo(route[j], FindWireIdFromTo(route[j], route[j + 1], route[j]), bitrate);
                    if ((int)ar[0] == 0 || (int)ar[1] == 0) return;
                    String ip = String.Format("127.0.1." + route[j]);
                    Send(ip, new AgentData()
                    {
                        Message = AgentComProtocol.ROUTE_FOR_U,
                        /*LastFsuCount = (int) ar0[1],*/
                        LastMod = (Modulation)ar0[0],
                        FsuCount = (int)ar[1],
                        Mod = (Modulation)ar[0],
                        FirstWireId = FindWireIdFromTo(route[j - 1], route[j], route[j]),
                        SecondWireId = FindWireIdFromTo(route[j], route[j + 1], route[j]),
                        StartingFreq = startfrequency
                    });
                    //Log.d("WYSYLALEM DO SRODKOWEGO ROUTERA DANE ROUTINGOWE (" + ip + ")");
                }
                else if (j == route.Length - 1)
                {
                    //wyslij do destinationIP routra
                    //Log.d("WYSYLAM DO OSTATNIEGO EDGE ROUTERA DANE ROUTINGOWE");
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
                    //Log.d("WYSYLALEM DO OSTATNIEGO EDGE ROUTERA DANE ROUTINGOWE (" + ip + ")");
                }
                //sprawdz czy dostepne bitrejtyy
                if (!WaitForAnswerWithTimeout()) return;

                if (_bufferRouterResponse.Message.Equals(ExtSrc.AgentComProtocol.CONNECTION_IS_ON))
                {
                    startfrequency = _bufferRouterResponse.StartingFreq;
                    //dodawanie do routeHistory
                    if (j < route.Length - 1)
                    {
                        //Log.d("Add to route {0} wire {1} and slot {2} ", route[j], FindWireId(route[j], route[j + 1]), bufferRouterResponse.FSid);
                        routeHistory.Add(new int[3] { route[j], FindWireIdFromTo(route[j], route[j + 1], route[j]), _bufferRouterResponse.FSid });
                    }
                    if (j == route.Length - 1)
                    {
                        routeHistory.Add(new int[3] { route[j], FindWireIdFromTo(route[j - 1], route[j], route[j]), _bufferRouterResponse.FSid });
                        RouteHistoryList.Add(new String[5] { clientSourceIP, ClientDestinationIP, hashKey, "manual", bitrate.ToString() }, routeHistory);

                        int rRid = Int32.Parse(ClientDestinationIP.Substring(ClientDestinationIP.Length - 1, 1));
                        // edgeRouterIDs.Add(hashKey, new int[2] { rSid, rRid });

                        int[] tmp = EdgeRouterIDs[hashKey];
                        //todo tutaj do edgeRouterIDs dopisuje rRid, a co gdy CONNECTION_UNAVAILABLE ? 
                        //todo wpis zostanie nie kompletny, gdzie usuwanie? w disroute brak tego
                        EdgeRouterIDs[hashKey] = new int[] { tmp[0], rRid };
                        Log.d("Route set.");
                        foreach (int[] rh in routeHistory)
                        {
                            Log.d(String.Format("Router {0} wire {1} and slot {2}. ", rh[0], rh[1], rh[2]));
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
                    //Log.d("EdgeRouterIDs");
                    Disroute(routeHist, uniKey);
                }
                else if (EdgeLocalRouterIDs.ContainsKey(uniKey))
                {
                    //Log.d("EdgeLocalRouterIDs");
                    DisrouteLocal(routeHist, uniKey);
                }
                else if (EdgeRemoteRouterIDs.ContainsKey(uniKey))
                {
                    //Log.d("EdgeRemoteRouterIDs");
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
                    Log.CC("Connection teardown out.");
                    //Log.d("ONE DISROUTE MSG IS GOING TO BE SENT : " + routeHistory.ElementAt(i)[0] + " -> " + routeHistory.ElementAt(i)[1] + " -> " + routeHistory.ElementAt(i)[2]);
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
                        if (!WaitForAnswerWithTimeout()) break;
                        if (_bufferRouterResponse != null && _bufferRouterResponse.Message.Equals(ExtSrc.AgentComProtocol.DISROUTE_IS_DONE))
                            //Log.d("DISROUTE FOR " + String.Format("127.0.1." + routeHistory.ElementAt(i)[0]) + " IS DONE");
                            Log.d("Disoute done.");
                        else if (_bufferRouterResponse == null || _bufferRouterResponse.Message.Equals(ExtSrc.AgentComProtocol.DISROUTE_ERROR))
                        {
                            //Log.d("DISROUTE FOR " + String.Format("127.0.1." + routeHistory.ElementAt(i)[0]) + " ERROR!!!");
                            Log.d("Disroute error.");
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
                        if (!WaitForAnswerWithTimeout()) break;

                        if (_bufferRouterResponse != null && _bufferRouterResponse.Message.Equals(ExtSrc.AgentComProtocol.DISROUTE_EDGE_IS_DONE))
                            //Log.d("DISROUTE EDGE FOR " + String.Format("127.0.1." + routeHistory.ElementAt(0)[0]) + " IS DONE");
                            Log.d("Disoute done.");
                        else if (_bufferRouterResponse == null || _bufferRouterResponse.Message.Equals(ExtSrc.AgentComProtocol.DISROUTE_ERROR_EDGE))
                        {
                            //Log.d("DISROUTE EDGE FOR " + String.Format("127.0.1." + routeHistory.ElementAt(0)[0]) + " ERROR!!!");
                            Log.d("Disroute error.");
                            continue; //todo tu był return test aby disroutowało sie do konca nawet jak jest ktorys router off
                        }

                    }
                    Log.CC("Link Connection Teardown.");
                    Log.LRM("Link Connection deallocation.");
                    Log.LRM("Link Connection teardown confirmed.");
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
                    Log.CC("Connection teardown out.");
                    //Log.d("ONE DISROUTE MSG IS GOING TO BE SENT : " + routeHistory.ElementAt(i)[0] + " -> " + routeHistory.ElementAt(i)[1] + " -> " + routeHistory.ElementAt(i)[2]);
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
                        if (!WaitForAnswerWithTimeout()) break;
                        if (_bufferRouterResponse != null && _bufferRouterResponse.Message.Equals(ExtSrc.AgentComProtocol.DISROUTE_IS_DONE))
                            //Log.d("DISROUTE FOR " + String.Format("127.0.1." + routeHistory.ElementAt(i)[0]) + " IS DONE");
                            Log.d("Disoute done.");
                        else if (_bufferRouterResponse == null || _bufferRouterResponse.Message.Equals(ExtSrc.AgentComProtocol.DISROUTE_ERROR))
                        {
                            //Log.d("DISROUTE FOR " + String.Format("127.0.1." + routeHistory.ElementAt(i)[0]) + " ERROR!!!");
                            Log.d("Disroute error.");
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
                        if (!WaitForAnswerWithTimeout()) break;

                        if (_bufferRouterResponse != null && _bufferRouterResponse.Message.Equals(ExtSrc.AgentComProtocol.DISROUTE_EDGE_IS_DONE))
                            //Log.d("DISROUTE EDGE FOR " + String.Format("127.0.1." + routeHistory.ElementAt(0)[0]) + " IS DONE");
                            Log.d("Disoute done.");
                        else if (_bufferRouterResponse == null || _bufferRouterResponse.Message.Equals(ExtSrc.AgentComProtocol.DISROUTE_ERROR_EDGE))
                        {
                            //Log.d("DISROUTE EDGE FOR " + String.Format("127.0.1." + routeHistory.ElementAt(0)[0]) + " ERROR!!!");
                            Log.d("Disroute error.");
                            continue; //todo tu był return test aby disroutowało sie do konca nawet jak jest ktorys router off
                        }

                    }
                    Log.CC("Link Connection Teardown.");
                    Log.LRM("Link Connection deallocation.");
                    Log.LRM("Link Connection teardown confirmed.");
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
                    Log.CC("Connection teardown out.");
                    //Log.d("ONE DISROUTE MSG IS GOING TO BE SENT : " + routeHistory.ElementAt(i)[0] + " -> " + routeHistory.ElementAt(i)[1] + " -> " + routeHistory.ElementAt(i)[2]);
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
                        if (!WaitForAnswerWithTimeout()) break;
                        if (_bufferRouterResponse != null && _bufferRouterResponse.Message.Equals(ExtSrc.AgentComProtocol.DISROUTE_IS_DONE))
                            //Log.d("DISROUTE FOR " + String.Format("127.0.1." + routeHistory.ElementAt(i)[0]) + " IS DONE");
                            Log.d("Disoute done.");
                        else if (_bufferRouterResponse == null || _bufferRouterResponse.Message.Equals(ExtSrc.AgentComProtocol.DISROUTE_ERROR))
                        {
                            //Log.d("DISROUTE FOR " + String.Format("127.0.1." + routeHistory.ElementAt(i)[0]) + " ERROR!!!");
                            Log.d("Disroute error.");
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
                            UniqueKey = hashKey,
                            IsEndEdge = true
                        });
                        if (!WaitForAnswerWithTimeout()) break;

                        if (_bufferRouterResponse != null && _bufferRouterResponse.Message.Equals(ExtSrc.AgentComProtocol.DISROUTE_EDGE_IS_DONE))
                            //Log.d("DISROUTE EDGE FOR " + String.Format("127.0.1." + routeHistory.ElementAt(0)[0]) + " IS DONE");
                            Log.d("Call teardown confirmed");
                        else if (_bufferRouterResponse == null || _bufferRouterResponse.Message.Equals(ExtSrc.AgentComProtocol.DISROUTE_ERROR_EDGE))
                        {
                            //Log.d("DISROUTE EDGE FOR " + String.Format("127.0.1." + routeHistory.ElementAt(0)[0]) + " ERROR!!!");
                            Log.d("Disroute error.");
                            continue; //todo tu był return test aby disroutowało sie do konca nawet jak jest ktorys router off
                        }

                    }
                    Log.CC("Link Connection Teardown.");
                    Log.LRM("Link Connection deallocation.");
                    Log.LRM("Link Connection teardown confirmed.");
                }
                EdgeRemoteRouterIDs.Remove(hashKey);
                return true;
            }
            EdgeRemoteRouterIDs.Remove(hashKey);
            return false;
        }

        bool DisrouteFullDomains(String uniKey)
        {
            var routeHist = RouteHistoryList.FirstOrDefault(d => d.Key[2].Equals(uniKey));
            if (routeHist.Value != null)
            {
                Log.NCC("Call Teardown in");
                if (EdgeRouterIDs.ContainsKey(uniKey))
                {
                    //Log.d("EdgeRouterIDs");
                    Disroute(routeHist.Value, uniKey);
                }
                else if (EdgeLocalRouterIDs.ContainsKey(uniKey))
                {
                    Log.NCC("Call Teardown out");
                    Log.NCC("Call Teardown confirmed");
                    Log.NCC("Connection Teardown out");
                    Log.NCC("Connection Teardown in");
                    //Log.d("EdgeLocalRouterIDs");
                    DisrouteLocal(routeHist.Value, uniKey);
                    var ip = routeHist.Key[1];
                    var id = Int32.Parse(ip.Substring(ip.Length - 1, 1));
                    var newIp = "127.0.1." + DomainToTargetConnector(id);
                    Log.CC("Connection teardown out.");
                    Send(newIp, new AgentData() { Message = AgentComProtocol.DOMAIN_DISROUTE, UniqueKey = uniKey });
                }
                else if (EdgeRemoteRouterIDs.ContainsKey(uniKey))
                {
                    Log.NCC("Call Teardown out");
                    //Log.d("EdgeRemoteRouterIDs");
                    DisrouteRemote(routeHist.Value, uniKey);
                    var ip = routeHist.Key[0];
                    var id = Int32.Parse(ip.Substring(ip.Length - 1, 1));
                    var newIp = "127.0.1." + DomainToTargetConnector(id);
                    Send(newIp, new AgentData() { Message = AgentComProtocol.DOMAIN_DISROUTE, UniqueKey = uniKey });
                }
                var key = RouteHistoryList.FirstOrDefault(d => d.Value.Equals(routeHist.Value)).Key;
                RouteHistoryList.Remove(key);
                Log.NCC("Call termination confirmed");
                return true;
            }
            return false;
        }

        private void RecalculatePaths(int removedId)
        {
            var mapToDo = new List<String[]>();
            foreach (var keyValuePair in RouteHistoryList)
            {
                foreach (var eachPartOfRoute in keyValuePair.Value)
                {
                    if (eachPartOfRoute[0] == removedId)
                    {
                        var key = keyValuePair.Key[2];
                        var addressA = keyValuePair.Key[0];
                        var addressB = keyValuePair.Key[1];
                        var type = keyValuePair.Key[3];
                        var bitrate = keyValuePair.Key[4];
                        mapToDo.Add(new[] { addressA, addressB, key, type, bitrate });
                    }
                }
            }
            mapToDo.ForEach(e =>
            {
                if (e[3] == "manual") return;
                DisrouteFullDomains(e[2]);
                var res = SetRouteForMe(e[0], e[1], Convert.ToInt32(e[4]), e[2]);
                if (res)
                {
                    Send(e[0], new AgentData()
                    {
                        Message = AgentComProtocol.MODIFY_UNQCON_AFTER_REPAIR,
                        OriginatingAddress = e[0],
                        TargetAddress = e[1],
                        RouterIpAddress =
                            "127.0.1." + DomainToTargetConnector(Int32.Parse(e[1].Substring(e[1].Length - 1, 1)))
                    });
                }
                else
                {
                    Send(e[0], new AgentData()
                    {
                        Message = AgentComProtocol.ROUTE_UNAVAIBLE,
                        UniqueKey = e[2]
                    });
                }
            });
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
                    //Log.d("bitrate "+bitrate+" fsucnt "+FSUcount+" modvl "+ modVal);
                }

            }
            var ar = new ArrayList { mod, (int)FSUcount };
            return ar;
        }

        public void Send(String ip, ExtSrc.AgentData adata)
        {
            Log.d("SEND :" + adata.Message);
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
                Log.d("Router offline.");
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
                Log.d(String.Format("S: {0} bytes to {1}.", bytesSent, client.RemoteEndPoint));

                // Signal that all bytes have been sent.
                _sendDone.Set();
            }
            catch (Exception e)
            {
                //Log.d(e.ToString());
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
            Log.d("SendDomainInfoToOtherDomains");
            var data = new AgentData() { Message = AgentComProtocol.DOMAIN_INFO, DomainInfo = MyDomainInfo };
            foreach (var kvp in OtherDomainInfo)
            {
                var ip = kvp.Key.ToString();
                Log.d("send domain info to 127.0.1." + ip);
                Socket client;
                if (!Sockets.TryGetValue("127.0.1." + ip, out client))
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
                    Log.d("Router offline.");
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
                                        Log.d("CLOSING");
                                        //todo closing routers
                                        CloseRouterSocket(ip);
                                        return;
                                    }
                                    //Log.d("NOT CLOSING");
                                }
                            }).Start();
                        }
                    }, _onlineAgentSocket);
                    _allDoneOnline.WaitOne();
                }
            }
            catch (Exception e)
            {
                //Log.d(e.ToString());
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
            catch (SocketException)
            {
                //                int line = (new StackTrace(e, true)).GetFrame(0).GetFileLineNumber();
                //Log.d("Router not responding (ERROR LINE: " + line + ")");
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
                    //Log.d(GetTimestamp(DateTime.Now));
                    routerOnline.IsOnline = true;
                    //Log.d("ROUTER ONLINE " + routerOnline.Socket.RemoteEndPoint);
                }, state);
            }
            catch (Exception e)
            {
                //Log.d(e.ToString());
            }
        }

        void CloseRouterSocket(IPEndPoint ipEndPoint)
        {
            Log.d(String.Format("Router {0} closed.", ipEndPoint));
            var router = OnlineRoutersList.FirstOrDefault(s => Equals(s.Socket.RemoteEndPoint, ipEndPoint));
            if (router == null) return;
            router.IsOnline = false;
            var ip = ipEndPoint.Address.ToString();
            var routerId = Int32.Parse(ipEndPoint.Address.ToString().Substring(ipEndPoint.Address.ToString().Length - 1, 1));
            Sockets.Remove(Convert.ToString((ipEndPoint).Address));
            router.Socket.Close();
            lock (((ICollection)OnlineRoutersList).SyncRoot)
            {
                OnlineRoutersList.Remove(router);
            }
            var dijkstraDataRemoveList = DijkstraDataList.Where(dd => dd.routerID == routerId).ToList();
            {
                //Log.d("DD REMOVED");
                dijkstraDataRemoveList.ForEach(dijkstraData => DijkstraDataList.Remove(dijkstraData));
            }
            //var myValue = ClientMap.Where(x => x.Value == ip).ToList();
            //myValue.ForEach(strings => ClientMap.Remove(strings.Key));
            //todo usuwanie polaczen po usunieciu routera, szukanie alternatywnej drogi??
        }

        bool WaitForAnswerWithTimeout()
        {
            var waitTime = 0;
            while (_bufferRouterResponse == null)
            {
                //w8 na odp od routera
                Thread.Sleep(50);
                waitTime += 50;
                if (waitTime > Timeout) return false;
            }
            return true;
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

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose managed resources
                _socket.Close();
                _onlineAgentSocket.Close();
                _allDone.Close();
                _sendDone.Close();
                _allDoneOnline.Close();
                _sendDoneOnline.Close();
                foreach (var socket in Sockets.Values)
                {
                    socket.Close();
                }
            }
            // free native resources
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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
