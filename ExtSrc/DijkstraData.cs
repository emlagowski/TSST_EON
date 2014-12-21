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
        public int[] RouterIds { get; set; }
        public DijkstraData(int routerID, int wireID, int wireDistance, int[] routerIds)
        {
            this.routerID = routerID;
            this.wireID = wireID;
            this.wireDistance = wireDistance;
            this.RouterIds = routerIds;
        }


    }
    //porownuje tylko wireID i wireDistance
    public class DijkstraEqualityComparer : IEqualityComparer<DijkstraData>
    {
        public bool Equals(DijkstraData x, DijkstraData y)
        {
            return x.wireDistance == y.wireDistance && x.wireID == y.wireID;
        }

        public int GetHashCode(DijkstraData obj)
        {
            var result = 19;
            for (var i = 0; i < 10; i++)
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
