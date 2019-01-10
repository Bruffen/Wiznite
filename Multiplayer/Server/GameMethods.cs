using Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Text;

namespace Server
{
    public partial class ServerController
    {

        private void SyncPlayersInLobby(LobbyServerSide l, Player p)
        {
            foreach(Message m in p.Messages)
            {
                string messageJson = JsonConvert.SerializeObject(m);
                byte[] msg = Encoding.ASCII.GetBytes(messageJson);
                l.UdpLobby.Send(msg, msg.Length, l.EndPoint);
            }
        }
    }
}
