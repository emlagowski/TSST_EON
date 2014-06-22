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
        public void remove(String one, String two)
    {
        //addressList.Remove(new AddressPair(one, two));
        foreach (AddressPair ap in addressList)
        {
            if (ap.TerminationPoint.Equals(one) && ap.NextHop.Equals(two))
                addressList.RemoveAt(addressList.IndexOf(ap));
        }
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
        public class AddressPair : IEquatable<AddressPair>
        {
        public String TerminationPoint { get; set; }
        public String NextHop { get; set; }

            public AddressPair(String one, String two)
            {
                TerminationPoint = one;
                NextHop = two;
            }
            public bool Equals(AddressPair other)
            {
                if (other == null) return false;
                return (this.TerminationPoint.Equals(other.TerminationPoint) && this.NextHop.Equals(other.NextHop) );
            }
        }
    }
}
