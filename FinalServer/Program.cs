using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FinalServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server("127.0.0.1", 8000);
            Application.Run(new Form1(server));


            int[] lambdasOut_c1 = { 1, 2, 3, 4, 5 };

            int[] lambdasIn_c2 = { 1, 2, 3, 4, 5 };
            //int[] lambdas_c2 = { 1, 2, 3, 4, 5 };




            server.signaling.addConnection(new Connection(lambdasOut_c1, null, 5, 172), 1);
            server.signaling.addConnection(new Connection(null, lambdasOut_c1, 6, 235), 1);

            Console.ReadLine();
        }
    }
}
