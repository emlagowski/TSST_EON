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

namespace Agent
{
    public class Communication
    {
        private int FSidCounter = -1;
        public readonly int TIMEOUT = 2000;
        Action<DijkstraData> dijkstraDataAdder;
        private Form form;
        Socket socket;
        public ManualResetEvent allDone = new ManualResetEvent(false);
        public ManualResetEvent sendDone = new ManualResetEvent(false);
        public ManualResetEvent allDoneOnline = new ManualResetEvent(false);
        public ManualResetEvent sendDoneOnline = new ManualResetEvent(false);
        private bool running = true;
        public Dijkstra dijkstra { get; set; }

        // String addressIP of endPoint, socket
        Dictionary<String, Socket> sockets { get; set; }

        List<ExtSrc.AgentData> _bufferAgentData;
        Socket OnlineAgentSocket;
        public List<RouterOnline> OnlineRoutersList { get; set; }

        ExtSrc.AgentData bufferRouterResponse;
        // routerAaddress, routerBaddress, hashKey - > routeHistory (list of routerID, wireId, FSid )
        public Dictionary<String[], List<int[]>> routeHistoryList { get; set; }
        // hashKey, {ID of edge routers for path}
        public Dictionary<String, int[]> edgeRouterIDs { get; set; }
        
        // ROUTING DIJKSTRA DATA
        public BindingList<ExtSrc.DijkstraData> dijkstraDataList { get; set; }

        // routerIpaddres - clientIpaddress
        public Dictionary<String, String> clientMap;

        public List<ExtSrc.AgentData> bufferAgentData
        {
            get
            {
                //Console.WriteLine("GET called.");
                return _bufferAgentData;
            }
            set
            {
                //Console.WriteLine("SET called.");
                _bufferAgentData = value;
            }
        }

        public Communication(Form form)
        {
            this.form = form;
            OnlineRoutersList = new List<RouterOnline>();
            dijkstraDataAdder = dd => dijkstraDataList.Add(dd);
            routeHistoryList = new Dictionary<String[], List<int[]>>(new MyEqualityStringComparer());
            edgeRouterIDs = new Dictionary<String, int[]>();
            dijkstraDataList = new BindingList<DijkstraData>();
            

            sockets = new Dictionary<String, Socket>();
            clientMap = new Dictionary<String, String>();
            dijkstra = new Dijkstra(this);
            bufferAgentData = new List<ExtSrc.AgentData>();
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Parse("127.6.6.6"), 6666));
            socket.ReceiveBufferSize = 1024 * 100;
            OnlineAgentSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            OnlineAgentSocket.Bind(new IPEndPoint(IPAddress.Parse("127.6.6.6"), 6667));

            new Thread(Run).Start();
            new Thread(ProcessAgentDataRun).Start();
            new Thread(OnlineRun).Start();
            new Thread(delegate()
            {
                while (true)
                {
                    lock (((ICollection) OnlineRoutersList).SyncRoot)
                    {
                        if (OnlineRoutersList.Count != 0)
                        {
                            OnlineRoutersList.ForEach(s => SendOnlineRequest(s.Socket));
                        }
                    }
                    Thread.Sleep(1000);
                }
            }).Start();



            //Dijkstra dijkstra = new Dijkstra();
            //int[,] tab = new int[,] {           {1,2,1},
            //                                    {1,3,2},
            //                                    {1,5,3},
            //                                    {2,5,7},
            //                                    {3,4,5},
            //                                    {3,5,4},
            //                                    {4,5,6}};

            //int[] res = dijkstra.evaluate(tab, 2, 4);

