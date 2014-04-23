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

        public void Run()
        {
            while (true)
            {
                Console.WriteLine("waiting ...");
                _client = _listener.AcceptTcpClient();
                Console.WriteLine("got something");
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
