using Common;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    public class LobbyServerSide
    {
        public UdpClient UdpLobby { get; set; }
        public string MulticastIP { get; set; }
        public int MulticastPort { get; set; }
        public string Name { get; set; }
        public List<Player> Players { get; set; }

        public LobbyServerSide(string name)
        {
            this.Name = name;
            Players = new List<Player>();
        }
    }
}
