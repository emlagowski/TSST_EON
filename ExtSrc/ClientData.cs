using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtSrc
{
     [Serializable()]
    public class ClientData
    {


        public String EndAddress { get; set; }
        public int bandwidthNeeded { get; set; }
        public String info { get; set; }
        
        public ClientData(int band, String inf, String endAddr)
        {
            info = inf;
            bandwidthNeeded = band;
            EndAddress = endAddr;
        }
        

        public override string ToString()
        {
            String result = String.Format("OriginatingAddress: {2} Bandwidth Needed: {0} Message: {1}",  bandwidthNeeded,  info, EndAddress);
            return result;
        }


    }
}
