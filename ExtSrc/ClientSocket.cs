using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ExtSrc
{
    public class ClientSocket
    {
        public Socket socket {get; set;}
        public int ID { get; set; }
        public ClientSocket(int ID)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.ID = ID;
        }

        public ClientSocket(int ID, Socket s)
        {
            socket = s;
            this.ID = ID;
        }
    }
}
