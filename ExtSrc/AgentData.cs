using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtSrc
{
   public class AgentData
    {
       public String routerAddress;
       public FIB fib;
       public UnexpectedFIB unFib;

       public AgentData(String address, FIB fib, UnexpectedFIB ufib) 
       {
           routerAddress = address;
           this.fib = fib;
           unFib = ufib;
       
       }
       
    }
}
