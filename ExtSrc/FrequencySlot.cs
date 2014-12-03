using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtSrc
{
    public class FrequencySlot
    {
        public int ID;
        public List<FrequencySlotUnit> FSUList { get; set; }
        public int startingFreq { get; set; }

        Modulation modulation;

        public FrequencySlot(int id, Modulation mod, int startFre)
        {
            ID = id;
            modulation = mod;
            FSUList = new List<FrequencySlotUnit>();
            startingFreq = startFre;
        }

    }

    public enum Modulation { NULL = 0, QPSK = 1, SixteenQAM = 2 };
}
