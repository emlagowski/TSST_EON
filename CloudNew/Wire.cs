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
        Int32 _portInA, _portInB, _portOutA, _portOutB;
        TcpClient _inA, _inB, _outA, _outB;

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
                Int32[] i = { _portInA, _portOutA, _portInB, _portOutB };
                return i;
            }
        }

        public Wire(String addressOne, Int32 portInA, Int32 portOutA, String addressTwo, Int32 portInB, Int32 portOutB)
        {
            _addressOne = addressOne;
            _addressTwo = addressTwo;
            _portInA = portInA;
            _portOutA = portOutA;
            _portInB = portInB;
            _portOutB = portOutB;
            _isOnOne = false;
            _isOnTwo = false;
        }

        public void Run()
        {
            HandleClientRequest ReqA = new HandleClientRequest(_inA, _outB);
            ReqA.StartClient();
            HandleClientRequest ReqB = new HandleClientRequest(_inB, _outA);
            ReqB.StartClient();
        }

        internal void start()
        {
            _inA = new TcpClient(_addressOne, _portInA);
            _inB = new TcpClient(_addressTwo, _portInB);
            _outA = new TcpClient(_addressOne, _portOutA);
            _outB = new TcpClient(_addressTwo, _portOutB);
            Run();
            _isOn = true;
        }
    }
}
