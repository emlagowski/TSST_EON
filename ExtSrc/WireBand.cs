using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtSrc
{
    [Serializable()]
    public class WireBand
    {
        public int wireID;
        public int lambdaCapactity;
        public Boolean[] lambdas;
        public int distance;

        public int wID { get { return wireID; } }

        public int lambCap { get { return lambdaCapactity; } }

        public Boolean[] lamb { get { return lambdas; } }

        public int dist { get { return distance; } }

        public WireBand(int id, int capacity, int distance)
        {
            wireID = id;
            lambdaCapactity = capacity;
            this.distance = distance;
            lambdas = new Boolean[capacity];
            for (int i = 0; i < lambdas.Length; i++)
            {
                lambdas[i] = true;
            }
        }
    }
}

