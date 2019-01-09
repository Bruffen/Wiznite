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

            newLobby.UdpLobby = new UdpClient();
            newLobby.EndPoint = new IPEndPoint(IPAddress.Parse(p.Lobby.MulticastIP), p.Lobby.MulticastPort);
            newLobby.UdpLobby.JoinMulticastGroup(newLobby.EndPoint.Address);

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
            lobbies[p.Lobby.Id].Players[0] = p;
            p.GameState = GameState.LobbyUnready;
            p.LobbyPos = 0;
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
                    PlayerCount = CountPlayersInLobby(lobby.Value)
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
            bool isJoinable = CountPlayersInLobby(l) < MaxPlayersPerLobby;
            if (isJoinable)
            {
                int pos = 0;
                for (int i = 0; i < l.Players.Length; i++)
                    if (l.Players[i] == null)
                    {
                        pos = i;
                        break;
                    }


                l.Players[pos] = p;
                p.Lobby.Name = l.Name;
                p.Lobby.PlayerCount = CountPlayersInLobby(l);
                p.LobbyPos = pos;
                p.Lobby.MulticastIP = l.EndPoint.Address.ToString();
                p.Lobby.MulticastPort = l.EndPoint.Port;
                p.GameState = GameState.LobbyUnready;

                playerJson = JsonConvert.SerializeObject(p);
                Console.WriteLine("    " + playerJson);
                Console.WriteLine(p.Name + " has joined " + l.Name);
            }
            byte[] msg = Encoding.ASCII.GetBytes(playerJson);

            server.Send(msg, msg.Length, endPoint);
        }

        /*
         * Sends all players info to multicast group
         * Called when a new player joins a lobby
         */ 
        private void HandlePlayerJoinLobby(Player p, IPEndPoint endPoint)
        {
            LobbyServerSide l = lobbies[p.Lobby.Id];

            Message message = new Message();
            message.MessageType = MessageType.LobbyNewPlayer;
            message.Description = JsonConvert.SerializeObject(l.Players);
            string messageJson = JsonConvert.SerializeObject(message);
            Console.WriteLine("    Sending data to lobby " + l.Name);
            Console.WriteLine("    " + messageJson);
            byte[] msg = Encoding.ASCII.GetBytes(messageJson);
            l.UdpLobby.Send(msg, msg.Length, l.EndPoint);
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

        private int CountPlayersInLobby(LobbyServerSide l)
        {
            int count = 0;
            for (int i = 0; i < l.Players.Length; i++)
            {
                if (l.Players[i] != null)
                    count++;
            }
            return count;
        }
    }
}
