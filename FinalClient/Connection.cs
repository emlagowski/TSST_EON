using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FinalClient
{
    class Connection
    {
        IPEndPoint EPIn, EPOut;
        int InWireID, OutWireID, InLambdaID, OutLambdaID;
        public Connection(IPEndPoint inEP, IPEndPoint outEP, int inWire, int outWire, int inLambda, int outLambda)
        {
            EPIn = inEP;
            EPOut = outEP;
            InWireID = inWire;
            OutWireID = outWire;
            InLambdaID = inLambda;
            OutLambdaID = outLambda;
        }
    }
}
