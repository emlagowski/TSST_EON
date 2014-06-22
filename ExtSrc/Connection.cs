using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ExtSrc
{
    [Serializable()]
    public class Connection : IEquatable<Connection>
    {
        IPEndPoint EPIn, EPOut;

        public int[] InLambdaIDs {get; set;}
        public int[] OutLambdaIDs { get; set; }
        public int InWireID { get; set; }//{ get { return InWireID; } set { this.InWireID = value; } }
        public int OutWireID { get; set; }//{ get { return OutWireID; } set { this.OutWireID = value; } }
        public int Bandwidth { get; set; }//{ get { return Bandwidth; } set { this.Bandwidth = value; } }
        public int connectionID { get; set; }//{ get { return connectionID; } set { this.connectionID = value; } } // musi byc jakas wiekksza losowa liczba zeby nie bylo ze dwie maja takie samo id

        public Connection(int[] inLambdas, int[] outLambdas,int wireID, int band, int cID) 
        {
            EPIn = null;
            EPOut = null;
            InWireID = -1;
            OutWireID = wireID;
            InLambdaIDs = inLambdas;
            OutLambdaIDs = outLambdas;
            Bandwidth = band;
            connectionID = cID;
        
        }
        ///Tego uzyjjjjjjjjjjjjjjjjj
        public Connection(int[] inLambdas, int[] outLambdas, int inWireID, int outWireID, int band, int cID)
        {
            EPIn = null;
            EPOut = null;
            InWireID = inWireID;
            OutWireID = outWireID;
            InLambdaIDs = inLambdas;
            OutLambdaIDs = outLambdas;
            Bandwidth = band;
            connectionID = cID;

        }
        public Connection(IPEndPoint inEP, IPEndPoint outEP, int inWire, int outWire, int[] inLambdas, int[] outLambdas, int band, int cID)
        {
            EPIn = inEP;
            EPOut = outEP;
            InWireID = inWire;
            OutWireID = outWire;
            InLambdaIDs = inLambdas;
            OutLambdaIDs = outLambdas;
            Bandwidth = band;
            connectionID = cID;

        }
        public int CID
        {
            get { return connectionID; }
            set { connectionID = value; }
        }
        public override String ToString()
        {
            String result = String.Format("EPIn: {0} EPOut: {1} InWireID: {2} OutWireID: {3} Bandwidth: {4} Connection ID: {5}",
                EPIn, EPOut, InWireID, OutWireID, Bandwidth, connectionID);
            return result;
        }
        public bool Equals(Connection other)
        {
            if (other == null) return false;
           // return (this.InWireID.Equals(other.InWireID) && this.OutWireID.Equals(other.OutWireID) && this.InLambdaIDs.Equals(other.InLambdaIDs)
            //    && this.OutLambdaIDs.Equals(other.OutLambdaIDs) && this.Bandwidth.Equals(other.Bandwidth) && this.connectionID.Equals(other.connectionID));
            return (this.connectionID.Equals(other.connectionID));
        }

    }
}

