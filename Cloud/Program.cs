using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cloud
{
    class Program
    {
        static void Main(string[] args)
        {
            Cloud cloud = new Cloud();
            cloud.connect(1, 2);
            cloud.connect(3, 4);
            cloud.connect(5, 6);
            cloud.connect(1, 4);
            cloud.connect(6, 3);
            cloud.disconnect(1, 2);
            Console.ReadLine();
        }
    }
}
