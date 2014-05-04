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
            List<String> routers = new List<string>();
            Client.readFIB();
            Client clientOne = new Client("127.0.0.10");
            routers.Add("127.0.0.10");
            Thread t = new Thread(delegate()
                {
                    ClientForm cf = new ClientForm(clientOne);
                    cf.Show();
                    Application.Run();
                });
            t.Start();
            Client clientTwo = new Client("127.0.0.20");
            routers.Add("127.0.0.20");

            Thread t2 = new Thread(delegate()
            {
                ClientForm cf = new ClientForm(clientTwo);
                cf.Show();
                Application.Run();
            });
            t2.Start();
            Client clientThree = new Client("127.0.0.30");
            routers.Add("127.0.0.30");

            Thread t3 = new Thread(delegate()
            {
                ClientForm cf = new ClientForm(clientThree);
                cf.Show();
                Application.Run();
            });
            t3.Start();
            Client clientFour = new Client("127.0.0.40");
            routers.Add("127.0.0.40");

            Thread t4 = new Thread(delegate()
            {
                ClientForm cf = new ClientForm(clientFour);
                cf.Show();
                Application.Run();
            });
            t4.Start();
            Client clientFive = new Client("127.0.0.50");
            routers.Add("127.0.0.50");

            Thread t5 = new Thread(delegate()
            {
                ClientForm cf = new ClientForm(clientFive);
                cf.Show();
                Application.Run();
            });
            t5.Start();

            ExtSrc.AgentData.routers = routers;


            int[] lambdasOut_c1 = {1,2,3,4,5 };

            int[] lambdasIn_c2 = { 1, 2, 3, 4, 5 };
            //int[] lambdas_c2 = { 1, 2, 3, 4, 5 };

            
            
           
            clientOne.signaling.addConnection(new Connection(null, lambdasOut_c1, 5, 172), 1);
            clientTwo.signaling.addConnection(new Connection(lambdasOut_c1, null, 6, 235), 1);

          //  new Connection(2, 987);
           // new Connection(1, 543);

            ExtSrc.Data d1 = new ExtSrc.Data("127.0.0.40", 5, "testOne<EOF>", 172);
            //Data d2 = new Data("127.0.0.30", 6, "testTwo<EOF>", 235);
            //Data d3 = new Data("127.0.0.10", 2, "testThree<EOF>", 987);
           // ExtSrc.Data d4 = new ExtSrc.Data("127.0.0.20", 1, "testFour<EOF>", 543);

            clientOne.Send(d1);
            Thread.Sleep(1000);
          //  clientOne.Send(d2, "127.0.0.30");
          //  Thread.Sleep(1000);
          //  clientTwo.Send(d3, "127.0.0.10");
          //  Thread.Sleep(1000);
             // clientFive.Send(d4, "127.0.0.40");



            Console.ReadLine();
        }
    }
}
