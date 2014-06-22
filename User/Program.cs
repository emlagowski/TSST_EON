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
            Thread t = new Thread(delegate()
            {
                ClientForm uf = new ClientForm(user);
                uf.Show();
                Application.Run();
            });
            t.Start();

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
