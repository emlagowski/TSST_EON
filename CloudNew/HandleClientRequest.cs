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
        TcpClient _clientSocket, _clientToSend;
        NetworkStream _networkStream = null, _targetStream = null;
        public HandleClientRequest(TcpClient clientConnected, TcpClient clientToSend)
        {
            this._clientSocket = clientConnected;
            this._clientToSend = clientToSend;
        }
        public void StartClient()
        {
            _networkStream = _clientSocket.GetStream();
            _targetStream = _clientToSend.GetStream();
            WaitForRequest();
        }

        public void WaitForRequest()
        {
            byte[] buffer = new byte[_clientSocket.ReceiveBufferSize];

            _networkStream.BeginRead(buffer, 0, buffer.Length, ReadCallback, buffer);
        }

        private void ReadCallback(IAsyncResult result)
        {
            NetworkStream networkStream = _clientSocket.GetStream();
            NetworkStream targetStream = _clientToSend.GetStream();
            try
            {
                int read = networkStream.EndRead(result);
                if (read == 0)
                {
                    _networkStream.Close();
                    _clientSocket.Close();
                    return;
                }

                byte[] buffer = result.AsyncState as byte[];
                string data = Encoding.Default.GetString(buffer, 0, read);
                //do the job with the data here
                //send the data back to client.
                Byte[] sendBytes = Encoding.ASCII.GetBytes("Processed " + data);
                //networkStream.Write(sendBytes, 0, sendBytes.Length);
                //networkStream.Flush();
                targetStream.Write(sendBytes, 0, sendBytes.Length);
                targetStream.Flush();
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
