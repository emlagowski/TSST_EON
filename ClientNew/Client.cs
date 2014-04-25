using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;

namespace ClientNew
{
    class Client
    {
        ArrayList wires;
        string _address;
        Boolean _isOn;
        String [,] FIB;

        public Client(String address, int port)
        {
            wires = new ArrayList();
            _address = address;
            _isOn = false;
            Thread t = new Thread(Run);
            t.Start();
        }

        public void SendData(String address, string dataTosend)
        {
            while (FIB == null) Thread.Sleep(100);
            Int32 portIn = findSendingPort(address);
            if (portIn == -1)
            {
                Console.WriteLine("SendData method - Address not found.");
                return;
            }
            TcpClient client = new TcpClient(_address, portIn);
            Console.WriteLine("Send Data from {0} to {1}", _address, address, dataTosend);
            if (string.IsNullOrEmpty(dataTosend))
                return;
            NetworkStream serverStream = client.GetStream();
            byte[] outStream = System.Text.Encoding.ASCII.GetBytes(dataTosend);
            serverStream.Write(outStream, 0, outStream.Length);
            serverStream.Flush();
        }

        private Int32 findSendingPort(string address)
        {
            Int32 result = -1;
            for (int i = 0; i < FIB.GetLength(0); i++)
            {
                if (address.Equals(FIB[i,0]))
                {
                    result =  Convert.ToInt32(FIB[i, 1]);
                }
            }
            return result;
        }

        public void ReceiveData(String address, Int32 portOut)
        {
            TcpClient client = null;
            while (client == null)
            {
                try
                {
                    client = new TcpClient(_address, portOut);
                }
                catch(Exception e ) { }
            }
            while (true)
            {
                StringBuilder message = new StringBuilder();
                NetworkStream serverStream = client.GetStream();
                serverStream.ReadTimeout = 100;
                //the loop should continue until no dataavailable to read and message string is filled.
                //if data is not available and message is empty then the loop should continue, until
                //data is available and message is filled.
                while (true)
                {
                    if (serverStream.DataAvailable)
                    {
                        int read = serverStream.ReadByte();
                        if (read > 0)
                            message.Append((char)read);
                        else
                            break;
                    }
                    else if (message.ToString().Length > 0)
                        break;
                }
                Console.WriteLine("Received Data at {0} from {1} - {2}", _address, address, message.ToString());
            }
            //return message.ToString();
        }

        public void Run()
        {
            TcpClient tmp = new TcpClient("127.0.0.1", 2222);
            NetworkStream ns = tmp.GetStream();
            byte[] sendMsg = System.Text.Encoding.ASCII.GetBytes(_address);
            ns.Write(sendMsg, 0, sendMsg.Length);
            byte[] response = new byte[256];
            ns.Read(response, 0, response.Length);
            String s = System.Text.Encoding.ASCII.GetString(response, 0, response.Length);
            Parse(s);
            Console.WriteLine("{0} - Init sent to cloud.", _address);
            

            //RECEIVE DATA
            for (int i = 0; i < FIB.Length; i++)
            {
                ReceiveData(FIB[i,0], Convert.ToInt32(FIB[i,2]));
            }

        }

        private void Parse(string s)
        {
            String[] a1 = s.Split('|');
            FIB = new String[(a1.Length-1),3];
            for(int i=0; i< a1.Length-1; i++)
            {
                String[] tmp = a1[i].Split(':');
                FIB[i, 0] = tmp[0];
                FIB[i, 1] = tmp[1];
                FIB[i, 2] = tmp[2];
            }
        }
    }
}
