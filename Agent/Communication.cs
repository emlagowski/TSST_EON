using System;
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

        // List<Socket> sockets;
        Dictionary<String, Socket> sockets { get; set; }

        List<ExtSrc.AgentData> _bufferAgentData;
        ExtSrc.AgentData bufferRouterResponse;



        // ROUTING DIJKSTRA DATA
        //public List<int> routerIDsList { get; set; }
        //public List<int> wireIDsList { get; set; }
        //public List<int> wireDistancesList { get; set; }
        public List<ExtSrc.DijkstraData> dijkstraDataList { get; set; }

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

        //public List<ExtSrc.AgentData> bufferDataAgent
        //{
        //    get { return agentData; }
        //    set { agentData = value; }
        //}

        public Communication()
        {
            dijkstraDataList = new List<ExtSrc.DijkstraData>();
            sockets = new Dictionary<String, Socket>();
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
                    Console.WriteLine("Waiting for a connection...");
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

            state.dt = (ExtSrc.AgentData)formattor.Deserialize(ms);

            Console.WriteLine("Read '{0}'[{1} bytes] from socket {2}.",
                      state.dt.ToString(), bytesRead, IPAddress.Parse(((IPEndPoint)handler.RemoteEndPoint).Address.ToString()));

            ///
            /// Odbieramy dane od routera dodajemy do bufora,
            /// aby odebrac dane od wszystkich i nic nie stracić
            /// 
            if (state.dt.message.Equals(ExtSrc.AgentComProtocol.CONNECTION_IS_ON) || state.dt.message.Equals(ExtSrc.AgentComProtocol.CONNECTION_UNAVAILABLE))
                bufferRouterResponse = state.dt;
            else
                bufferAgentData.Add(state.dt);


            StateObject newState = new StateObject();
            newState.workSocket = handler;
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
                    foreach (ExtSrc.DijkstraData dd in agentData.wireIDsList)
                    {
                        Console.WriteLine(dd.routerID+" "+dd.wireID+" "+dd.wireDistance);
                        dijkstraDataList.Add(dd);
                        
                    }
                    if (agentData.wireIDsList.ElementAt(0).routerID == 5)
                    {
                        calculateRoute("127.0.0.2", "127.0.0.4", null);
                    }

                    //rejestruje sie na liste 
                    break;
                case ExtSrc.AgentComProtocol.SET_ROUTE_FOR_ME:
                    //policz droge i odeslij do wszystkich ruterow ktore maja byc droga informacje route-for-you
                    calculateRoute(agentData.originatingAddress, agentData.targetAddress, null);
                    break;
                case ExtSrc.AgentComProtocol.MSG_DELIVERED:
                    //info o tym ze jakas wiadomosc dotarla na koniec drogi
                    break;
                case ExtSrc.AgentComProtocol.CONNECTION_IS_ON:
                    //zestawianie zakonczone w danym routerze
                    break;
                default:
                    Console.WriteLine("Zły msg przybył");
                    break;
            }
        }
        private void setRoute(String sourceIP, String destinationIP, int[] excludedWiresIDs, int bitrate)
        {
            int startfrequency = -1;
            int[] route = calculateRoute(sourceIP, destinationIP, excludedWiresIDs);

            for (int j = 0 ; j<route.Length; j++)
            {
                bufferRouterResponse = null;
                if (j == 0)
                {
                    //wyslij d source routera
                    
                }
                else if (j > 0 && j < route.Length - 1)
                {
                    //wyslij do zwyklych routerow
                }
                else if (j == route.Length)
                {
                    //wyslij do destinationIP routra
                }
                //sprawdz czy dostepne bitrejty
                while (bufferRouterResponse == null)
                {
                    //w8 na odp od routera
                    Thread.Sleep(50);
                }
                if (bufferRouterResponse.message.Equals(ExtSrc.AgentComProtocol.CONNECTION_IS_ON))
                {
                    startfrequency = bufferRouterResponse.startingFreq;

                }
                else if(bufferRouterResponse.message.Equals(ExtSrc.AgentComProtocol.CONNECTION_UNAVAILABLE))
                {
                    //rozlaczyc to co juz zestawione i zaczac liczyc dijkstre bez kabla ktory nie mial miejsca
                }
                String ip = String.Format("127.0.0." + j);
                Send(ip, new ExtSrc.AgentData());

            }

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
}
