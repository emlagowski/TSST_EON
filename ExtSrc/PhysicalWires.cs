using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ExtSrc
{   [Serializable()]
    public class PhysicalWires
    {
        ArrayList _wires;

        public ArrayList Wires
        {
            get { return _wires; }
        }

        public PhysicalWires()
        {
            _wires = new ArrayList();
        }

        public void add(Wire w){
            _wires.Add(w);
        }
    }
}
