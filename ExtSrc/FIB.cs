using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtSrc
{   [Serializable()]
    public class FIB
    {
        List<AddressPair> addressList;

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
                if (addressList[i].addresOne == ip)
                {
                    result = addressList[i].addressTwo;
                    return result;
                }
            }
            return result;
        }
    [Serializable()]
        class AddressPair
        {
            public String addresOne, addressTwo;

            public AddressPair(String one, String two)
            {
                addresOne = one;
                addressTwo = two;
            }
        }
    }
}
