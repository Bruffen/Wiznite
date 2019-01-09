using Common;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    public class LobbyServerSide
    {
        public string Name { get; set; }
        public Player[] Players { get; set; }
        public UdpClient UdpLobby { get; set; }
        public IPEndPoint EndPoint { get; set; }

        public LobbyServerSide(string name)
        {
            this.Name = name;
            Players = new Player[ServerController.MaxPlayersPerLobby];
        }
    }
}
