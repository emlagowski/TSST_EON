using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CloudNew
{
    public class HandleClientRequest
    {
        TcpClient _clientOne, _clientTwo;
        NetworkStream _networkStreamOne = null, _networkStreamTwo = null;

        public HandleClientRequest(TcpClient clientOne, TcpClient clientTwo)
        {
            this._clientOne = clientOne;
            this._clientTwo = clientTwo;
            //Console.WriteLine("new handleclient");
        }
        public void StartClient()
        {
            while (_clientOne == null || _clientTwo == null) { Thread.Sleep(100); }
            Console.WriteLine("poszlo");
            _networkStreamOne = _clientOne.GetStream();
            _networkStreamTwo = _clientTwo.GetStream();
            WaitForRequest();
        }

        public void WaitForRequest()
        {
            byte[] buffer1 = new byte[_clientOne.ReceiveBufferSize];

            _networkStreamOne.BeginRead(buffer1, 0, buffer1.Length, ReadCallback, buffer1);
        }

        private void ReadCallback(IAsyncResult result)
        {
            NetworkStream nsStart = _clientOne.GetStream();
            NetworkStream nsStop = _clientTwo.GetStream();
            try
            {
                int read = nsStart.EndRead(result);
                if (read == 0)
                {
                    _networkStreamOne.Close();
                    _clientOne.Close();
                    return;
                }

                byte[] buffer = result.AsyncState as byte[];
                string data = Encoding.Default.GetString(buffer, 0, read);
                //do the job with the data here
                //send the data back to client.
                Byte[] sendBytes = Encoding.ASCII.GetBytes("Processed " + data);
                //nsStart.Write(sendBytes, 0, sendBytes.Length);
                //nsStart.Flush();
                nsStop.Write(sendBytes, 0, sendBytes.Length);
                nsStop.Flush();
                Console.WriteLine("Transferring '{0}' successful." , data);
            }
            catch (Exception ex)
            {
                throw;
            }

            this.WaitForRequest();
        }
    }
}
