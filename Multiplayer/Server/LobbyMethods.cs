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
        /*
         * Creation of player 
         * Information suchs as IDs are sent back to the player
         */
        private void NewPlayer(Player p, IPEndPoint endPoint)
        {
            p.Id = Guid.NewGuid();
            p.GameState = GameState.LobbyDisconnected;
            p.IP = endPoint;
            players.Add(p.Id, p);
            Console.WriteLine("New player: " + p.Name);

            //Send player back with generated ID
            string playerJson = JsonConvert.SerializeObject(p);
            byte[] msg = Encoding.ASCII.GetBytes(playerJson);

            server.Send(msg, msg.Length, endPoint);
        }

        /*
         * Lobby creation from owner player
         * Owner player sends lobby's name
         */
        private void NewLobby(Player p, IPEndPoint endPoint)
        {
            LobbyServerSide newLobby = new LobbyServerSide(p.Lobby.Name);
            p.Lobby.Id = Guid.NewGuid();
            p.Lobby.MulticastIP = GetNextAdress();
            p.Lobby.MulticastPort = multicastPort;

            newLobby.MulticastIP = p.Lobby.MulticastIP;
            newLobby.MulticastPort = p.Lobby.MulticastPort;
            newLobby.UdpLobby = new UdpClient();
            newLobby.UdpLobby.JoinMulticastGroup(IPAddress.Parse(newLobby.MulticastIP));

            lobbies.Add(p.Lobby.Id, newLobby);
            Console.WriteLine(string.Format("New lobby created: {0}, {1} listenning in {2}:{3}", newLobby.Name, p.Lobby.Id, p.Lobby.MulticastIP, p.Lobby.MulticastPort));
            p.GameState = GameState.LobbyConnecting;

            JoinCreatedLobby(p, endPoint);
        }

        /*
         * Joining owner to newly created lobby
         * Send player with lobby ID back
         */
        private void JoinCreatedLobby(Player p, IPEndPoint endPoint)
        {
            lobbies[p.Lobby.Id].Players.Add(p);
            p.GameState = GameState.LobbyUnready;
            Console.WriteLine(p.Name + " has joined " + lobbies[p.Lobby.Id].Name);

            string playerJson = JsonConvert.SerializeObject(p);
            byte[] msg = Encoding.ASCII.GetBytes(playerJson);
            server.Send(msg, msg.Length, endPoint);
        }

        /*
         * Prepare client side lobbies to send list to players
         * This is so player can see all existing lobbies and choose to join one
         */
        private List<Lobby> ListLobbies()
        {
            List<Lobby> lobbyList = new List<Lobby>();
            foreach (KeyValuePair<Guid, LobbyServerSide> lobby in lobbies)
            {
                Lobby l = new Lobby
                {
                    Id = lobby.Key,
                    Name = lobby.Value.Name,
                    PlayerCount = lobby.Value.Players.Count
                };

                lobbyList.Add(l);
            }

            return lobbyList;
        }

        /*
         * Send message listing all existing lobbies
         */
        private void SendExistingLobbies(IPEndPoint endPoint)
        {
            Message message = new Message();
            message.MessageType = MessageType.ListLobbies;
            message.Description = JsonConvert.SerializeObject(ListLobbies());
            string msgJson = JsonConvert.SerializeObject(message);
            byte[] msg = Encoding.ASCII.GetBytes(msgJson);

            server.Send(msg, msg.Length, endPoint);
        }

        /*
         * Player chooses an existing lobby to join and sends its ID here
         * Everything from the lobby is then sent back
         */
        private void JoinExistingLobby(Player p, IPEndPoint endPoint)
        {
            string playerJson = "null";
            LobbyServerSide l = lobbies[p.Lobby.Id];
            if (l.Players.Count < MaxPlayersPerLobby)
            {
                l.Players.Add(p);
                Console.WriteLine(p.Name + " has joined " + l.Name);

                p.Lobby.Name = l.Name;
                p.Lobby.PlayerCount = l.Players.Count;
                p.Lobby.MulticastIP = l.MulticastIP;
                p.Lobby.MulticastPort = l.MulticastPort;
                p.GameState = GameState.LobbyUnready;

                playerJson = JsonConvert.SerializeObject(p);
            }
            byte[] msg = Encoding.ASCII.GetBytes(playerJson);

            server.Send(msg, msg.Length, endPoint);
        }

        /*
         * Create new multicast IP adress for new lobby
         * Checks if any are already in use or not
         */
        private string GetNextAdress()
        {
            string ipAddress;
            do
            {
                ipPart4++;
                if (ipPart4 >= 256)
                {
                    ipPart4 = 0;
                    ipPart3++;
                    if (ipPart3 >= 256)
                    {
                        ipPart3 = 0;
                        ipPart2++;
                        if (ipPart2 >= 256)
                        {
                            ipPart2 = 0;
                        }
                    }
                }
                ipAddress = string.Format("{0}.{1}.{2}.{3}", ipPart1, ipPart2, ipPart3, ipPart4);
            } while (multicastAddressesInUse.Contains(IPAddress.Parse(ipAddress)));

            //Does port need to change too?

            return ipAddress;
        }
    }
}
