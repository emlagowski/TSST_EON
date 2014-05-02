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
        public WireBand(int id, int capacity)
        {
            wireID = id;
            lambdaCapactity = capacity;
            lambdas = new Boolean[capacity];
            for (int i = 0; i < lambdas.Length; i++)
            {
                lambdas[i] = true;
            }
        }
    }
}
