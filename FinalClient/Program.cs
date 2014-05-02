using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FinalClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Client clientOne = new Client("127.0.0.10", 8010);
            Client clientTwo = new Client("127.0.0.20", 8020);
            Client clientThree = new Client("127.0.0.30", 8030);
            Client clientFour = new Client("127.0.0.40", 8040);
            Client clientFive = new Client("127.0.0.50", 8050);
            Client clientSix = new Client("127.0.0.60", 8060);


            clientOne.Send("testOne<EOF>");
            Thread.Sleep(1000);
            clientThree.Send("testTwo<EOF>");
            Thread.Sleep(1000);
            clientSix.Send("testThree<EOF>");
            Thread.Sleep(1000);
            clientTwo.Send("testFour<EOF>");
            Thread.Sleep(1000);
            clientOne.Send("testFive<EOF>");
            Thread.Sleep(1000);
            clientOne.Send("testSix<EOF>");
            Thread.Sleep(1000);
            clientOne.Send("testSeven<EOF>");
            Thread.Sleep(1000);
            clientOne.Send("testEight<EOF>");
            Thread.Sleep(1000);
            clientTwo.Send("testNine<EOF>");
            Thread.Sleep(1000);


            Console.ReadLine();
        }
    }
}
