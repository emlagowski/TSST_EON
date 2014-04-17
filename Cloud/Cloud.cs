using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace Cloud
{
    class Cloud
    {
        ArrayList busyPorts;
        ArrayList usedConnections;

        public Cloud()
        {
            busyPorts = new ArrayList();
            usedConnections = new ArrayList();
        }

        /**
         * Metoda ustanawia polaczenie dwoch portow w postaci lacza. 
         * Jesli porty sa wolne dodaje je do listy portow zajetych 
         * i tworzy obiekt Connection ktory dodaje do listy usedConnection.
         **/
        public void connect(int portA, int portB){
            Boolean flag = true;
            int i =0;
            foreach(int obj in busyPorts)
            {
                if (obj == portA || obj == portB)
                {
                    Console.WriteLine("This port is used.");
                    flag = false;
                    break; //czy ten brejk wyjdzie z calego foreach?
                }
                Console.WriteLine(i);
                i++;
            }
            if (flag == true)
            {
                busyPorts.Add(portA);
                busyPorts.Add(portB);
                usedConnections.Add(new Connection(portA, portB));
                Console.WriteLine("Connection is on.");
            }
        }

        /**
         * Metoda wylacza polaczenie dwoch portow w postaci lacza. 
         * Najpierw sprawdza czy numery portow sa uzyte, jesli tak to je usuwa z listy a nastepnie
         * robi to samo z Connection.
         * **/
        public void disconnect(int portA, int portB)
        {
            int pa=-1, pb=-1, cc=-1;
            Boolean flag = false;
            foreach (int obj in busyPorts)
            {
                if (obj == portA)
                {
                    pa = busyPorts.IndexOf(obj);
                }
                if (obj == portB)
                {
                    pb = busyPorts.IndexOf(obj);
                }
            }
            Connection c = new Connection(portA, portB);
            foreach (Connection obj in usedConnections)
            {
                Connection d = obj as Connection;
                if (d.equals(c))
                {
                    cc = usedConnections.IndexOf(obj);
                    flag = true;
                    break;
                }
            }
            if (flag == true)
            {
                usedConnections.RemoveAt(cc);
                if (pa < pb)
                {
                    busyPorts.RemoveAt(pb);
                    busyPorts.RemoveAt(pa);
                }
                else
                {
                    busyPorts.RemoveAt(pa);
                    busyPorts.RemoveAt(pb);
                }
                Console.WriteLine("Connection is off.");
            }
            else
            {
                Console.WriteLine("There is no connection like this.");
            }

        }

        /**
         * Meta sluzaca do przesylania.
         * **/
        public void send(Object obj)
        {

        }

        class Connection
        {
            int portA;
            int portB;

            public Connection(int portA, int portB)
            {
                this.portA = portA;
                this.portB = portB;
            }

            public int getA()
            {
                return portA;
            }

            public int getB()
            {
                return portB;
            }

            public Boolean equals(Object obj)
            {
                if (obj == null)
                {
                    return false;
                }
                if (obj.GetType() != typeof(Connection))
                {
                    return false;
                }
                Connection c = obj as Connection;
                if (c.getA() == this.getA() && c.getB() == this.getB())
                {
                    return true;
                }
                return false;
            }
        }
    }
}
