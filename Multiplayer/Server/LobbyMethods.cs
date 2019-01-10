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
		* Player removed from server
		* ifthere's no one left lobby is deleted else message is sent to the remaining players
		*/
		private void RemovePlayer(Player p, IPEndPoint endPoint)
		{
			LobbyServerSide l = lobbies[p.Lobby.Id];

			l.Players[p.LobbyPos] = null;
			Console.WriteLine("Player: " + p.Name + " left");

			if(CountPlayersInLobby(l) > 0)
			{
				SendPlayersInLobby(l, p);
				Console.WriteLine("    " + p.Name + " left the lobby.");
			}
			else
				RemoveLobby(p);
		}

		/*
         * Lobby deleted
         */
		private void RemoveLobby(Player p)
		{
			LobbyServerSide l = lobbies[p.Lobby.Id];

			lobbies.Remove(p.Lobby.Id);
			Console.WriteLine(string.Format("Lobby deleted: {0}", l.Name));
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
                Console.WriteLine(p.Name + " has joined " + l.Name);
            }
            byte[] msg = Encoding.ASCII.GetBytes(playerJson);

            server.Send(msg, msg.Length, endPoint);
        }

        /*
         * Called when a new player joins a lobby
         */ 
        private void HandlePlayerJoinLobby(Player p)
        {
            LobbyServerSide l = lobbies[p.Lobby.Id];

            SendPlayersInLobby(l, p);
        }

		/*
		* Called when a new player readies up
		*/
		private void HandlePlayerReady(Player p, IPEndPoint endPoint)
		{
			LobbyServerSide l = lobbies[p.Lobby.Id];
			l.Players[p.LobbyPos].GameState = p.GameState;
			Console.WriteLine("    " + l.Players[p.LobbyPos].Name + " has readied up.");

            int readies = 0;
            foreach (Player player in l.Players)
            {
                if (player != null && player.GameState == GameState.LobbyReady)
                    readies++;
            }
            Console.WriteLine(string.Format("{0} out of {1} players are ready in {2}.", readies, MaxPlayersPerLobby, l.Name));

            if (readies == MaxPlayersPerLobby)
            {
                foreach (Player player in l.Players)
                    player.GameState = GameState.GameStarted;
                Console.WriteLine(l.Name + " is going to start.");

                SendGameStartMessage(l);
            }
            SendPlayersInLobby(l, p);
		}

        /*
        * Called when a new player readies up
        */
        private void HandlePlayerUnready(Player p)
        {
            LobbyServerSide l = lobbies[p.Lobby.Id];
            l.Players[p.LobbyPos].GameState = p.GameState;

            SendPlayersInLobby(l, p);
            Console.WriteLine("    " + l.Players[p.LobbyPos].Name + " is not ready.");
        }

        /*
         * Sends all players info to multicast group
         */
        private void SendPlayersInLobby(LobbyServerSide l, Player p)
        {
            Message message = new Message();
            message.MessageType = MessageType.LobbyStatus;
            message.Description = JsonConvert.SerializeObject(l.Players);
            string messageJson = JsonConvert.SerializeObject(message);
            byte[] msg = Encoding.ASCII.GetBytes(messageJson);
            l.UdpLobby.Send(msg, msg.Length, l.EndPoint);
        }

        private void SendGameStartMessage(LobbyServerSide l)
        {
            Message msg = new Message();
            msg.MessageType = MessageType.GameStart;
            string msgJson = JsonConvert.SerializeObject(msg);
            byte[] msgBytes = Encoding.ASCII.GetBytes(msgJson);
            l.UdpLobby.Send(msgBytes, msgBytes.Length, l.EndPoint);
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