            //foreach (int x in res)
            //    Console.WriteLine(x);


        }

        /**
         * Thread body, waiting for new connections
         */

        public string IpToString(EndPoint endPoint)
        {
            return (endPoint as IPEndPoint).Address.ToString();
        }

        void Run()
        {
            try
            {
                socket.Listen(100);
                while (true)
                {
                    // Set the event to nonsignaled state.
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.
                    //Console.WriteLine("Waiting for a connection...");
                    socket.BeginAccept(new AsyncCallback(AcceptCallback), socket);

                    // Wait until a connection is made before continuing.
                    allDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        void OnlineRun()
        {
            try
            {
                OnlineAgentSocket.Listen(100);
                while (true)
                {
                    allDoneOnline.Reset();
                    OnlineAgentSocket.BeginAccept(delegate(IAsyncResult ar)
                    {
                        allDoneOnline.Set();
                        lock (((ICollection) OnlineRoutersList).SyncRoot)
                        {
                            var ro = new RouterOnline() {Socket = ((Socket) ar.AsyncState).EndAccept(ar)};
                            var adrress = ro.Socket.RemoteEndPoint as IPEndPoint;
                            OnlineRoutersList.Add(ro);
                            new Thread(() =>
                            {
                                var ip = adrress;
                                while (true)
                                {
                                    Thread.Sleep(1000);
                                    var x = GetTimestamp(DateTime.Now) - ro.TimeStamp;
                                    if (x > 30000000)
                                    {
                                        //Console.WriteLine("CLOSING");
                                        //todo closing routers
                                        CloseRouterSocket(ip);
                                        return;
                                    }
                                    //Console.WriteLine("NOT CLOSING");
                                }
                            }).Start();
                        }
                    }, OnlineAgentSocket);
                    allDoneOnline.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            allDone.Set();

            // Get the socket that handles the client request.
            var listener = (Socket)ar.AsyncState;
            var handler = listener.EndAccept(ar);
            sockets.Add(Convert.ToString((handler.RemoteEndPoint as IPEndPoint).Address), handler);
            //addConnection(handler.RemoteEndPoint.ToString());
            //Console.WriteLine("Socket [{0}] {1} - {2} was added to sockets list", sockets.Count, handler.LocalEndPoint.ToString(), handler.RemoteEndPoint.ToString());

            // Create the state object.
            var state = new StateObject {workSocket = handler};
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, ReadCallback, state);
        }

        public void ReadCallback(IAsyncResult ar)
        {
            try
            {
                String content = String.Empty;

                // Retrieve the state object and the handler socket
                // from the asynchronous state object.
                StateObject state = (StateObject) ar.AsyncState;
                Socket handler = state.workSocket;

                // Read data from the client socket. 
                int bytesRead = handler.EndReceive(ar);
                BinaryFormatter formattor = new BinaryFormatter();

                MemoryStream ms = new MemoryStream(state.buffer);

                state.dt = (ExtSrc.AgentData) formattor.Deserialize(ms);

                Console.WriteLine("R: '{0}'[{1} bytes] from {2}.",
                    state.dt.ToString(), bytesRead, IpToString(handler.RemoteEndPoint));

                //
                // Odbieramy dane od routera dodajemy do bufora,
                // aby odebrac dane od wszystkich i nic nie stracić
                // 
                if (state.dt.message.Equals(ExtSrc.AgentComProtocol.CONNECTION_IS_ON) ||
                    state.dt.message.Equals(ExtSrc.AgentComProtocol.CONNECTION_UNAVAILABLE) ||
                    state.dt.message.Equals(ExtSrc.AgentComProtocol.DISROUTE_IS_DONE) ||
                    state.dt.message.Equals(ExtSrc.AgentComProtocol.DISROUTE_ERROR) ||
                    state.dt.message.Equals(ExtSrc.AgentComProtocol.DISROUTE_ERROR_EDGE) ||
                    state.dt.message.Equals(ExtSrc.AgentComProtocol.DISROUTE_EDGE_IS_DONE)
                    )
                    bufferRouterResponse = state.dt;
                else
                    bufferAgentData.Add(state.dt);


                var newState = new StateObject {workSocket = handler};
                handler.BeginReceive(newState.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), newState);
            }
            catch (SocketException e)
            {
                //int line = (new StackTrace(e, true)).GetFrame(0).GetFileLineNumber();
                //Console.WriteLine("Router probably closed (ERROR LINE: "+line+")");
            }
        }

        public void ProcessAgentDataRun()
        {
            while (running)
            {
                if (bufferAgentData.Count != 0)
                {
                    ProcessAgentData(bufferAgentData.First());
                    bufferAgentData.RemoveAt(0);
                }

            }
        }

       
        private void ProcessAgentData(ExtSrc.AgentData agentData)
        {
            int id1, id2;
            switch (agentData.message)
            {
                case ExtSrc.AgentComProtocol.REGISTER:
                    Console.WriteLine("Router {0} connected.", agentData.originatingAddress);
                    //dijkstra.RoutersNum++;
                    foreach (DijkstraData dd in agentData.wireIDsList)
                    {
                        //Console.WriteLine("ADDED "+dd.routerID+" "+dd.wireID+" "+dd.wireDistance);
                        
                        //dijkstraDataList.Add(dd);
                        form.Invoke(this.dijkstraDataAdder, dd);
                    }
                    //rejestruje sie na liste 
                    break;
                case ExtSrc.AgentComProtocol.SET_ROUTE_FOR_ME:
                    Console.WriteLine("Router asked for route.");
                    //policz droge i odeslij do wszystkich ruterow ktore maja byc droga informacje route-for-you
                    String hashCode = agentData.uniqueKey;
                    setRoute(agentData.clientIPAddress, agentData.targetAddress, agentData.bitrate, null, hashCode);                               
                    Send(agentData.routerIPAddress,new AgentData(AgentComProtocol.U_CAN_SEND, hashCode));
                    break;
                case ExtSrc.AgentComProtocol.MSG_DELIVERED:
                    //todo info o tym ze jakas wiadomosc dotarla na koniec drogi
                    break;
                case ExtSrc.AgentComProtocol.CONNECTION_IS_ON:
                    //todo zestawianie zakonczone w danym routerze
                    break;
                case ExtSrc.AgentComProtocol.REGISTER_CLIENT:
                    //dodawanie do mapu adresow ip router-klient
                    Console.WriteLine("Client {0} connected to router {1}.", agentData.clientIPAddress, agentData.routerIPAddress);
                    clientMap.Add(agentData.clientIPAddress, agentData.routerIPAddress);
                    break;
                case ExtSrc.AgentComProtocol.CLIENT_DISCONNECTED:
                    Console.WriteLine("Client {0} disconnected from router {1}.", agentData.clientIPAddress, agentData.routerIPAddress);
                    clientMap.Remove(agentData.clientIPAddress);
                    break;
                default:
                    //Console.WriteLine("Zły msg przybył");
                    break;
            }
        }
        //metoda uzywana w agencie gdzie recznie podajemy droge
        //niestety zdublowana czesc kodu bo drugi setroute liczy juz senderRouterID itp ale nie chce nic zepsuc wiec niech tak zostanie narazie
        public void setRoute(String clientSourceIP, String ClientDestinationIP, int bitrate, int[] excludedWiresIDs, String hashKey, int[] route, int startF = -1)
        {
            List<int[]> routeHistory = new List<int[]>();
            int startfrequency = -1;
            // zamiana ip koncowego klienta na ip jego routera
            String senderRouterIP;
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
            }
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
            FSidCounter++;
            for (int j = 0; j < route.Length; j++)
            {
                bufferRouterResponse = null;

                if (j == 0)
                {
                    //wyslij d source routera
                    //Console.WriteLine("WYSYLAM DO PIERWSZEGO EDGE ROUTERA DANE ROUTINGOWE");
                    ArrayList ar = calculateFSUcount(FindWireId(route[j], route[j + 1]), bitrate);
                    String ip = String.Format("127.0.1." + route[j]);
                    Send(ip, new ExtSrc.AgentData(ExtSrc.AgentComProtocol.ROUTE_FOR_U_EDGE, (int)ar[1], (Modulation)ar[0],
                                                        FindWireId(route[j], route[j + 1]), ClientSenderID)
                                                        {
                                                            originatingAddress = clientSourceIP,
                                                            targetAddress = ClientDestinationIP,
                                                            uniqueKey = hashKey,
                                                            startingFreq = startF,
                                                            FSid = FSidCounter
                                                        });
                    int rSid = Int32.Parse(senderRouterIP.Substring(senderRouterIP.Length - 1, 1));
                    edgeRouterIDs.Add(hashKey, new int[2] { rSid, -1 });

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
                    Send(ip, new ExtSrc.AgentData(ExtSrc.AgentComProtocol.ROUTE_FOR_U, (int)ar0[1], (Modulation)ar0[0], (int)ar[1], (Modulation)ar[0],
                                                    FindWireId(route[j - 1], route[j]), FindWireId(route[j], route[j + 1]), startfrequency){FSid = FSidCounter});
                    //Console.WriteLine("WYSYLALEM DO SRODKOWEGO ROUTERA DANE ROUTINGOWE (" + ip + ")");
                }
                else if (j == route.Length - 1)
                {
                    //wyslij do destinationIP routra
                    //Console.WriteLine("WYSYLAM DO OSTATNIEGO EDGE ROUTERA DANE ROUTINGOWE");
                    ArrayList ar0 = calculateFSUcount(FindWireId(route[j - 1], route[j]), bitrate);
                    String ip = String.Format("127.0.1." + route[j]);
                    Send(ip, new ExtSrc.AgentData(ExtSrc.AgentComProtocol.ROUTE_FOR_U_EDGE, (int)ar0[1], (Modulation)ar0[0],
                                                        FindWireId(route[j - 1], route[j]), ClientRecipientID)
                    {
                        originatingAddress = clientSourceIP,
                        targetAddress = ClientDestinationIP,
                        uniqueKey = hashKey,
                        startingFreq = startfrequency,
                        FSid = FSidCounter

                    });
                    //Console.WriteLine("WYSYLALEM DO OSTATNIEGO EDGE ROUTERA DANE ROUTINGOWE (" + ip + ")");
                }
                //sprawdz czy dostepne bitrejty
                var waitTime = 0;
                while (bufferRouterResponse == null)
                {
                    //todo a co jak sie zatnie? coś nie dojdzie? router padnie?

                    //w8 na odp od routera
                    Thread.Sleep(50);
                    //stestowac to
                    /*waitTime += 50;
                    if(waitTime > 10000)
                        return;*/
                }

                if (bufferRouterResponse.message.Equals(ExtSrc.AgentComProtocol.CONNECTION_IS_ON))
                {
                    startfrequency = bufferRouterResponse.startingFreq;
                    //dodawanie do routeHistory
                    if (j < route.Length - 1)
                    {
                        //Console.WriteLine("Add to route {0} wire {1} and slot {2} ", route[j], FindWireId(route[j], route[j + 1]), bufferRouterResponse.FSid);
                        routeHistory.Add(new int[3] { route[j], FindWireId(route[j], route[j + 1]), bufferRouterResponse.FSid });
                    }
                    if (j == route.Length - 1)
                    {
                        routeHistory.Add(new int[3] { route[j], FindWireId(route[j - 1], route[j]), bufferRouterResponse.FSid });
                        routeHistoryList.Add(new String[3] { clientSourceIP, ClientDestinationIP, hashKey }, routeHistory);

                        int rRid = Int32.Parse(recipientRouterIP.Substring(recipientRouterIP.Length - 1, 1));
                        // edgeRouterIDs.Add(hashKey, new int[2] { rSid, rRid });

                        int[] tmp = edgeRouterIDs[hashKey];
                        //todo tutaj do edgeRouterIDs dopisuje rRid, a co gdy CONNECTION_UNAVAILABLE ? 
                        //todo wpis zostanie nie kompletny, gdzie usuwanie? w disroute brak tego
                        edgeRouterIDs[hashKey] = new int[] { tmp[0], rRid };
                        Console.WriteLine("Route set.");
                        foreach (int[] rh in routeHistory)
                        {
                            Console.WriteLine("Router {0} wire {1} and slot {2}. ", rh[0], rh[1] , rh[2]);
                        }
                        return;
                    }
                }
                else if (bufferRouterResponse.message.Equals(ExtSrc.AgentComProtocol.CONNECTION_UNAVAILABLE))
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
            if (edgeRouterIDs.TryGetValue(hashKey, out edgeRouters))
            {
                for (int i = routeHistory.Count - 1; i >= 0; i--)
                {
                    //Console.WriteLine("ONE DISROUTE MSG IS GOING TO BE SENT : " + routeHistory.ElementAt(i)[0] + " -> " + routeHistory.ElementAt(i)[1] + " -> " + routeHistory.ElementAt(i)[2]);
                    if ((edgeRouters[0] != routeHistory.ElementAt(i)[0]) && (edgeRouters[1] != routeHistory.ElementAt(i)[0]))
                    {
                        //   agentData.firstWireID, agentData.FSid, agentData.secondWireID, agentData.secondFSid
                        bufferRouterResponse = null;
                        Send(String.Format("127.0.1." + routeHistory.ElementAt(i)[0]), new ExtSrc.AgentData(ExtSrc.AgentComProtocol.DISROUTE, routeHistory.ElementAt(i)[1], routeHistory.ElementAt(i)[2]));
                        var waitTime = 0;
                        while (bufferRouterResponse == null)
                        {
                            //w8 na odp od routera
                            Thread.Sleep(50);
                            waitTime += 50;
                            if (waitTime > TIMEOUT) break;
                        }
                        if (bufferRouterResponse != null && bufferRouterResponse.message.Equals(ExtSrc.AgentComProtocol.DISROUTE_IS_DONE))
                            //Console.WriteLine("DISROUTE FOR " + String.Format("127.0.1." + routeHistory.ElementAt(i)[0]) + " IS DONE");
                            Console.WriteLine("Disoute done.");
                        else if (bufferRouterResponse == null || bufferRouterResponse.message.Equals(ExtSrc.AgentComProtocol.DISROUTE_ERROR))
                        {
                            //Console.WriteLine("DISROUTE FOR " + String.Format("127.0.1." + routeHistory.ElementAt(i)[0]) + " ERROR!!!");
                            Console.WriteLine("Disroute error.");
                            continue; //todo tu był return test aby disroutowało sie do konca nawet jak jest ktorys router off
                        }
                    }
                    else
                    {
                        bufferRouterResponse = null;
                        Send(String.Format("127.0.1." + routeHistory.ElementAt(i)[0]), new ExtSrc.AgentData(ExtSrc.AgentComProtocol.DISROUTE_EDGE, routeHistory.ElementAt(i)[1], routeHistory.ElementAt(i)[2]) { uniqueKey = hashKey });
                        var waitTime = 0;
                        while (bufferRouterResponse == null)
                        {
                            //w8 na odp od routera
                            Thread.Sleep(50);
                            waitTime += 50;
                            if (waitTime > TIMEOUT) break;
                        }

                        if (bufferRouterResponse != null && bufferRouterResponse.message.Equals(ExtSrc.AgentComProtocol.DISROUTE_EDGE_IS_DONE))
                            //Console.WriteLine("DISROUTE EDGE FOR " + String.Format("127.0.1." + routeHistory.ElementAt(0)[0]) + " IS DONE");
                            Console.WriteLine("Disoute done.");
                        else if (bufferRouterResponse == null || bufferRouterResponse.message.Equals(ExtSrc.AgentComProtocol.DISROUTE_ERROR_EDGE))
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
            foreach (var dd in dijkstraDataList)
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

        // Calculating FrequencySlotUnits needed for transport.
        // returns arraylist[modulation, fsucount]
        private ArrayList calculateFSUcount(int wireID, int bitrate)
        {
            Modulation mod = Modulation.NULL;
            double FSUcount = 0;
            foreach (ExtSrc.DijkstraData dt in dijkstraDataList)
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
                    //todo te 20 trzeba przemyslec
                    FSUcount = Math.Round((double)(bitrate) / 20.0 * (double)modVal);
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
            List<ExtSrc.DijkstraData> duplicates = dijkstraDataList.GroupBy(x => x, new ExtSrc.DijkstraEqualityComparer()).
                SelectMany(grp => grp.Skip(1)).ToList();
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
                foreach (ExtSrc.DijkstraData dd in dijkstraDataList)
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

               int[] route = dijkstra.evaluate(dta, id1, id2);
              
               return route;
        }

        public void Send(String ip, ExtSrc.AgentData adata)
        {
            //var s = sockets[ip];
            Socket client;
            if (!sockets.TryGetValue(ip, out client))
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
                sendDone.WaitOne();
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
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void SendOnlineRequest(Socket socketRouterOnline)
        {
            try
            {
                var fs = new MemoryStream();
                new BinaryFormatter().Serialize(fs, "IS_ONLINE");
                var buffer = fs.ToArray();
                socketRouterOnline.BeginSend(buffer, 0, buffer.Length, 0, SendOnlineRequestCallback, socketRouterOnline);
                sendDone.WaitOne(); //todo?
            }
            catch (SocketException e)
            {
//                int line = (new StackTrace(e, true)).GetFrame(0).GetFileLineNumber();
//                Console.WriteLine("Router not responding (ERROR LINE: " + line + ")");
            }
        }

        private void SendOnlineRequestCallback(IAsyncResult ar)
        {
            try
            {
                var client = (Socket)ar.AsyncState;
                var bytesSent = client.EndSend(ar);
                sendDone.Set();//todo?
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

        private void CloseRouterSocket(IPEndPoint ipEndPoint)
        {
            Console.WriteLine("Router {0} closed.", ipEndPoint);
            var router = OnlineRoutersList.FirstOrDefault(s => Equals(s.Socket.RemoteEndPoint, ipEndPoint));
            if (router == null) return;
            router.IsOnline = false;
            var ip = ipEndPoint.Address.ToString();
            var routerId = Int32.Parse(ipEndPoint.Address.ToString().Substring(ipEndPoint.Address.ToString().Length - 1, 1));
            sockets.Remove(Convert.ToString((ipEndPoint).Address));
            router.Socket.Close();
            OnlineRoutersList.Remove(router);
            var dijkstraDataRemoveList = dijkstraDataList.Where(dd => dd.routerID == routerId).ToList();
            {
                //Console.WriteLine("DD REMOVED");
                dijkstraDataRemoveList.ForEach(dijkstraData => dijkstraDataList.Remove(dijkstraData));
            }
            var myValue = clientMap.Where(x => x.Value == ip).ToList();
            myValue.ForEach(strings => clientMap.Remove(strings.Key));
            //todo usuwanie polaczen po usunieciu routera, szukanie alternatywnej drogi??
        }

        public static long GetTimestamp(DateTime value)
        {
            return value.Ticks;
        }

        public void Close()
        {
            socket.Close();
            sockets.Values.ToList().ForEach(s => s.Close());
            OnlineAgentSocket.Close();
            System.Windows.Forms.Application.Exit();
            System.Environment.Exit(1);
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
