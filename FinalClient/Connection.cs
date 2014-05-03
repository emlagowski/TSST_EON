using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FinalClient
{
    [Serializable()]
    public class Connection
    {
        IPEndPoint EPIn, EPOut;
        [NonSerialized()]
        int[] InLambdaIDs;
        int[] OutLambdaIDs;
        int InWireID, OutWireID, Bandwidth;


        public Connection(IPEndPoint inEP, IPEndPoint outEP, int inWire, int outWire, int[] inLambdas, int[] outLambdas, int band)
        {
            EPIn = inEP;
            EPOut = outEP;
            InWireID = inWire;
            OutWireID = outWire;
            InLambdaIDs = inLambdas;
            OutLambdaIDs = outLambdas;
            Bandwidth = band;

        }
        public override String ToString()
        {
            String result = String.Format("EPIn: {0} EPOut: {1} InWireID: {2} OutWireID: {3} Bandwidth: {4}",
                EPIn, EPOut, InWireID, OutWireID, Bandwidth);
            return result;
        }
    }
}

