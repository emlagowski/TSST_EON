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

namespace Agent
{
    public class Communication
    {
        Socket socket;
        public ManualResetEvent allDone = new ManualResetEvent(false);
        public ManualResetEvent sendDone = new ManualResetEvent(false);
        private bool running = true;
        public Dijkstra dijkstra { get; set; }

        // String addressIP of endPoint, socket
        Dictionary<String, Socket> sockets { get; set; }

        List<ExtSrc.AgentData> _bufferAgentData;

        ExtSrc.AgentData bufferRouterResponse;
        // routerAaddress, routerBaddress, hashKey - > routeHistory (list of routerID, wireId, FSid )
        public Dictionary<String[], List<int[]>> routeHistoryList { get; set; }
        // hashKey, {ID of edge routers for path}
        public Dictionary<String, int[]> edgeRouterIDs { get; set; }
        
        // ROUTING DIJKSTRA DATA
        public List<ExtSrc.DijkstraData> dijkstraDataList { get; set; }

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
                Console.WriteLine("SET called.");
                _bufferAgentData = value;
            }
        }

        public Communication()
        {
            routeHistoryList = new Dictionary<String[], List<int[]>>(new MyEqualityStringComparer());
            edgeRouterIDs = new Dictionary<String, int[]>();
            dijkstraDataList = new List<ExtSrc.DijkstraData>();
            sockets = new Dictionary<String, Socket>();
            clientMap = new Dictionary<String, String>();
            dijkstra = new Dijkstra();
            bufferAgentData = new List<ExtSrc.AgentData>();
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Parse("127.6.6.6"), 6666));
            socket.ReceiveBufferSize = 1024 * 100;

            Thread t = new Thread(Run);
            t.Start();


            Thread dataProcessor = new Thread(ProcessAgentDataRun);
            dataProcessor.Start();

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
            sockets.Add(Convert.ToString((handler.RemoteEndPoint as IPEndPoint).Address), handler);
            //addConnection(handler.RemoteEndPoint.ToString());
            //Console.WriteLine("Socket [{0}] {1} - {2} was added to sockets list", sockets.Count, handler.LocalEndPoint.ToString(), handler.RemoteEndPoint.ToString());

            // Create the state object.
            var state = new StateObject {workSocket = handler};
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
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

            state.dt = (ExtSrc.AgentData)formattor.Deserialize(ms);

            Console.WriteLine("Read '{0}'[{1} bytes] from socket {2}.",
                      state.dt.ToString(), bytesRead, IPAddress.Parse(((IPEndPoint)handler.RemoteEndPoint).Address.ToString()));

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
                    Console.WriteLine("REGISTER");
                    foreach (DijkstraData dd in agentData.wireIDsList)
                    {
                        Console.WriteLine("ADDED "+dd.routerID+" "+dd.wireID+" "+dd.wireDistance);
                        dijkstraDataList.Add(dd);
                    }
                    //rejestruje sie na liste 
                    break;
                case ExtSrc.AgentComProtocol.SET_ROUTE_FOR_ME:
                    Console.WriteLine("SET_ROUTE_FOR_ME");
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
                    Console.WriteLine("REGISTER_CLIENT");
                    clientMap.Add(agentData.clientIPAddress, agentData.routerIPAddress);
                    break;
                default:
                    Console.WriteLine("Zły msg przybył");
                    break;
            }
        }
        private void setRoute(String clientSourceIP, String ClientDestinationIP, int bitrate, int[] excludedW, String hashKey)
        {
            List<int[]> routeHistory = new List<int[]>(); 
            int[] excludedWiresIDs = excludedW;
            int startfrequency = -1;
            // zamiana ip koncowego klienta na ip jego routera
            String senderRouterIP;
            clientMap.TryGetValue(clientSourceIP, out senderRouterIP);
            if (senderRouterIP == null)
            {
                Console.WriteLine("NIE MA TAKIEGO (BICIA) KLIENTA.(SENDER)");
                return;
            }
            String recipientRouterIP;
            clientMap.TryGetValue(ClientDestinationIP, out recipientRouterIP);
            if (recipientRouterIP == null)
            {
                Console.WriteLine("NIE MA TAKIEGO (BICIA) KLIENTA.(RECIPIENT)");
                return;
            }
            //String Client = Int32.Parse(sourceIP.Substring(sourceIP.Length - 1, 1));
            int ClientSenderID = Int32.Parse(clientSourceIP.Substring(clientSourceIP.Length - 1, 1));
            //int RouterRecipientID = Int32.Parse(destinationIP.Substring(destinationIP.Length - 1, 1));
            int ClientRecipientID = Int32.Parse(ClientDestinationIP.Substring(ClientDestinationIP.Length - 1, 1));
            int[] route = calculateRoute(senderRouterIP, recipientRouterIP, excludedWiresIDs);
            if (route == null)
            {
                Console.WriteLine("AKTUALNIE NIE MOŻNA ZNALEŹĆ DROGI Z " + clientSourceIP+ " DO "+ ClientDestinationIP );
                return;
            }

            for (int j = 0 ; j<route.Length; j++)
            {
                bufferRouterResponse = null;
                
                if (j == 0)
                {
                    //wyslij d source routera
                    Console.WriteLine("WYSYLAM DO PIERWSZEGO EDGE ROUTERA DANE ROUTINGOWE");
                    ArrayList ar = calculateFSUcount(FindWireId(route[j], route[j+1]), bitrate);
                    String ip = String.Format("127.0.1." + route[j]);
                    Send(ip, new ExtSrc.AgentData(ExtSrc.AgentComProtocol.ROUTE_FOR_U_EDGE, (int)ar[1], (Modulation)ar[0],
                                                        FindWireId(route[j], route[j + 1]), ClientSenderID));
                    int rSid = Int32.Parse(senderRouterIP.Substring(senderRouterIP.Length - 1, 1));
                    edgeRouterIDs.Add(hashKey, new int[2] { rSid, -1 });

                    Console.WriteLine("WYSYLALEM DO PIERWSZEGO EDGE ROUTERA DANE ROUTINGOWE ("+ip+")");
                }
                else if (j > 0 && j < route.Length - 1)
                {
                    //fsucount, mod, firstwireid,secondwireid, startingfreq dla odbierajacego kabla bo juz obliczone w poprzednim roouterze
                    //wyslij do zwyklych routerow
                    Console.WriteLine("WYSYLAM DO SRODKOWEGO ROUTERA DANE ROUTINGOWE");
                    ArrayList ar0 = calculateFSUcount(FindWireId(route[j-1], route[j]), bitrate);
                    ArrayList ar = calculateFSUcount(FindWireId(route[j], route[j+1]), bitrate);
                    if ((int)ar[0] == 0 || (int)ar[1] == 0) return;
                    String ip = String.Format("127.0.1." + route[j]);
                    Send(ip, new ExtSrc.AgentData(ExtSrc.AgentComProtocol.ROUTE_FOR_U, (int)ar0[1], (Modulation)ar0[0], (int)ar[1], (Modulation)ar[0], 
                                                    FindWireId(route[j-1], route[j]), FindWireId(route[j], route[j + 1]), startfrequency));
                    Console.WriteLine("WYSYLALEM DO SRODKOWEGO ROUTERA DANE ROUTINGOWE (" + ip + ")");
                }
                else if (j == route.Length - 1)
                {
                    //wyslij do destinationIP routra
                    Console.WriteLine("WYSYLAM DO OSTATNIEGO EDGE ROUTERA DANE ROUTINGOWE");
                    ArrayList ar0 = calculateFSUcount(FindWireId(route[j - 1], route[j]), bitrate);
                    String ip = String.Format("127.0.1." + route[j]);
                    Send(ip, new ExtSrc.AgentData(ExtSrc.AgentComProtocol.ROUTE_FOR_U_EDGE, (int)ar0[1], (Modulation)ar0[0],
                                                        FindWireId(route[j - 1], route[j]), ClientRecipientID));
                    Console.WriteLine("WYSYLALEM DO OSTATNIEGO EDGE ROUTERA DANE ROUTINGOWE (" + ip + ")");
                }
                //sprawdz czy dostepne bitrejty
                while (bufferRouterResponse == null)
                {
                    //todo a co jak sie zatnie? coś nie dojdzie? router padnie?
                    //w8 na odp od routera
                    Thread.Sleep(50);
                }
                
                if (bufferRouterResponse.message.Equals(ExtSrc.AgentComProtocol.CONNECTION_IS_ON))
                {
                    startfrequency = bufferRouterResponse.startingFreq;
                    //dodawanie do routeHistory
                    if (j < route.Length - 1) {
                        Console.WriteLine("ADDING to routeHistory : router_id " + route[j] + " -> wire_id " + FindWireId(route[j], route[j + 1]) + " -> fs_id " + bufferRouterResponse.FSid);
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
                        edgeRouterIDs[hashKey] = new int[] { tmp[0], rRid};
                        Console.WriteLine("ROUTING ZAKONCZONY, ROUTERY SKONFIGUROWANE. MOZNA WYSYLAC WIADOMOSC.");
                        foreach (int[] rh in routeHistory)
                        {
                            Console.WriteLine("routeHistory routerID: " + rh[0] + " wireID : " + rh[1] + " FSid : " + rh[2]);
                        }
                        return;
                    }
                }
                else if(bufferRouterResponse.message.Equals(ExtSrc.AgentComProtocol.CONNECTION_UNAVAILABLE))
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

        public Boolean disroute(List<int[]> routeHistory, String hashKey)
        {
            int[] edgeRouters;
            if (edgeRouterIDs.TryGetValue(hashKey, out edgeRouters))
            {
                for (int i = routeHistory.Count - 1; i >= 0; i--)
                {
                    Console.WriteLine("ONE DISROUTE MSG IS GOING TO BE SENT : " + routeHistory.ElementAt(i)[0] + " -> " + routeHistory.ElementAt(i)[1] + " -> " + routeHistory.ElementAt(i)[2]);
                    if ((edgeRouters[0] != routeHistory.ElementAt(i)[0]) && (edgeRouters[1] != routeHistory.ElementAt(i)[0]))
                    {
                        //   agentData.firstWireID, agentData.FSid, agentData.secondWireID, agentData.secondFSid
                        bufferRouterResponse = null;
                        Send(String.Format("127.0.1." + routeHistory.ElementAt(i)[0]), new ExtSrc.AgentData(ExtSrc.AgentComProtocol.DISROUTE, routeHistory.ElementAt(i)[1], routeHistory.ElementAt(i)[2]));

                        while (bufferRouterResponse == null)
                        {
                            //w8 na odp od routera
                            Thread.Sleep(50);
                        }
                        if (bufferRouterResponse.message.Equals(ExtSrc.AgentComProtocol.DISROUTE_IS_DONE))
                            Console.WriteLine("DISROUTE FOR " + String.Format("127.0.1." + routeHistory.ElementAt(i)[0]) + " IS DONE");
                        else if (bufferRouterResponse.message.Equals(ExtSrc.AgentComProtocol.DISROUTE_ERROR))
                        {
                            Console.WriteLine("DISROUTE FOR " + String.Format("127.0.1." + routeHistory.ElementAt(i)[0]) + " ERROR!!!");
                            return false;
                        }
                    }
                    else
                    {
                        bufferRouterResponse = null;
                        Send(String.Format("127.0.1." + routeHistory.ElementAt(i)[0]), new ExtSrc.AgentData(ExtSrc.AgentComProtocol.DISROUTE_EDGE, routeHistory.ElementAt(i)[1], routeHistory.ElementAt(i)[2]));
                        while (bufferRouterResponse == null)
                        {
                            //w8 na odp od routera
                            Thread.Sleep(50);
                        }

                        if (bufferRouterResponse.message.Equals(ExtSrc.AgentComProtocol.DISROUTE_EDGE_IS_DONE))
                            Console.WriteLine("DISROUTE EDGE FOR " + String.Format("127.0.1." + routeHistory.ElementAt(0)[0]) + " IS DONE");
                        else if (bufferRouterResponse.message.Equals(ExtSrc.AgentComProtocol.DISROUTE_ERROR_EDGE))
                        {
                            Console.WriteLine("DISROUTE EDGE FOR " + String.Format("127.0.1." + routeHistory.ElementAt(0)[0]) + " ERROR!!!");
                            return false;
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
                    Console.WriteLine("bitrate "+bitrate+" fsucnt "+FSUcount+" modvl "+ modVal);
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

            }
            MemoryStream fs = new MemoryStream();

            BinaryFormatter formatter = new BinaryFormatter();

            formatter.Serialize(fs, adata);

            byte[] buffer = fs.ToArray();



            // Begin sending the data to the remote device.
            client.BeginSend(buffer, 0, buffer.Length, 0,
                new AsyncCallback(SendCallback), client);
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
                Console.WriteLine("Agent: Sent {0} bytes to Router.", bytesSent);

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


}
