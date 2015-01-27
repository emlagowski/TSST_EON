#region

using System;
using System.Collections.Generic;

#endregion

namespace ExtSrc
{
    [Serializable()]
    public class AgentData
    {
        public AgentComProtocol Message { get; set; }

        // Originating Client Address
        public String OriginatingAddress { get; set; }

        // Target Client Address
        public String TargetAddress { get; set; }
        public String RouterIpAddress { get; set; }
        public String ClientIpAddress { get; set; }
        public bool IsStartEdge { get; set; }

        //addfreqslot
        public int StartingFreq { get; set; }
        public int FsuCount { get; set; }
        public int LastFsuCount { get; set; }
        public Modulation Mod { get; set; }
        public Modulation LastMod { get; set; }
        //wires
        public int FirstWireId { get; set; }
        public int SecondWireId { get; set; }
        public int Bitrate { get; set; }

        //--------------------------
        //--------------ClientTables
        public int WireId { get; set; }

        public int FSid { get; set; }

        public int ClientSocketId { get; set; }

        // Unique key of connection
        public String UniqueKey { get; set; }

        // In REGISTER message Node put here local IDs of wires
        public List<DijkstraData> WireIDsList { get; set; }

        public List<int> DomainInfo { get; set; }
        public int RouterID { get; set; }
        public int DomainRouterID { get; set; }
        public List<int[]> StartingFreqs { get; set; }
        public List<List<int[]>> StartingFreqsPool { get; set; }

        public AgentData()
        {
            Message = AgentComProtocol.NULL;
            OriginatingAddress = null;
            TargetAddress = null;
            StartingFreq = -1;
            FsuCount = -1;
            Mod = Modulation.NULL;
            FirstWireId = -1;
            SecondWireId = -1;
            WireId = -1;
            FSid = -1;
            ClientSocketId = -1;
            UniqueKey = null;
            WireIDsList = new List<DijkstraData>();
            DomainInfo = new List<int>();
        }
    }
    public enum AgentComProtocol
    {
        NULL,
        REGISTER,
        REGISTER_CLIENT,
        SET_ROUTE_FOR_ME,
        ROUTE_FOR_U_EDGE,
        ROUTE_FOR_U_EDGE_MANUAL,
        ROUTE_FOR_U,
        ROUTE_FOR_U_MANUAL,
        U_CAN_SEND, DISROUTE,
        DISROUTE_EDGE,
        DISROUTE_IS_DONE,
        CONNECTION_IS_ON,
        CONNECTION_UNAVAILABLE,
        MSG_DELIVERED,
        DISROUTE_ERROR_EDGE,
        DISROUTE_ERROR,
        DISROUTE_EDGE_IS_DONE,
        CLIENT_DISCONNECTED,
        DOMAIN_REGISTER,
        DOMAIN_INFO,
        DOMAIN_SET_ROUTE_FOR_ME,
        DOMAIN_CAN_WE_SET_ROUTE,
        DOMAIN_CAN_ROUTE,
        DOMAIN_CAN_SEND,
        AVAIBLE_STARTING_FREQS,
        MY_FREES_FREQ_SLOTS,
        UNREGISTER,
        DOMAIN_DISROUTE,
        MODIFY_UNQCON_AFTER_REPAIR,
        DOMAIN_CAN_NOT_ROUTE,
        ROUTE_UNAVAIBLE
    }

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
    ///    ROUTE_FOR_U_EDGE_MANUAL     - NMS odpowiada routerowi ze ma zestawic konkretne placzenie u siebie (klieckie u router edge )
    ///                                  Z NMSA wysyłane są też sloty które zajmie połączenie
    ///    ROUTE_FOR_U_MANUAL          - NMS odpowiada routerowi ze ma zestawic konkretne placzenie u siebie ( wew. u router interior)
    ///                                  Z NMSA wysyłane są też sloty które zajmie połączenie

}



