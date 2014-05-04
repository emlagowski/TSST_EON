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

namespace Agent
{
    class Communication
    {
        Socket socket;
        public ManualResetEvent allDone = new ManualResetEvent(false);
        List<Socket> sockets;
        public List<ExtSrc.AgentData> agentData;
        public Communication()
        {
            sockets = new List<Socket>();
            agentData = new List<ExtSrc.AgentData>();
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Parse("127.6.6.6"), 6666));
            Thread t = new Thread(Run);
            t.Start();
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
            sockets.Add(handler);
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


            agentData.Add(state.dt);


            StateObject newState = new StateObject();
            newState.workSocket = handler;
            handler.BeginReceive(newState.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), newState);
           
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
