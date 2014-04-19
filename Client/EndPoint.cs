using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class EndPoint
    {       
        public Int32 sendingPort { get; set; }
        public Int32 receivingPort { get; set; }

        public EndPoint() { }
        public EndPoint(Int32 sending, Int32 receiving)
        {
            sendingPort = sending;
            receivingPort = receiving;
        }
    }
}
//ttufgudsgfhdashd