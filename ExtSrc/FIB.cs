using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtSrc
{   [Serializable()]
    public class FIB
    {
        public List<AddressPair> addressList { get; set; }

        public FIB()
        {
            addressList = new List<AddressPair>();
        }

        public void addNext(String one, String two)
        {
            addressList.Add(new AddressPair(one, two));
        }


        public String findTarget(String ip)
        {
            String result = "";
            for (int i = 0; i < addressList.Count; i++)
            {
                if (addressList[i].TerminationPoint == ip)
                {
                    result = addressList[i].NextHop;
                    return result;
                }
            }
            return result;
        }
    [Serializable()]
        public class AddressPair
        {
        public String TerminationPoint { get; set; }
        public String NextHop { get; set; }

            public AddressPair(String one, String two)
            {
                TerminationPoint = one;
                NextHop = two;
            }
        }
    }
}
