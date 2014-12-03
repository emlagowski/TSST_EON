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
        public String routerIPAddress{ get; set; }
        public String clientIPAddress { get; set; }
        //addfreqslot
        public int startingFreq { get; set; }
        public int FSUCount { get; set; }
        public int lastFSUCount { get; set; }
        public Modulation mod { get; set; }
        public Modulation lastMod { get; set; }
        //wires
        public int firstWireID { get; set; }
        public int secondWireID { get; set; }
        public int bitrate { get; set; }

        //--------------------------
        //--------------ClientTables
        public int wireID { get; set; }
        public int FSid { get; set; }
        public int secondFSid { get; set; }
        public int clientSocketID { get; set; }
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

        public AgentData(AgentComProtocol msg, int startfreq, int FSid)
            : this()
        {
            if (AgentComProtocol.DISROUTE.Equals(msg) || AgentComProtocol.DISROUTE_EDGE.Equals(msg))
            {
                message = msg;
                firstWireID = startfreq;
                this.FSid = FSid;

            }
            else
            {
                message = msg;
                startingFreq = startfreq;
                this.FSid = FSid;
            }
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
            clientSocketID = sockID;
            
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
            clientSocketID = sockID;                              
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
            clientSocketID = -1;
            uniqueKey = null;
            wireIDsList = new List<DijkstraData>();
        }

        public AgentData(AgentComProtocol msg, String ip, String unqKey)
            : this()
        {
            if (msg == AgentComProtocol.REGISTER_CLIENT)
            {
                //rejestracja klienta w w agencie, 
                this.message = msg;
                this.clientIPAddress = unqKey;
                this.routerIPAddress = ip;
            }
            else
            {
                message = msg;
                targetAddress = ip;
                uniqueKey = unqKey;
            }
        }

        public AgentData(AgentComProtocol msg, String originating, String clientIP, String ip, String unqKey, int bitrate)
            : this()
        {
            message = msg;
            targetAddress = ip;
            uniqueKey = unqKey;
            clientIPAddress = clientIP;
            routerIPAddress = originating;
            this.bitrate = bitrate;
        }

        public AgentData(AgentComProtocol msg, String unKey)
            : this()
        {
            message = msg;
            uniqueKey = unKey;
        }

        //public AgentData(AgentComProtocol msg, int frstWid, int frstFSid, int secWid, int secFSid)
        //    : this()
        //{
        //    message = msg;
        //    firstWireID = frstWid;
        //    FSid = frstFSid;
        //    secondWireID = secWid;
        //    secondFSid = secFSid;
        //}

        //fsucount, mod, firstwireid,secondwireid, startingfreq dla odbierajacego kabla bo juz obliczone w poprzednim roouterze
        public AgentData(AgentComProtocol msg, int lastfsucount, Modulation lastmod, int fsucount, Modulation md, int firstWireID, int secondWireID, int startingfreq)
            : this()
        {
            message = msg;
            lastFSUCount = lastfsucount;
            lastMod = lastmod;
            FSUCount = fsucount;
            mod = md;
            this.firstWireID = firstWireID;
            this.secondWireID = secondWireID;
            this.startingFreq = startingfreq;
        }

        public AgentData(AgentComProtocol agentComProtocol, int fsuCount, Modulation modulation, int wireID, int clientID)
            : this()
        {
            this.message = agentComProtocol;
            this.FSUCount = fsuCount;
            this.mod = modulation;
            this.wireID = wireID;
            this.clientSocketID = clientID;
        }



    }
    public enum AgentComProtocol { NULL, REGISTER, REGISTER_CLIENT, SET_ROUTE_FOR_ME, ROUTE_FOR_U_EDGE, ROUTE_FOR_U, U_CAN_SEND, DISROUTE, DISROUTE_EDGE, DISROUTE_IS_DONE, CONNECTION_IS_ON, CONNECTION_UNAVAILABLE, MSG_DELIVERED, DISROUTE_ERROR_EDGE, DISROUTE_ERROR, DISROUTE_EDGE_IS_DONE }
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



