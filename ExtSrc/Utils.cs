using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtSrc
{
    public class Utils
    {
        public static int GetId(String ipAddress)
        {
            return Convert.ToInt32(ipAddress.Split('.').Last());
        }
    }
}
