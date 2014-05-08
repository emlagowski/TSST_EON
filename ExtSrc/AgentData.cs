using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtSrc
{
    [Serializable()]
    public class AgentData
    {
        public String routerAddress;
        public PhysicalWires physWires;
        public FIB unFib;
        public List<ExtSrc.WireBand> AvalaibleBandIN;
        public List<ExtSrc.WireBand> AvalaibleBandOUT;
        public List<ExtSrc.Connection> Connections { get; set; }
        public String message;


        public String address
        {
            get { return routerAddress; }
            set { routerAddress = value;  }
        }

        public PhysicalWires fibTable
        {
            get { return physWires; }
            set { physWires = value; }
        }

        public List<ExtSrc.WireBand> AbIN
        {
            get { return AvalaibleBandIN; }
        }

        public List<ExtSrc.WireBand> AbOUT
        {
            get { return AvalaibleBandOUT; }
        }

        public List<ExtSrc.Connection> Conn
        {
            get { return Connections; }
        }

        public FIB unFibTable
        {
            get { return unFib; }
            set { unFib = value; }
        }

        public AgentData(Connection conn)
        {
            this.routerAddress = null;
            this.physWires = null;
            this.unFib = null;
            this.AvalaibleBandIN = null;
            this.AvalaibleBandOUT = null;
            this.Connections = new List<ExtSrc.Connection>(){conn};
            message = null;
        }
        public AgentData(Connection conn, String msg)
        {
            this.routerAddress = null;
            this.physWires = null;
            this.unFib = null;
            this.AvalaibleBandIN = null;
            this.AvalaibleBandOUT = null;
            this.Connections = new List<ExtSrc.Connection>() { conn };
            message = msg;
        }
        public AgentData(ExtSrc.FIB fib, String msg)
        {
            this.routerAddress = null;
            this.physWires = null;
            this.unFib = fib;
            this.AvalaibleBandIN = null;
            this.AvalaibleBandOUT = null;
            this.Connections = null;
            message = msg;
        }

        public AgentData(String address, PhysicalWires fib, FIB ufib, List<ExtSrc.WireBand> AvalaibleBandIN, List<ExtSrc.WireBand> AvalaibleBandOUT, List<ExtSrc.Connection> Connections)
        {
            this.routerAddress = address;
            this.physWires = fib;
            this.unFib = ufib;
            this.AvalaibleBandIN = AvalaibleBandIN;
            this.AvalaibleBandOUT = AvalaibleBandOUT;
            this.Connections = Connections;
            message = null;

        }

        public ExtSrc.WireBand findWireIN(int wireID)
        {
            ExtSrc.WireBand result = null;
            AvalaibleBandIN.ForEach(delegate(ExtSrc.WireBand wb)
            {
                if (wb.wireID == wireID) result = wb;
            });
            return result;

        }
        public ExtSrc.WireBand findWireOut(int wireID)
        {
            ExtSrc.WireBand result = null;
            AvalaibleBandOUT.ForEach(delegate(ExtSrc.WireBand wb)
            {
                if (wb.wireID == wireID) result = wb;
            });
            return result;//CZYto zwraca mi referencje???????????

        }
        public void addConnection(ExtSrc.Connection c)
        {
            //NIE SPRAWDZA CZY JUZ SA ZAJETE LAMBDY 
            if (c.InLambdaIDs != null)
            {
                for (int i = 0; i < c.InLambdaIDs.Length; i++)
                    findWireIN(c.InWireID).lambdas[c.InLambdaIDs[i]] = false; // bedzie blad jak findwire zwroci null
            }
            if (c.OutLambdaIDs != null)
            {
                for (int i = 0; i < c.OutLambdaIDs.Length; i++)
                    findWireOut(c.OutWireID).lambdas[c.OutLambdaIDs[i]] = false;
            }
            Connections.Add(c); // nie sprawdza czy id sie roznia

        }
        public void removeConnection(ExtSrc.Connection c)
        {
            if (c.InLambdaIDs != null)
            {
                for (int i = 0; i < c.InLambdaIDs.Length; i++)
                    findWireIN(c.InWireID).lambdas[c.InLambdaIDs[i]] = true; // bedzie blad jak findwire zwroci null
            }
            if (c.OutLambdaIDs != null)
            {
                for (int i = 0; i < c.OutLambdaIDs.Length; i++)
                    findWireOut(c.OutWireID).lambdas[c.OutLambdaIDs[i]] = true;
            }
            Connections.Remove(c); // nie sprawdza czy id sie roznia
        }

    }
}
