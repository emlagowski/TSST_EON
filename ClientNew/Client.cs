using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace ClientNew
{
    class Client
    {
        int _port;
        string _address;
        TcpClient _client;
        Boolean _isOn;

        public Client(String address, int port)
        {
            _port = port;
            _address = address;
            TcpClient client = new TcpClient();
            _isOn = false;
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
