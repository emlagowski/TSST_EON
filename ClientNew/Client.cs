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
        NetworkStream Stream;

        public Client(String address, int port)
        {
            _port = port;
            _address = address;
           // _listener = new TcpListener(IPAddress.Parse(_address), _port);
           // _listener.Start();
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
            //
            Byte[] ok = new Byte[3];
            Byte[] okTest = {1,0,1};
            ns.Read(ok, 0, ok.Length);
            if (ok != okTest) return;
            TcpClient Clnt = new TcpClient(_address, _port);
            Stream = Clnt.GetStream();
            byte[] sendMsg1= System.Text.Encoding.ASCII.GetBytes("hello");
            Stream.Write(sendMsg1, 0, sendMsg1.Length);

            //
            while (true)
            {
                //Console.WriteLine("{0}:{1} - Waiting ...", _address, _port);
               // _client = _listener.AcceptTcpClient();
               // NetworkStream stream = _client.GetStream();
                Byte[] bytes = new Byte[256];
                String data = null;
                int i;
                while ((i = Stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    data = System.Text.Encoding.ASCII.GetString(bytes, 0, bytes.Length);
                    String[] d = data.Split('\0');
                    Console.WriteLine("{0}:{1} - Received - {2}", _address, _port, d[0]);
                }
                //stream.Close();
            }
        }

        public void send(String address, Int32 port, String msg)
        {
            TcpClient tmp = new TcpClient(address, port); // tu chyba trzeba dawac namiary na chmure, a nie na cel.
            NetworkStream ns = tmp.GetStream();
            byte[] buffor = System.Text.Encoding.ASCII.GetBytes(msg);
            ns.Write(buffor, 0, buffor.Length);
            ns.Close();
            tmp.Close();
            Console.WriteLine("Sent!");
        }
    }
}
