using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SubnetworkController
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            var ip = args[0];

            var agent = new SubnetworkController(ip);

            new Thread(delegate()
            {
                var snf = new SubNetConForm(agent);
                agent.SubNetForm = snf;
                snf.Show();
                agent.ConnectAndRun();
                Application.Run();
            }).Start();

//            Application.EnableVisualStyles();
//            Application.SetCompatibleTextRenderingDefault(false);
//            Application.Run(new SubNetConForm());
        }
    }

}
