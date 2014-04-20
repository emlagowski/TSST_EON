using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class ClientProgram
    {
        static void Main(string[] args)
        {
           /* Console.WriteLine("Put receiving port (1 or 3).");
            Int32 a = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("Put sending port (2 or 4).");
            Int32 b = Convert.ToInt32(Console.ReadLine());
            Client cl = new Client();
            cl.connect(new EndPoint(b, a));
            Console.WriteLine("Push 'Enter' to connect.");
            Console.ReadLine();
            cl.send("tesssstab");*/
            Client cl1 = new Client();
            Client cl2 = new Client();
            cl1.connect(new EndPoint(2,1));
            cl2.connect(new EndPoint(4,3));
            cl1.send("Pierwszy do drugiego");
            Console.ReadLine();

            //bbb

        }
    }
}
