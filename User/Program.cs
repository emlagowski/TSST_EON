using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace User
{
    class Program
    {
        static void Main(string[] args)
        {
            User user = new User("127.0.0.5", "127.0.0.10");
            user.connect();
            //user.Send("siemka", "127.0.0.20"); // jeszcze nie obslugiwane w ruterze
            //dopisac w routerze, cos czekajaca na polaczenie z klienta na porcie 7000. 
            //Addres docelowy podany jest w pakiecie |
        }
    }
}
