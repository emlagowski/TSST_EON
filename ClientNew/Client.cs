using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ClientNew
{
    class Client
    {
        int _port;
        string _address;
        TcpListener _listener;
        TcpClient _client;
        Boolean _isOn;

        public Client(String address, int port)
        {
            _port = port;
            _address = address;
            _listener = new TcpListener(IPAddress.Parse(_address), _port);
            _listener.Start();
            _isOn = false;
            Thread t = new Thread(Run);
            t.Start();
        }

        public String getAddress()
        {
            String s = String.Format(_address + ":" + _port); 
            return s;
        }

        public void SendData(string dataTosend)
        {
            while (_client == null) Thread.Sleep(1000);
            Console.WriteLine("{0}:{1} - Send Data - {2}", _address, _port, dataTosend);
            if (string.IsNullOrEmpty(dataTosend))
                return;
            NetworkStream serverStream = _client.GetStream();
            byte[] outStream = System.Text.Encoding.ASCII.GetBytes(dataTosend);
            serverStream.Write(outStream, 0, outStream.Length);
            serverStream.Flush();
        }

        public void ReceiveData()
        {
            while (_client == null) Thread.Sleep(1000);
            StringBuilder message = new StringBuilder();
            NetworkStream serverStream = _client.GetStream();
            serverStream.ReadTimeout = 100;
            //the loop should continue until no dataavailable to read and message string is filled.
            //if data is not available and message is empty then the loop should continue, until
            //data is available and message is filled.
            while (true)
            {
                if (serverStream.DataAvailable)
                {
                    int read = serverStream.ReadByte();
                    if (read > 0)
                        message.Append((char)read);
                    else
                        break;
                }
                else if (message.ToString().Length > 0)
                    break;
            }
            Console.WriteLine("{0}:{1} - Received Data - {2}", _address, _port, message.ToString());
            //return message.ToString();
        }

        public void Run()
        {
            TcpClient tmp = new TcpClient("127.0.0.1", 2222);
            NetworkStream ns = tmp.GetStream();
            byte[] sendMsg = System.Text.Encoding.ASCII.GetBytes(getAddress());
            ns.Write(sendMsg, 0, sendMsg.Length);
            Console.WriteLine("{0}:{1} - Init sent to cloud.", _address, _port);
            while (true)
            {
                //Console.WriteLine("{0}:{1} - Waiting ...", _address, _port);
                _client = _listener.AcceptTcpClient();
                Console.WriteLine("{0}:{1} - got something", _address, _port);
            }
        }
    }
}
