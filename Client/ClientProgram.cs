using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class ClientProgram
    {
        static void Main(string[] args)
        {
           /**Console.WriteLine("Put receiving port (1 or 3).");
            Int32 a = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("Put sending port (2 or 4).");
            Int32 b = Convert.ToInt32(Console.ReadLine());
            Client cl = new Client();
            cl.connect(new EndPoint(b, a));
            int i = 0;
            while (i != 3)
            {
                Console.WriteLine("Push 'Enter' to type msg.");
                Console.ReadLine();
                cl.send(Console.ReadLine());
                i++;
            }**/
            Boolean withbreak = false;
            Client cl1 = new Client();
            Client cl2 = new Client();
            cl1.connect(new EndPoint(2,1));
            cl2.connect(new EndPoint(4,3));
            cl1.send("[1->2](1)");
            if(withbreak)Thread.Sleep(1000);
            cl1.send("[1->2](2)");
            if (withbreak) Thread.Sleep(1000);
            cl2.send("[2->1](1)");
            if (withbreak) Thread.Sleep(1000);
            cl2.send("[2->1](2)");
            if (withbreak) Thread.Sleep(1000);
            cl1.send("[1->2](3)");
            if (withbreak) Thread.Sleep(1000);
            cl1.send("[1->2](4)");
            if (withbreak) Thread.Sleep(1000);
            cl2.send("[2->1](3)");
            if (withbreak) Thread.Sleep(1000);
            cl2.send("[2->1](4)");
            if (withbreak) Thread.Sleep(1000);
            cl1.send("x");
            cl2.send("x");
            cl1.send("x");
            cl2.send("x");
            cl1.send("x");
            cl2.send("x");
            cl1.send("x");
            cl2.send("x");
            cl1.send("x");
            cl2.send("x");
            cl1.send("x");
            cl2.send("x");
            cl1.send("x");
            cl2.send("x");
            cl1.send("x");
            cl2.send("x");
            cl1.send("x");
            cl2.send("x");
            Console.ReadLine();

            //bbb

        }
    }
}
