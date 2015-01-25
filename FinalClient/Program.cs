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
            // RouterIp, isEdge?, AgentIp, DomainConnectIp(optional)
            var ip = args[0];
            var edgeB = false;
            String domainAddress = null;
            String agentAddress = null;
            try
            {
                var edge = args[1];
                if(edge.Equals("true"))
                    edgeB = true;
                agentAddress = args[2];
                domainAddress = args[3];
            }
            catch
            {
                // Ignore Exception, not edge router.
            }

            var clientZero = new Node(ip, edgeB, agentAddress);

            new Thread(() =>
            {
                var cf = new RouterForm(clientZero);
                clientZero.RouterForm = cf;
                cf.Show();
                clientZero.ConnectAndRun();
                Application.Run();
            }).Start();

            new Thread(() =>
            {
                if (domainAddress != null)
                    clientZero.ConnectDomain(domainAddress);
            }).Start();
        }
    }
}
