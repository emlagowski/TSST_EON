using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientNew
{
    class Program
    {
        static void Main(string[] args)
        {
            Client c1 = new Client("127.0.0.1", 1);
            Client c2 = new Client("127.0.0.2", 2);
            Client c3 = new Client("127.0.0.3", 3);
            Client c4 = new Client("127.0.0.4", 4);
            Client c5 = new Client("127.0.0.5", 5);
            Client c6 = new Client("127.0.0.6", 6);



            Console.ReadLine();
        }
    }
}
