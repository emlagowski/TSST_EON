using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtSrc
{
    public class FrequencySlotSwitchingTable
    {
        // int[] { wireID, FSid}
        public Dictionary<int[], int[]> freqSlotSwitchingTable { get; set; }

        public FrequencySlotSwitchingTable()
        {
            freqSlotSwitchingTable = new Dictionary<int[], int[]>(new MyEqualityComparer());
        }

        public void add(int wireA, int FSidA, int wireB, int FSidB)
        {
            freqSlotSwitchingTable.Add(new int[] { wireA, FSidA }, new int[] { wireB, FSidB });
            freqSlotSwitchingTable.Add(new int[] { wireB, FSidB }, new int[] { wireA, FSidA });
        }

        public void remove(int wireA, int FSidA, int wireB, int FSidB)
        {
            //todo dobrze usuwa bo jest uzyty MyEqualityComparer ktory sprawdza wartosci?
            freqSlotSwitchingTable.Remove(new int[] { wireA, FSidA });
            freqSlotSwitchingTable.Remove(new int[] { wireB, FSidB });
        }

        public int[] findRoute(int wire, int FSid)
        {
            //return freqSlotSwitchingTable[new int[] {wire, FSid}];
            int[] result;
            if (freqSlotSwitchingTable.TryGetValue(new int[] { wire, FSid }, out result))
                return result;
            else
                return null;
        }
    }

    public class MyEqualityComparer : IEqualityComparer<int[]>
    {
        public bool Equals(int[] x, int[] y)
        {
            if (x.Length != y.Length)
            {
                return false;
            }
            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] != y[i])
                {
                    return false;
                }
            }
            return true;
        }

        public int GetHashCode(int[] obj)
        {
            int result = 17;
            for (int i = 0; i < obj.Length; i++)
            {
                unchecked
                {
                    result = result * 23 + obj[i];
                }
            }
            return result;
        }
    }
}
