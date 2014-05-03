using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalSource
{
    [Serializable()]
    public class Data
    {
        public String EndAddress { get; set; }
        public int bandwidthNeeded { get; set; }
        public String info { get; set; }
        public int connectionID { get; set; }
        public Data(String address, int band, String inf, int cID)
        {
            EndAddress = address;
            bandwidthNeeded = band;
            info = inf;
            connectionID = cID;
        }
        public override string ToString()
        {
            String result = String.Format("EndAddress: {0} Bandwidth Needed: {1} Connection ID: {2} Message: {3}", EndAddress, bandwidthNeeded, connectionID, info);
            return result;
        }
    }
}
