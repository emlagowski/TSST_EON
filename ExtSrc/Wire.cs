using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
//bla
namespace ExtSrc
{
    [Serializable()]
    public class Wire
    {
        IPEndPoint _one, _two;
        public int ID;
        public int distance { get; set; }

        public IPEndPoint One
        {
            get { return _one; }
        }

        public IPEndPoint Two
        {
            get { return _two; }
        }

        public int IDD
        {
            get { return ID; }
        }

        public Wire(IPEndPoint first, IPEndPoint second, int id, int d)
        {
            _one = first;
            _two = second;
            ID = id;
            distance = d;
        }
    }
}
