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

        public void connect()
        {
            try
            {
                _client.Connect(_address, _port);
                _isOn = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
