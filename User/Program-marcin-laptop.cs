using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            String arg = args[0];
            Client user = new Client(arg);
//            Thread.Sleep(2000);
            if (arg.Equals("127.0.0.7"))
            {
                user.connect("127.0.1.4");
                Thread.Sleep(10000);
                user.Send(1, "client7to5", "127.0.0.5");
            }
            else if (arg.Equals("127.0.0.5"))
            {
                user.connect("127.0.1.1");
                Thread.Sleep(5000);
                user.Send(1, "client5to7", "127.0.0.7");
            }
            else if (arg.Equals("127.0.0.4"))
            {
                user.connect("127.0.1.1");
                Thread.Sleep(15000);
                user.Send(1, "client4to8", "127.0.0.8");
            }
            else if (arg.Equals("127.0.0.8"))
            {
                user.connect("127.0.1.3");
                Thread.Sleep(20000);
                user.Send(1, "client8to4", "127.0.0.4");
            }
            //Thread t = new Thread(delegate()
            //{
            //    ClientForm uf = new ClientForm(user);
            //    uf.Show();
            //    Application.Run();
            //});

            //t.Start();
            

            /*Client user = new Client("127.0.0.5");
            Thread t = new Thread(delegate()
            {
                ClientForm uf = new ClientForm(user);
                uf.Show();
                Application.Run();
            });
            t.Start();

            Client user2 = new Client("127.0.0.7");
            Thread t2 = new Thread(delegate()
            {
                ClientForm uf = new ClientForm(user2);
                uf.Show();
                Application.Run();
            });
            t2.Start();*/
        }
    }
}
