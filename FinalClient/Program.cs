using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Node
{
    public class Program
    {
        public static void Main(string[] args)
        {
            
            var ip = args[0];
            var edgeB = false;
            try
            {
                var edge = args[1];
                if(edge.Equals("true"))
                    edgeB = true;
            }
            catch
            {
                // Ignore Exception, not edge router.
            }

            var clientZero = new Node(ip, edgeB);

            new Thread(delegate()
            {
                var cf = new RouterForm(clientZero);
                clientZero.RouterForm = cf;
                cf.Show();
                clientZero.ConnectAndRun();
                Application.Run();
            }).Start();
        }
    }
}
