#region

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

#endregion

namespace ExtSrc
{
    public class FrequencySlotUnit : IDisposable
    {
        public int port { get; set; }
        public int ID { get; set; }
        public Boolean isUsed { get; set; }
        public Boolean isOn { get; set; }
        public Socket socket { get; set; }

        public ManualResetEvent sendDone { get; set; }
        private ManualResetEvent connectDone = new ManualResetEvent(false);
        //private ManualResetEvent sendDone = new ManualResetEvent(false);
        //private ManualResetEvent receiveDone = new ManualResetEvent(false);
        //public ManualResetEvent allDone = new ManualResetEvent(false);
        //public ManualResetEvent allReceive = new ManualResetEvent(false);
        public FrequencySlotUnit(int port, int id)
        {
            this.port = port;
            this.ID = id;
        }
        public void initFSUSocket(String address, IPEndPoint cloudEP)
        {
            sendDone = new ManualResetEvent(false);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(address), port);
            socket.Bind(endPoint);
            socket.BeginConnect(cloudEP, new AsyncCallback(ConnectCallback), socket);
        }
        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.
                client.EndConnect(ar);

                //Console.WriteLine("{0} Socket connected to {1}", client.LocalEndPoint.ToString(), client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.
                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void close()
        {
            //Console.WriteLine("FSU closing");
            var fs = new MemoryStream();
            var formatter = new BinaryFormatter();
            var data = new Data(0, "CLOSING_UNIT");
            formatter.Serialize(fs, data);
            var buffer = fs.ToArray();
            try
            {
                socket.BeginSend(buffer, 0, buffer.Length, 0, ar => socket.Close(), socket);
            }
            catch (ObjectDisposedException)
            {
                //Console.WriteLine(e.ToString());
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose managed resources
                socket.Close();
                sendDone.Close();
                connectDone.Close();
            }
            // free native resources
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
