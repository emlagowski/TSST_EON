using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtSrc
{
   public class DataAndID
    {
        public Data data { get; set; }
        public int ID { get; set; }

        public DataAndID(Data d, int id)
        {
            data = d;
            ID = id;
        }
    }
}
