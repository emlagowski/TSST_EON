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
        public const String A1 = "127.0.0.1:1:2|127.0.0.2:1:2";
        public const String A2 = "127.0.0.1:3:4|127.0.0.4:3:4";
        public const String A3 = "127.0.0.2:3:4|127.0.0.3:3:4";
        public const String A4 = "127.0.0.2:7:8|127.0.0.5:7:8";
        public const String A5 = "127.0.0.2:5:6|127.0.0.6:5:6";
        public const String A6 = "127.0.0.3:1:2|127.0.0.4:1:2";
        public const String A7 = "127.0.0.3:5:6|127.0.0.5:5:6";
        public const String A8 = "127.0.0.3:7:8|127.0.0.6:7:8";
        public const String A9 = "127.0.0.5:1:2|127.0.0.6:1:2";
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
                wires.Add(new Wire(address1[0], Convert.ToInt32(address1[1]), Convert.ToInt32(address1[2]), address2[0], Convert.ToInt32(address2[1]), Convert.ToInt32(address2[2])));
                Console.WriteLine("New wire is set: {0}:{1}:{2} <-> {3}:{4}:{5}", address1[0], address1[1], address1[2], address2[0], address2[1], address2[2]); 
            }
        }

        public String update(String address)
        {
            String result = null;
            for (int i = 0; i < wires.Count; i++)
            {
                Wire w = wires[i] as Wire;
                String[] addresses = w.Addresses;
                Int32[] ports = w.Ports;

                if (addresses[0].Equals(address))
                {
                    w._isOnOne = true;
                    String s = String.Format("{0}:{1}:{2}|", w.Addresses[1], w.Ports[2], w.Ports[3]);
                    result = String.Concat(result, s);
                }
                else if (addresses[1].Equals(address))
                {
                    w._isOnTwo = true;
                    String s = String.Format("{0}:{1}:{2}|", w.Addresses[0], w.Ports[0], w.Ports[1]);
                    result = String.Concat(result, s);
                }

                if (w._isOnOne && w._isOnTwo && !w._isOn)
                    w.start();
            }
            return result;
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
                String[] address = data.Split('\0');
                String msg = update(address[0]);
                byte[] sendMsg = System.Text.Encoding.ASCII.GetBytes(msg);
                stream.Write(sendMsg, 0, sendMsg.Length);
                Console.WriteLine("Router is online: {0}", address[0]);
                stream.Close();
                client.Close();
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
