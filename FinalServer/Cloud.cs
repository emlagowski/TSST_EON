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

#endregion

namespace Cloud
{
    public class Cloud : IDisposable
    {
        IPEndPoint endPoint;
        Socket localSocket;
        List<Socket> sockets;
        CloudWires cloudWires;
        public ManualResetEvent allDone = new ManualResetEvent(false);


        public Cloud(string ip, int port)
        {
            cloudWires = new CloudWires();
            sockets = new List<Socket>();
            endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            localSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            localSocket.Bind(endPoint);
            new Thread(Run).Start();
            new Thread(delegate()
            {
                while (true)
                {

                    Thread.Sleep(1000);
                }
            }).Start();
        }

        void Run()
        {
            try
            {
                localSocket.Listen(500);
                while (true)
                {
                    // Set the event to nonsignaled state.
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.
                    if (localSocket != null) localSocket.BeginAccept(AcceptCallback, localSocket);

                    // Wait until a connection is made before continuing.
                    allDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                //Log.d(e.ToString());
            }
        }

        public void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                // Signal the main thread to continue.
                allDone.Set();

                // Get the socket that handles the client request.
                var listener = (Socket)ar.AsyncState;
                var handler = listener.EndAccept(ar);
                sockets.Add(handler);

                // Create the state object.
                var state = new StateObject { workSocket = handler };
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, ReadCallback, state);
            }
            catch (Exception)
            {

            }
        }

        public void ReadCallback(IAsyncResult ar)
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

                state.dt = (ExtSrc.Data)formattor.Deserialize(ms);

                if (state.dt.info.Equals("CLOSING_UNIT"))
                {
                    handler.Close();
                    //Log.d(sockets.Remove(handler) + " " + sockets.Count);

                    return;
                }
                Log.d(String.Format("R: {0} bytes from {1}", bytesRead, IPAddress.Parse(((IPEndPoint)handler.RemoteEndPoint).Address.ToString())));

                var s = FindTarget((IPEndPoint)handler.RemoteEndPoint);

                var newState = new StateObject { workSocket = handler };

                Send(s, state.dt);

                handler.BeginReceive(newState.buffer, 0, StateObject.BufferSize, 0,
                    ReadCallback, newState);
            }
            catch (SocketException)
            {
                localSocket.BeginAccept(AcceptCallback, localSocket);
            }
            catch (Exception e)
            {
                //Log.d(e.ToString());
            }
        }

        private Socket FindTarget(IPEndPoint iPEndPoint)
        {
            foreach (var cep in cloudWires.ConnectedEndPointsList)
            {
                if (iPEndPoint.Equals(cep.One))
                {
                    var cep1 = cep;
                    foreach (var socket in sockets.Where(socket => socket.RemoteEndPoint.Equals(cep1.Two)))
                    {
                        return socket;
                    }
                }
                if (iPEndPoint.Equals(cep.Two))
                {
                    var cep1 = cep;
                    foreach (var socket in sockets.Where(socket => socket.RemoteEndPoint.Equals(cep1.One)))
                    {
                        return socket;
                    }
                }
            }
            //Log.d("Target was not found in the CloudWires.");
            return null;
        }

        private void Send(Socket handler, ExtSrc.Data data)
        {
            try
            {
                var ep = handler.RemoteEndPoint as IPEndPoint;
                var fs = new MemoryStream();
                var formatter = new BinaryFormatter();
                formatter.Serialize(fs, data);

                var buffer = fs.ToArray();

                // Begin sending the data to the remote device.
                handler.BeginSend(buffer, 0, buffer.Length, 0,
                    new AsyncCallback(SendCallback), handler);
            }
            catch (Exception)
            {
                //int line = (new StackTrace(e, true)).GetFrame(0).GetFileLineNumber();
                //Log.d("Router not responding (ERROR LINE: " + line + ")");
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                var handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                var bytesSent = handler.EndSend(ar);
                Log.d(String.Format("S: {0} bytes to {1}.", bytesSent, IPAddress.Parse(((IPEndPoint)handler.RemoteEndPoint).Address.ToString())));
            }
            catch (Exception e)
            {
                //Log.d(e.ToString());
            }
        }

        public void Close()
        {
            localSocket.Close();
            sockets.Where(s => s != null).ToList().ForEach(s => s.Close());
            System.Windows.Forms.Application.Exit();
            System.Environment.Exit(1);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose managed resources
                localSocket.Close();
                foreach (var socket in sockets)
                {
                    socket.Close();
                }
                allDone.Close();
            }
            // free native resources
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    public class CloudWires
    {
        public List<ConnectedEndPoints> ConnectedEndPointsList { get; private set; }

        public CloudWires()
        {
            ConnectedEndPointsList = new List<ConnectedEndPoints>();
            var xmlString = File.ReadAllText("cloudwires.xml");
            using (var reader = XmlReader.Create(new StringReader(xmlString)))
            {
                while (reader.ReadToFollowing("wire"))
                {
                    reader.MoveToFirstAttribute();
                    var ipA = reader.Value;
                    reader.MoveToNextAttribute();
                    var ipB = reader.Value;
                    reader.MoveToNextAttribute();
                    var wireId = reader.Value;
                    reader.MoveToNextAttribute();
                    var distance = reader.Value;
                    reader.MoveToNextAttribute();
                    var maxFreqSlots = reader.Value;
                    reader.MoveToNextAttribute();
                    var portPrefix = reader.Value;
                    reader.MoveToNextAttribute();
                    var spectralWidth = reader.Value;

                    for (var i = 0; i < Int32.Parse(maxFreqSlots); i++)
                    {
                        var port = String.Concat(new[] { portPrefix, i.ToString() });

                        ConnectedEndPointsList.Add(new ConnectedEndPoints(new IPEndPoint(IPAddress.Parse(ipA), Convert.ToInt32(port)),
                                               new IPEndPoint(IPAddress.Parse(ipB), Convert.ToInt32(port)),
                                            Convert.ToInt32(wireId)));
                    }
                }
            }
        }
    }

    public class ConnectedEndPoints
    {
        IPEndPoint _one, _two;
        int _id;

        public IPEndPoint One
        {
            get { return _one; }
        }

        public IPEndPoint Two
        {
            get { return _two; }
        }

        public ConnectedEndPoints(IPEndPoint first, IPEndPoint second, int id)
        {
            _one = first;
            _two = second;
            _id = id;
        }
    }

    public class StateObject
    {
        // Client  socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024 * 5;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public ExtSrc.Data dt;
    }
}
