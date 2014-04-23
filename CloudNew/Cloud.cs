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
        ArrayList wires;
        Thread thread;
        TcpListener listener;
        Boolean isOn;
        public Int32 port { get; set; }  
        public IPAddress localIP { get; set; }

        public Cloud(Int32 p){
            localIP = IPAddress.Parse(LocalIPAddress());
            port = p;
            listener = new TcpListener(localIP, port);
            thread = new Thread(Run);
            wires = new ArrayList();
            isOn = true;
            thread.Start();
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
                String[] a = tmp[0].Split('|');
                String[] address1 = a[0].Split(':');
                String[] address2 = a[1].Split(':');
                wires.Add(new Wire(address1[0], Convert.ToInt32(address1[1]), address2[0], Convert.ToInt32(address2[1])));
                Console.WriteLine("New wire is set: {0}:{1} <-> {2}:{3}", address1[0], address1[1], address2[0], address2[1]);
                stream.Close();
                client.Close();
            }
        }

        class Wire
        {
            String _addressOne, _addressTwo;
            Int32 _portOne, _portTwo;
            TcpClient _first, _second;

            public Wire(String addressOne, Int32 portOne, String addressTwo, Int32 portTwo)
            {
                _addressOne = addressOne;
                _addressTwo = addressTwo;
                _portOne = portOne;
                _portTwo = portTwo;
                _first = new TcpClient();
                _first.Connect(_addressOne, _portOne);
                _second = new TcpClient();
                _second.Connect(_addressTwo, _portTwo);
                Thread thread = new Thread(Run);
            }

            public void Run(){

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
