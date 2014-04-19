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
            Client cl1 = new Client();
            Client cl2 = new Client();
            cl1.connect(1);
            cl2.connect(1);
            cl1.send("Pierwszy do drugiego");
            Console.ReadLine();



        }
    }
}
