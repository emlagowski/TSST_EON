using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FinalClient
{
    public class Wire
    {
        IPEndPoint _one, _two;

        public IPEndPoint One
        {
            get { return _one; }
        }

        public IPEndPoint Two
        {
            get { return _two; }
        }

        public Wire(IPEndPoint first, IPEndPoint second)
        {
            _one = first;
            _two = second;
        }
    }
}
