using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ExtSrc
{
    public class ClientConnectionsTable
    {
        //dict <int[]{wireID,FS_ID}, ClientID>
        public Dictionary<int[], int> clientConnectionTable { get; set; }

        public ClientConnectionsTable()
        {
            clientConnectionTable = new Dictionary<int[], int>(new MyEqualityComparer());
        }

        //public void add(int wire, int FSid, Socket socket)
        //{
        //    clientConnectionTable.Add(new int[] { wire, FSid}, socket);
        //}

        public void add(int wire, int FSid, int socketID)
        {
            clientConnectionTable.Add(new int[] { wire, FSid }, socketID);
        }

        //public int[] findRoute(Socket socket)
        //{
        //    int[] result;
        //    foreach (Socket s in clientConnectionTable.Values)
        //    {
        //        if (socket.LocalEndPoint == s.LocalEndPoint && socket.RemoteEndPoint == s.RemoteEndPoint)
        //        {
        //            result = clientConnectionTable.FirstOrDefault(x => (x.Value.LocalEndPoint == socket.LocalEndPoint &&
        //                                                                (x.Value.RemoteEndPoint == socket.RemoteEndPoint))).Key;
        //        }
        //    }
        //    return null;
        //}
        //public int findOriginatingClientID(int wire, int FSid)
        //{
        //    int result;
        //    if (clientConnectionTable.TryGetValue(new int[] { wire, FSid }, out result))
        //        return result;
        //    else
        //        return result;
        //}
        public int[] findRoute(int socketID)
        {
            int[] result;
            foreach (int id in clientConnectionTable.Values)
            {
                if (id == socketID)
                {
                    result = clientConnectionTable.FirstOrDefault(x => (x.Value == id)).Key;
                    //tu nie bylo returna
                    return result;
                }
            }
            return null;
        }



        public int findClient(int wire, int FSid)
        {
            int result;
            if (clientConnectionTable.TryGetValue(new int[] { wire, FSid }, out result))
                return result;
            else
                return result;
        }
    }


    }
