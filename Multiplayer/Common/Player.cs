using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;

namespace Common
{
    public class Player
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Lobby Lobby { get; set; }
        public int LobbyPos { get; set; }
        public Queue<Message> Messages { get; set; }
        [JsonIgnore]
        public UdpClient UdpClient { get; set; }
        public GameState GameState { get; set; }
    }
}
