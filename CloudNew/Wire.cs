using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace CloudNew
{
    class Wire
    {
        String _addressOne, _addressTwo;
        public Boolean _isOnOne, _isOnTwo, _isOn;
        Int32 _portOne, _portTwo;
        TcpClient _first, _second;

        public String[] Addresses
        {
            get
            {
                String[] s = { _addressOne, _addressTwo };
                return s;
            }
        }

        public Int32[] Ports
        {
            get
            {
                Int32[] i = { _portOne, _portTwo };
                return i;
            }
        }

        public Boolean[] isOn
        {
            get
            {
                Boolean[] b = { _isOnOne, _isOnTwo };
                return b;
            }
        }

        public Wire(String addressOne, Int32 portOne, String addressTwo, Int32 portTwo)
        {
            _addressOne = addressOne;
            _addressTwo = addressTwo;
            _portOne = portOne;
            _portTwo = portTwo;
            _isOnOne = false;
            _isOnTwo = false;
        }

        public void Run()
        {
            HandleClientRequest firstReq = new HandleClientRequest(_first, _second);
            firstReq.StartClient();
            HandleClientRequest secondReq = new HandleClientRequest(_second, _first);
            secondReq.StartClient();
        }

        internal void start()
        {
            _first = new TcpClient();
            _first.Connect(_addressOne, _portOne);
            _second = new TcpClient();
            _second.Connect(_addressTwo, _portTwo);
            //Thread thread = new Thread(Run);
            //thread.Start();
            Run();
            _isOn = true;
        }
    }
}
