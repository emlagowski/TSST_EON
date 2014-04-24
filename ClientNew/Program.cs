using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
            
            Thread t1 = new Thread(c1.ReceiveData);
            t1.Start();
            Thread t2 = new Thread(c2.ReceiveData);
            t2.Start();
            Thread t3 = new Thread(c3.ReceiveData);
            t3.Start();
            Thread t4 = new Thread(c4.ReceiveData);
            t4.Start();
            Thread t5 = new Thread(c5.ReceiveData);
            t5.Start();
            Thread t6 = new Thread(c6.ReceiveData);
            t6.Start();

            c1.SendData("test");
            Console.ReadLine();
        }
    }
}
