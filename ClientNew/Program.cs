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
            System.Threading.Thread.Sleep(5000);
            c1.send("127.0.0.2", 2, "pierwsza");
            c1.send("127.0.0.4", 4, "druga");
            c1.send("127.0.0.2", 2, "trzecia");
            c2.send("127.0.0.1", 1, "czwarta");
            c2.send("127.0.0.1", 1, "piata");
            c2.send("127.0.0.3", 3, "szosta");
            c2.send("127.0.0.6", 6, "siodma");
            c3.send("127.0.0.5", 5, "osma");
            c1.send("127.0.0.6", 6, "ta_nie_powinna_dojsc");
            Console.ReadLine();
        }
    }
}
