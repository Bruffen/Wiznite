using Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    public class ServerController
    {
        private const int MaxPlayersPerLobby = 4;
        private Dictionary<Guid, LobbyServerSide> lobbies;
        private Dictionary<Guid, Player> players;
        private UdpClient server;

        public ServerController()
        {
            lobbies = new Dictionary<Guid, LobbyServerSide>();
            players = new Dictionary<Guid, Player>();
        }

        public void Start()
        {
            int port = 7777;
            Console.WriteLine("Server started listening in port " + port);

            using (server = new UdpClient(port))
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);
                while (true)
                {
                    byte[] msg = server.Receive(ref endPoint);
                    string jsonMsg = Encoding.ASCII.GetString(msg);
                    Player playerMsg = JsonConvert.DeserializeObject<Player>(jsonMsg);

                    switch (playerMsg.GameState)
                    {
                        case GameState.LobbyDisconnected:
                            NewPlayer(playerMsg, endPoint);
                            break;
                        case GameState.LobbyCreation:
                            NewLobby(playerMsg, endPoint);
                            break;
                        case GameState.LobbiesRequest:
                            SendExistingLobbies(endPoint);
                            break;
                        case GameState.LobbyConnecting:
                            JoinExistingLobby(playerMsg, endPoint);
                            break;
                    }
                }
            }
        }

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
            lobbies.Add(p.Lobby.Id, newLobby);
            Console.WriteLine("New lobby created: " + newLobby.Name + ", " + p.Lobby.Id);
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
            if (lobbies[p.Lobby.Id].Players.Count < MaxPlayersPerLobby)
            {
                lobbies[p.Lobby.Id].Players.Add(p);
                Console.WriteLine(p.Name + " has joined " + lobbies[p.Lobby.Id].Name);

                p.Lobby.Name = lobbies[p.Lobby.Id].Name;
                p.Lobby.PlayerCount = lobbies[p.Lobby.Id].Players.Count;
                p.GameState = GameState.LobbyUnready;

                playerJson = JsonConvert.SerializeObject(p);
            }
            byte[] msg = Encoding.ASCII.GetBytes(playerJson);

            server.Send(msg, msg.Length, endPoint);
        }
    }
}
