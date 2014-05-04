using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
//bla
namespace ExtSrc
{
    public class Wire
    {
        IPEndPoint _one, _two;
       public int ID;

        public IPEndPoint One
        {
            get { return _one; }
        }

        public IPEndPoint Two
        {
            get { return _two; }
        }

        public Wire(IPEndPoint first, IPEndPoint second, int id)
        {
            _one = first;
            _two = second;
            ID = id;
        }
    }
}
