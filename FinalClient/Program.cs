using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FinalClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Client.readFIB();
            Client clientOne = new Client("127.0.0.10");
            Thread t = new Thread(delegate()
                {
                    ClientForm cf = new ClientForm(clientOne);
                    cf.Show();
                    Application.Run();
                });
            t.Start();
            Client clientTwo = new Client("127.0.0.20");
            Thread t2 = new Thread(delegate()
            {
                ClientForm cf = new ClientForm(clientTwo);
                cf.Show();
                Application.Run();
            });
            t2.Start();
            Client clientThree = new Client("127.0.0.30");
            Thread t3 = new Thread(delegate()
            {
                ClientForm cf = new ClientForm(clientThree);
                cf.Show();
                Application.Run();
            });
            t3.Start();
            Client clientFour = new Client("127.0.0.40");
            Thread t4 = new Thread(delegate()
            {
                ClientForm cf = new ClientForm(clientFour);
                cf.Show();
                Application.Run();
            });
            t4.Start();
            Client clientFive = new Client("127.0.0.50");
            Thread t5 = new Thread(delegate()
            {
                ClientForm cf = new ClientForm(clientFive);
                cf.Show();
                Application.Run();
            });
            t5.Start();

            clientOne.Send("testOne<EOF>", "127.0.0.20");
            Thread.Sleep(1000);
            clientOne.Send("testTwo<EOF>", "127.0.0.30");
            Thread.Sleep(1000);
            clientTwo.Send("testThree<EOF>", "127.0.0.10");
            Thread.Sleep(1000);
            clientFive.Send("testFour<EOF>", "127.0.0.40");



            Console.ReadLine();
        }
    }
}
