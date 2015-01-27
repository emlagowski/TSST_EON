#region

using System;

#endregion

namespace ExtSrc
{
   public class DataAndID
    {
        public Data data { get; set; }
        public int ID { get; set; }
        public String uniqueKey { get; set; }

        public DataAndID(Data d, int id)
        {
            data = d;
            ID = id;
        }

        public DataAndID(Data d, int id, String key)
        {
            data = d;
            ID = id;
            uniqueKey = key;
        }
    }
}
