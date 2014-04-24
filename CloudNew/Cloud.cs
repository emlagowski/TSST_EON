using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Collections;
using System.Threading;
using System.Net;

namespace CloudNew
{
    class Cloud
    {
        public const String A1 = "127.0.0.1:1|127.0.0.2:2";
        public const String A2 = "127.0.0.1:1|127.0.0.4:4";
        public const String A3 = "127.0.0.2:2|127.0.0.3:3";
        public const String A4 = "127.0.0.2:2|127.0.0.5:5";
        public const String A5 = "127.0.0.2:2|127.0.0.6:6";
        public const String A6 = "127.0.0.3:3|127.0.0.4:4";
        public const String A7 = "127.0.0.3:3|127.0.0.5:5";
        public const String A8 = "127.0.0.3:3|127.0.0.6:6";
        public const String A9 = "127.0.0.5:5|127.0.0.6:6";
        public String[] paths = {A1, A2, A3, A4, A5, A6, A7, A8, A9};


        ArrayList wires;
        Thread thread;
        TcpListener listener;
        Boolean isOn;
        public Int32 port { get; set; }  
        public IPAddress localIP { get; set; }

        public Cloud(Int32 p){
            localIP = IPAddress.Parse("127.0.0.1");
            port = p;
            listener = new TcpListener(localIP, port);
            thread = new Thread(Run);
            wires = new ArrayList();
            addWires();
            isOn = true;
            thread.Start();
        }

        public void addWires()
        {
            for (int i = 0; i < paths.Length; i++)
            {
                String[] tmp = paths[i].Split('|');
                String[] address1 = tmp[0].Split(':');
                String[] address2 = tmp[1].Split(':');
                wires.Add(new Wire(address1[0], Convert.ToInt32(address1[1]), address2[0], Convert.ToInt32(address2[1])));
                Console.WriteLine("New wire is set: {0}:{1} <-> {2}:{3}", address1[0], address1[1], address2[0], address2[1]); 
            }
        }

        public void update(String address, Int32 port, NetworkStream stream)
        {
            for (int i = 0; i < wires.Count; i++)
            {
                Wire w = wires[i] as Wire;
                String[] addresses = w.Addresses;
                Int32[] ports = w.Ports;

                if ((addresses[0].Equals(address)) && (ports[0] == port))
                {
                    w._isOnOne = true;
                    w.First = stream; 
   
                } else if ((addresses[1].Equals(address)) && (ports[1] == port))
                {
                    w._isOnTwo = true;
                    w.Second = stream;
                }

                if (w._isOnOne && w._isOnTwo && !w._isOn) w.start();
            }
        }

        public void Run(){
            listener.Start();
            while (isOn)
            {
                Console.WriteLine("Waiting for connection...");
                TcpClient client = listener.AcceptTcpClient();
                Console.WriteLine("Data received.");
                NetworkStream stream = client.GetStream();
                Byte[] bytes = new Byte[256];
                String data = null;
                int i = stream.Read(bytes, 0, bytes.Length);
                data = System.Text.Encoding.ASCII.GetString(bytes, 0, bytes.Length);
                Console.WriteLine("Now we are splitting");
                String[] tmp = data.Split('\0');
                String[] address = tmp[0].Split(':');
                //
                TcpListener listenerTemp = new TcpListener(IPAddress.Parse(address[0]), Convert.ToInt32(address[1]));
                listenerTemp.Start();
                Byte[] ok = { 1, 0, 1 };
                Console.WriteLine(System.Text.Encoding.ASCII.GetString(ok, 0, ok.Length));
                stream.Write(ok, 0, ok.Length);
                TcpClient clientTemp = listenerTemp.AcceptTcpClient();
                NetworkStream streamTemp = clientTemp.GetStream();
                Byte[] bytes1 = new Byte[256];
                String data1 = null;
                int j = stream.Read(bytes1, 0, bytes1.Length);
                data1 = System.Text.Encoding.ASCII.GetString(bytes1, 0, bytes1.Length);
                if(data1.Equals("hello"))
                    Console.WriteLine("Zainicjowano stream pomyslnie");
                else
                    Console.WriteLine("Blad inicjacji streama");

                    

                //
                update(address[0], Convert.ToInt32(address[1]), streamTemp);
                Console.WriteLine("Router is online: {0}:{1} ", address[0], address[1]);
                stream.Close();
                client.Close();
            }
        }
       

        class Wire
        {
            String _addressOne, _addressTwo;
            public Boolean _isOnOne, _isOnTwo, _isOn;
            Int32 _portOne, _portTwo;
            TcpClient _first, _second;
            //
            
            public NetworkStream First { get; set; }
            public NetworkStream Second { get; set; }
            //

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

            public Wire(String addressOne, Int32 portOne, String addressTwo, Int32 portTwo)
            {
                _addressOne = addressOne;
                _addressTwo = addressTwo;
                _portOne = portOne;
                _portTwo = portTwo;
                _isOnOne = false;
                _isOnTwo = false;
            }

            public Thread StartTheThread(NetworkStream start, NetworkStream stop)
            {
                var t = new Thread(() => Run(start, stop));
                t.Start();
                return t;
            }

            public void Run(NetworkStream start, NetworkStream stop)
            {
                //NetworkStream nsStart = start.GetStream();
                //NetworkStream nsStop = start.GetStream();
                Byte[] bytes = new Byte[256];
                String data = null;
                int i;
                while ((i = start.Read(bytes, 0, bytes.Length)) != 0)
                {
                    Console.WriteLine("idzie idzie"); // no wlasnie nie idzie 
                    data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                    byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);
                    stop.Write(msg, 0, msg.Length);
                }                
            }

            internal void start()
            {
                //_first = new TcpClient();
                //_first.Connect(_addressOne, _portOne);
                //_second = new TcpClient();
                //_second.Connect(_addressTwo, _portTwo);
                StartTheThread(First, Second);
                StartTheThread(Second, First);
                _isOn = true;
            }
        }

        public string LocalIPAddress()
        {
            IPHostEntry host;
            string localIP = "";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }
    }
}
