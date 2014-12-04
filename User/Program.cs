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
            var user = new Client(args[0]);

            new Thread(delegate()
            {
                var uf = new ClientForm(user);
                uf.Show();
                user.initialization();
                Application.Run();
            }).Start();

        }
    }
}
