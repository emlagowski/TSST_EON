using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ExtSrc
{
    using ExtSrc.Observers;
    using System.Threading;
    using Observer = ExtSrc.Observers.Observer;
    public class NewWire : Observer
    {
        static readonly int GUARD_BAND = 10;
        static readonly int FREQ_SLOT_UNIT = 10;
        static readonly int EMPTY_VALUE = -1;

        public int ID { get; set; }
        public int distance { get; set; }
        //public List<FrequencySlotUnit> lambdas { get; set; }
        public List<FrequencySlotUnit> FrequencySlotUnitList { get; set; }
        public Dictionary<int, FrequencySlot> FrequencySlotDictionary { get; set; }

        public int[] spectralWidth;
        public NewWire(int id, int distance, int lambdasSize, int spectralWidth)
        {
            this.ID = id;
            this.distance = distance;
            FrequencySlotUnitList = new List<FrequencySlotUnit>();
            FrequencySlotDictionary = new Dictionary<int, FrequencySlot>();
            //represents 1000Ghz
            this.spectralWidth = Enumerable.Repeat(EMPTY_VALUE, spectralWidth).ToArray(); //new int[spectralWidth];
        }

        public int addFreqSlot(int startingFreq, int FSUcount, Modulation mod)
        {

            int id = FrequencySlotDictionary.Values.Count();
            FrequencySlot slot = new FrequencySlot(id, Modulation.QPSK);

            FrequencySlotDictionary.Add(id, slot);

            for (int i = 0; i < FSUcount; i++)
            {
                foreach (FrequencySlotUnit unit in FrequencySlotUnitList)
                {
                    if (!unit.isUsed)
                    {
                        unit.isUsed = true;
                        slot.FSUList.Add(unit);
                        break;
                    }
                }
            }
            takeSpectralWidth(startingFreq, FSUcount * FREQ_SLOT_UNIT + GUARD_BAND, id);
            return id;
        }

        public void takeSpectralWidth(int start, int count, int id)
        {
            for (int i = start; i < start + count; i++)
            {
                spectralWidth[i] = id;
            }
        }

        public void removeSpectralWidth(int start, int count)
        {
            for (int i = start; i < start + count; i++)
            {
                spectralWidth[i] = EMPTY_VALUE;
            }
        }

        public void initWire(String address, IPEndPoint cloudEP)
        {
            foreach (var l in FrequencySlotUnitList)
            {
                l.initFSUSocket(address, cloudEP);
                //Thread.Sleep(1000);
            }
        }

        public void update()
        {
            //close all sockets
            foreach (var l in FrequencySlotUnitList)
            {
                l.close();
            }
        }
    }
}
