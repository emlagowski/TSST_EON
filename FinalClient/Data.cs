using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalClient
{
    public class Data
    {
        String EndAddress;
        int bandwidthNeeded;
        String info;

        public Data(String address, int band, String inf)
        {
            EndAddress = address;
            bandwidthNeeded = band;
            info = inf;
        }
    }
}//ff
