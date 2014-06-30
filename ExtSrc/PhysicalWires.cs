using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ExtSrc
{   [Serializable()]
    public class PhysicalWires
    {
        public List<NewWire> Wires { get; set; }

        public PhysicalWires()
        {
            Wires = new List<NewWire>();
        }

        public void add(NewWire w){
            Wires.Add(w);
        }


        public NewWire getWireByID(int id)
        {
            foreach (NewWire nw in Wires)
                if (nw.ID == id)
                    return nw;
            return null;
        }
        public int[] getIDsbyPort(int port)
        {
            foreach(NewWire nw in Wires){
                foreach (FrequencySlot slot in nw.FrequencySlotDictionary.Values)
                {
                    foreach (FrequencySlotUnit unit in slot.FSUList)
                    {
                        if (unit.port == port) return new int[] {nw.ID, slot.ID };
                    }
                    //if ( == port) return new int[] { nw.ID, l.ID };
                }
            }
            return null;
        }
        public List<Socket> getSockets(int[] IDs)
        {
            List<Socket> list = new List<Socket>();
            foreach (NewWire nw in Wires)
            {
                if(nw.ID == IDs[0])
                {
                    
                    FrequencySlot fs;
                    nw.FrequencySlotDictionary.TryGetValue(IDs[1], out fs);
                    if (fs != null)
                    {
                        foreach (FrequencySlotUnit unit in fs.FSUList)
                           {
                                list.Add(unit.socket);
                           }
                    }
                }
            }
            return list;            
        }

        public List<FrequencySlotUnit> getFrequencySlotUnits(int[] IDs)
        {
            List<FrequencySlotUnit> list = new List<FrequencySlotUnit>();
            foreach (NewWire nw in Wires)
            {
                if (nw.ID == IDs[0])
                {

                    FrequencySlot fs;
                    nw.FrequencySlotDictionary.TryGetValue(IDs[1], out fs);
                    if (fs != null)
                    {
                        foreach (FrequencySlotUnit unit in fs.FSUList)
                        {
                            list.Add(unit);
                        }
                    }
                }
            }
            return list;

        }
            
        }
    }
