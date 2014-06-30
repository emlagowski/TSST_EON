using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtSrc
{
    [Serializable()]
    public class DijkstraData
    {
        public int routerID { get; set; }
        public int wireID { get; set; }
        public int wireDistance {get; set; }

        public DijkstraData(int routerID, int wireID, int wireDistance)
        {
            this.routerID = routerID;
            this.wireID = wireID;
            this.wireDistance = wireDistance;
        }


    }
    //porownuje tylko wireID i wireDistance
    public class DijkstraEqualityComparer : IEqualityComparer<DijkstraData>
    {
        public bool Equals(DijkstraData x, DijkstraData y)
        {
            
            if (x.wireDistance != y.wireDistance || x.wireID != y.wireID)
            {
                return false;
            }
            
            return true;
        }

        public int GetHashCode(DijkstraData obj)
        {
            int result = 19;
            for (int i = 0; i < 10; i++)
            {
                unchecked
                {
                    result = (int)(result * 23 + 0.8*obj.wireID + 0.44*obj.wireDistance);
                }
            }
            return result;
        }
    }
}
