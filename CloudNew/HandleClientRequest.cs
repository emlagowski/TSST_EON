using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace CloudNew
{
    public class HandleClientRequest
    {
        TcpClient _clientOne, _clientTwo;
        NetworkStream _networkStreamOne = null, _networkStreamTwo = null;
        IAsyncResult _asyncReadOne, _asyncReadTwo;
        public HandleClientRequest(TcpClient clientOne, TcpClient clientTwo)
        {
            this._clientOne = clientOne;
            this._clientTwo = clientTwo;
        }
        public void StartClient()
        {
            _networkStreamOne = _clientOne.GetStream();
            _networkStreamTwo = _clientTwo.GetStream();
            WaitForRequest();
        }

        public void WaitForRequest()
        {
            byte[] buffer1 = new byte[_clientOne.ReceiveBufferSize];
            byte[] buffer2 = new byte[_clientTwo.ReceiveBufferSize];
            _asyncReadOne = _networkStreamOne.BeginRead(buffer1, 0, buffer1.Length, ReadCallbackOne, buffer1);
            _asyncReadTwo = _networkStreamTwo.BeginRead(buffer2, 0, buffer2.Length, ReadCallbackTwo, buffer2);
        }

        private void ReadCallbackOne(IAsyncResult result)
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
                //networkStream.Write(sendBytes, 0, sendBytes.Length);
                //networkStream.Flush();
                nsStop.EndRead(_asyncReadTwo);
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

        private void ReadCallbackTwo(IAsyncResult result)
        {
            NetworkStream nsStart = _clientTwo.GetStream();
            NetworkStream nsStop = _clientOne.GetStream();
            try
            {
                int read = nsStart.EndRead(result);
                if (read == 0)
                {
                    _networkStreamTwo.Close();
                    _clientTwo.Close();
                    return;
                }

                byte[] buffer = result.AsyncState as byte[];
                string data = Encoding.Default.GetString(buffer, 0, read);
                //do the job with the data here
                //send the data back to client.
                Byte[] sendBytes = Encoding.ASCII.GetBytes("Processed " + data);
                //networkStream.Write(sendBytes, 0, sendBytes.Length);
                //networkStream.Flush();
                nsStop.EndRead(_asyncReadOne);
                nsStop.Write(sendBytes, 0, sendBytes.Length);
                nsStop.Flush();
                Console.WriteLine("Transferring '{0}' successful.", data);
            }
            catch (Exception ex)
            {
                throw;
            }

            this.WaitForRequest();
        }
    }
}
