using System;
using System.Net;

namespace Common
{
    public class Lobby
    {
        public string MulticastIP { get; set; }
        public int MulticastPort { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int PlayerCount { get; set; }
    }
}
