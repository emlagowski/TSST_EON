using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;

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

        public void Request( TcpClient clientOne, TcpClient clientTwo)
        {
            Console.WriteLine("tworzymy handlerClient"); // czy klient jest nullem
            HandleClientRequest Req = new HandleClientRequest(clientOne, clientTwo);
            Req.StartClient();
        }
        public Thread StartRequestThread( TcpClient clientOne, TcpClient clientTwo)
        {
            var t = new Thread(() => Request(clientOne, clientTwo));
            t.Start();
            return t;
        }

        public void RequestInit()
        {
            StartRequestThread( _inA,  _outB);
            StartRequestThread( _inB,  _outA);
        }
        public void RunListener(string address, int port, out TcpClient client)
        {
            TcpListener listener = new TcpListener(IPAddress.Parse(address), port);
            listener.Start();
            client = listener.AcceptTcpClient();
            Console.WriteLine("-------------------------------client stworzony");

        }

        private void intitListeners()
        {
            String msg = "xxx";
            byte[] bytesy = System.Text.Encoding.ASCII.GetBytes(msg);
            TcpClient t1 = new TcpClient(Addresses[0], Ports[0]);
            TcpClient t2 = new TcpClient(Addresses[0], Ports[1]);
            TcpClient t3 = new TcpClient(Addresses[1], Ports[2]);
            TcpClient t4 = new TcpClient(Addresses[1], Ports[3]);
            NetworkStream n1 = t1.GetStream();
            NetworkStream n2 = t2.GetStream();
            NetworkStream n3 = t3.GetStream();
            NetworkStream n4 = t4.GetStream();
            n1.Write(bytesy, 0, bytesy.Length);
            n2.Write(bytesy, 0, bytesy.Length);
            n3.Write(bytesy, 0, bytesy.Length);
            n4.Write(bytesy, 0, bytesy.Length);
        }

        internal void start()
        {
            var t1 = new Thread(() => RunListener(_addressOne, _portInA, out _inA));
            t1.Start();
            var t2 = new Thread(() => RunListener(_addressTwo, _portInB, out _inB));
            t2.Start();
            var t3 = new Thread(() => RunListener(_addressOne, _portOutA, out _outA));
            t3.Start();
            var t4 = new Thread(() => RunListener(_addressTwo, _portOutB, out _outB));
            t4.Start();

            Thread.Sleep(100);
            intitListeners();
            RequestInit();
            _isOn = true;
        }
    }
}
