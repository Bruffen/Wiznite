﻿using System;
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
        public List<Message> Messages { get; set; }
        [JsonIgnore]
        public UdpClient UdpClient { get; set; }
        [JsonIgnore]
        public IPEndPoint IP { get; set; }
        public GameState GameState { get; set; }
    }
}
