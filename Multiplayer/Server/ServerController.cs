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
        private Dictionary<Guid, LobbyServerSide> lobbies;

        public ServerController()
        {
            lobbies = new Dictionary<Guid, LobbyServerSide>();
        }

        public void Start()
        {
            int port = 7777;
            Console.WriteLine("Server started listening in port " + port);

            using (UdpClient client = new UdpClient(port))
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);
                while (true)
                {
                    byte[] msg = client.Receive(ref endPoint);
                    string jsonMsg = Encoding.ASCII.GetString(msg);
                    Player playerMsg = JsonConvert.DeserializeObject<Player>(jsonMsg);

                    switch (playerMsg.GameState)
                    {
                        case GameState.LobbyCreation:
                            NewLobby(playerMsg, client, endPoint);
                            break;
                        case GameState.LobbyConnecting:
                            NewPlayer(playerMsg, client, endPoint);
                            break;
                    }
                }
            }
        }

        /*
         * Lobby creation from owner player
         * Owner player sends lobby's name
         */
        private void NewLobby(Player p, UdpClient client, IPEndPoint endPoint)
        {
            LobbyServerSide newLobby = new LobbyServerSide(p.Lobby.Name);
            p.Lobby.Id = Guid.NewGuid();
            lobbies.Add(p.Lobby.Id, newLobby);
            Console.WriteLine("New lobby created: " + p.Lobby.Id + ", " + newLobby.Name);
            p.GameState = GameState.LobbyConnecting;

            NewPlayer(p, client, endPoint);
        }

        /*
         * Creation of player 
         * Both when he creates a new lobby or joins an already existing one
         * Information suchs as IDs are sent back to the player
         */
        private void NewPlayer(Player p, UdpClient client, IPEndPoint endPoint)
        {
            Console.WriteLine("New player " + p.Name + " in " + p.Lobby.Name);
            p.Id = Guid.NewGuid();
            lobbies[p.Lobby.Id].Players.Add(p);
            p.GameState = GameState.LobbySync;
            p.UdpClient = new UdpClient(endPoint);

            string playerJson = JsonConvert.SerializeObject(p);
            byte[] msg = Encoding.ASCII.GetBytes(playerJson);
            client.Send(msg, msg.Length, endPoint);
        }
    }
}
