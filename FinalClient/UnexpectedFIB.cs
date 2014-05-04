using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalClient
{
    class UnexpectedFIB
    {
        List<AddressPair> addressList;

        public UnexpectedFIB()
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
                }
            }
            return result;
        }

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
