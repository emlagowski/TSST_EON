using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtSrc
{
    [Serializable()]
    public class AgentData
    {

        public AgentComProtocol message;
        
        //-----------------ROUTING--
        public String originatingAddress{get; set;}
        public String targetAddress { get; set; }
        //addfreqslot
        public int startingFreq { get; set; }
        public int FSUCount { get; set; }
        public Modulation mod { get; set; }
        //wires
        public int firstWireID { get; set; }
        public int secondWireID { get; set; }
        //--------------------------
        //--------------ClientTables
        public int wireID { get; set; }
        public int FSid { get; set; }
        public int socketID { get; set; }
        //--------------------------
        //--------------ROUTING RETURN MSG
        public int FSreturnID { get; set; }
        public String uniqueKey { get; set; }
        //--------------REGISTER WIRE_IDs
        public List<DijkstraData> wireIDsList { get; set; }

        public AgentData(AgentComProtocol msg):this()
        {
            message = msg;
        }

        public AgentData(AgentComProtocol msg, List<DijkstraData> wiresList)
            : this()
        {
            message = msg;
            wireIDsList = wiresList;
        }

        public AgentData(AgentComProtocol msg, int startfreq)
            : this()
        {
            message = msg;
            startingFreq = startfreq;
        }

        public AgentData(AgentComProtocol msg, String origAddr, String targetAddr, int startFq, int FSUcnt, Modulation md, int frstWid, int secWid,
            int wID, int FSid, int sockID)
            : this()
        {
            message = msg;
            originatingAddress = origAddr;
            targetAddress = targetAddr;
            startingFreq = startFq;
            FSUCount = FSUcnt;
            mod = md;
            firstWireID = frstWid;
            secondWireID = secWid;
            wireID = wID;
            this.FSid = FSid;
            socketID = sockID;
            
        }

        public AgentData(AgentComProtocol msg, String origAddr, String targetAddr, int startFq, int FSUcnt, Modulation md, int frstWid, int secWid)
            : this()
        {
            message = msg;
            originatingAddress = origAddr;
            targetAddress = targetAddr;
            startingFreq = startFq;
            FSUCount = FSUcnt;
            mod = md;
            firstWireID = frstWid;
            secondWireID = secWid;
          
        }
        public AgentData(AgentComProtocol msg, int wID, int FSid, int sockID):this()
        {
            message = msg;
            wireID = wID;
            this.FSid = FSid;
            socketID = sockID;                              
        }
        public AgentData()
        {
            message = AgentComProtocol.NULL;
            originatingAddress = null;
            targetAddress = null;
            startingFreq = -1;
            FSUCount = -1;
            mod = Modulation.NULL;
            firstWireID = -1;
            secondWireID = -1;
            wireID = -1;
            this.FSid = -1;
            socketID = -1;
            uniqueKey = null;
            wireIDsList = new List<DijkstraData>();
        }

        public AgentData(AgentComProtocol msg, String ip, String unqKey)
            : this()
        {
            message = msg;
            targetAddress = ip;
            uniqueKey = unqKey;
        }

    }
    public enum AgentComProtocol { NULL, REGISTER, SET_ROUTE_FOR_ME, ROUTE_FOR_U_EDGE, ROUTE_FOR_U, U_CAN_SEND , DISROUTE, DISROUTE_EDGE, CONNECTION_IS_ON, CONNECTION_UNAVAILABLE, MSG_DELIVERED }
    ///    ###########     MSG TYPES    ##########
    ///    NULL                 -
    ///    REGISTER             - router rejestruje sie u NMS'a
    ///    SET_ROUTE_FOR_ME     - router prosi NMS'a o zestawienie drogi do danego adresu( tylko router edge moze wysalc)
    ///    ROUTE_FOR_U_EDGE     - NMS odpowiada routerowi ze ma zestawic konkretne placzenie u siebie (klieckie u router edge )
    ///    ROUTE_FOR_U          - NMS odpowiada routerowi ze ma zestawic konkretne placzenie u siebie ( wew. u router interior)
    ///    U_CAN_SEND           - NMS wysyla do edge , sygnal ze wsyztsko juz gotowe i mozna wysylac wiadomosc
    ///    DISROUTE             - rozłącz połączenie typu route interior
    ///    DISROUTE_EDGE        - rozłącz połączenie klienta z routerem brzegowym
    ///    CONNECTION_IS_ON     - wysyła router do nmsa informując o tym ze zestawil zadane polaczenie
    ///    MSG_DELIVERED        - router edge wysyla do nmsa ze dostał wiadomosc



}



