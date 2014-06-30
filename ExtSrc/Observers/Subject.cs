using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtSrc.Observers
{
    public interface Subject
    {
        void registerObservers(Observer o);
        void removeObservers(Observer o);
        void notifyObservers();
    }
}
