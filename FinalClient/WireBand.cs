using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalClient
{
    class WireBand
    {
        int wireID;
        int lambdaCapactity;
        Boolean[] lambdas;
        int distance;
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

