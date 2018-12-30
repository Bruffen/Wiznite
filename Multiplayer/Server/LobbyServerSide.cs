using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server
{
    public class LobbyServerSide
    {
        public string Name { get; set; }
        public List<Player> Players { get; set; }

        public LobbyServerSide(string name)
        {
            this.Name = name;
            Players = new List<Player>();
        }
    }
}
