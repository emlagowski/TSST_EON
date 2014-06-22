using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cloud
{
    class Program
    {
        static void Main(string[] args)
        {
            Cloud server = new Cloud("127.0.0.1", 8000);
            int[] lambdasOut_c1 = { 1, 2, 3, 4, 5 };

            int[] lambdasIn_c2 = { 1, 2, 3, 4, 5 };
            //int[] lambdas_c2 = { 1, 2, 3, 4, 5 };




            server.signaling.addConnection(new Connection(lambdasOut_c1, null, 5, 172), 1);
            server.signaling.addConnection(new Connection(null, lambdasOut_c1, 5, 172), 1);
            Application.Run(new Form1(server));


           

            Console.ReadLine();
        }
    }
}
