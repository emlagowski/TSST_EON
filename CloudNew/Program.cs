using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace CloudNew
{
    class Program
    {
        static void Main(string[] args)
        {
            Cloud cloud = new Cloud(2222); // w programie klient jest ustawione sztywno ze chmura dziala na 2222, jct trze zmienic

            Console.ReadLine();
        }
    }
}
