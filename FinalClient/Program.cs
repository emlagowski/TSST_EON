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
            Client.readFIB();
            Client clientOne = new Client("127.0.0.10");
            Client clientTwo = new Client("127.0.0.20");
            Client clientThree = new Client("127.0.0.30");
            Client clientFour = new Client("127.0.0.40");
            Client clientFive = new Client("127.0.0.50");

            clientOne.Send("testOne<EOF>", "127.0.0.20");
            Thread.Sleep(1000);
            clientOne.Send("testOne<EOF>", "127.0.0.30");



            Console.ReadLine();
        }
    }
}
