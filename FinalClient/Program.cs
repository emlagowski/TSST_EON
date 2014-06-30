using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Router
{
    public class Program
    {
        public static void Main(string[] args)
        {
            
            String ip = args[0];
            Boolean edge_b = false;
            try
            {
                String edge = args[1];
                if(edge.Equals("true"))
                    edge_b = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("NOT EDGE ROUTER");
            }

            Router clientZero;

            if (edge_b == false)
            {
                Console.WriteLine("THIS IS ROUTER :" + ip);

                clientZero = new Router(ip, false);

            }
            else
            {
                Console.WriteLine("THIS IS EDGE ROUTER :" + ip);

                clientZero = new Router(ip, true);

            }
            //Thread t0 = new Thread(delegate()
            //{
            //    RouterForm cf = new RouterForm(clientZero);
            //    cf.Show();
            //    Application.Run();
            //});
            //t0.Start();

            //Thread.Sleep(8000);
            //if (ip.Equals("127.0.1.1"))
            //{
            //    Console.WriteLine("WYSYLAMY");
            //    clientZero.Send(new ExtSrc.Data(5, "elo"), new int[] { 1, 1 });
            //}

            //Router clientOne = new Router("127.0.0.10");
          //  routers.Add("127.0.0.10");
            
           // Router clientTwo = new Router("127.0.0.20");
           // routers.Add("127.0.0.20");

           
          // Router clientThree = new Router("127.0.0.30");
         //  routers.Add("127.0.0.30");

            
          // Router clientFour = new Router("127.0.0.40");
           // routers.Add("127.0.0.40");

            
           // Router clientFive = new Router("127.0.0.50");
          //  routers.Add("127.0.0.50");

           

            //ExtSrc.AgentData.routers = routers;


//            int[] lambdasOut_c1 = {1,2,3,4,5 };

          //  int[] lambdasIn_c2 = { 1, 2, 3, 4, 5 };
            //int[] lambdas_c2 = { 1, 2, 3, 4, 5 };

            
            
           
 //          clientOne.signaling.addConnection(new ExtSrc.Connection(null, lambdasOut_c1, 2, 2, 5, 172));
  //         clientThree.signaling.addConnection(new ExtSrc.Connection(null, lambdasOut_c1, 5, 5, 5, 172));


           // clientThree.signaling.addConnection(new ExtSrc.Connection(lambdasOut_c1, null, 5, 5, 172), 5);

          //  new Connection(2, 987);
           // new Connection(1, 543);

           // ExtSrc.Data d1 = new ExtSrc.Data("127.0.0.40", 5, "testOne<EOF>", 172);
            //Data d2 = new Data("127.0.0.30", 6, "testTwo<EOF>", 235);
            //Data d3 = new Data("127.0.0.10", 2, "testThree<EOF>", 987);
           // ExtSrc.Data d4 = new ExtSrc.Data("127.0.0.20", 1, "testFour<EOF>", 543);

            //clientOne.Send(d1);
           // Thread.Sleep(1000);
          //  clientOne.Send(d2, "127.0.0.30");
          //  Thread.Sleep(1000);
          //  clientTwo.Send(d3, "127.0.0.10");
          //  Thread.Sleep(1000);
             // clientFive.Send(d4, "127.0.0.40");
            /**clientOne.agentCom.Send(AgentCommunication.socket, new ExtSrc.AgentData(clientOne.address, Client.fib, clientOne.unFib, clientOne.signaling.AvBaIN, clientOne.signaling.AvBaOUT, clientOne.signaling.Conn));
            Thread.Sleep(200);
            clientTwo.agentCom.Send(AgentCommunication.socket, new ExtSrc.AgentData(clientTwo.address, Client.fib, clientTwo.unFib, clientTwo.signaling.AvBaIN, clientTwo.signaling.AvBaOUT, clientTwo.signaling.Conn));
            Thread.Sleep(200);
            clientThree.agentCom.Send(AgentCommunication.socket, new ExtSrc.AgentData(clientThree.address, Client.fib, clientThree.unFib, clientThree.signaling.AvBaIN, clientThree.signaling.AvBaOUT, clientThree.signaling.Conn));
            Thread.Sleep(200);
            clientFour.agentCom.Send(AgentCommunication.socket, new ExtSrc.AgentData(clientFour.address, Client.fib, clientFour.unFib, clientFour.signaling.AvBaIN, clientFour.signaling.AvBaOUT, clientFour.signaling.Conn));
            Thread.Sleep(200);
            clientFive.agentCom.Send(AgentCommunication.socket, new ExtSrc.AgentData(clientFive.address, Client.fib, clientFive.unFib, clientFive.signaling.AvBaIN, clientFive.signaling.AvBaOUT, clientFive.signaling.Conn));
            **/



            
         /* Thread t = new Thread(delegate()
                {
                    RouterForm cf = new RouterForm(clientOne);
                    cf.Show();
                    Application.Run();
                });
            t.Start();

            Thread t2 = new Thread(delegate()
            {
                RouterForm cf = new RouterForm(clientTwo);
                cf.Show();
                Application.Run();
            });
            t2.Start();

            Thread t3 = new Thread(delegate()
            {
                RouterForm cf = new RouterForm(clientThree);
                cf.Show();
                Application.Run();
            });
            t3.Start();
            Thread t4 = new Thread(delegate()
            {
                RouterForm cf = new RouterForm(clientFour);
                cf.Show();
                Application.Run();
            });
            t4.Start();
            Thread t5 = new Thread(delegate()
            {
                RouterForm cf = new RouterForm(clientFive);
                cf.Show();
                Application.Run();
            });
            t5.Start();*/
   
            

            Console.ReadLine();
        }
    }
}
