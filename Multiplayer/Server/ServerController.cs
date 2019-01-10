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
        public const int MaxPlayersPerLobby = 4;
        private Dictionary<Guid, LobbyServerSide> lobbies;
        private Dictionary<Guid, Player> players;
        private List<IPAddress> multicastAddressesInUse;
        private int ipPart1, ipPart2, ipPart3, ipPart4;
        private UdpClient server;
        private int multicastPort = 7778;

        public ServerController()
        {
            ipPart1 = 233;
            ipPart2 = 0;
            ipPart3 = 0;
            ipPart4 = 0;
            lobbies = new Dictionary<Guid, LobbyServerSide>();
            players = new Dictionary<Guid, Player>();
            multicastAddressesInUse = new List<IPAddress>();
            Console.WriteLine("Multicast addresses in use:");
            foreach (NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (adapter.Supports(NetworkInterfaceComponent.IPv4))
                {
                    foreach (IPAddressInformation multi in adapter.GetIPProperties().MulticastAddresses)
                    {
                        if (multi.Address.AddressFamily != AddressFamily.InterNetworkV6)
                        {
                            Console.WriteLine("    " + multi.Address);
                            multicastAddressesInUse.Add(multi.Address);
                        }
                    }
                }
            }
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
						case GameState.LobbyDisconnecting:
							RemovePlayer(playerMsg, endPoint);
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
                        case GameState.LobbySync:
                            HandlePlayerJoinLobby(playerMsg);
                            break;
                        case GameState.LobbyReady:
                            HandlePlayerReady(playerMsg, endPoint);
                            break;
                        case GameState.LobbyUnready:
                            HandlePlayerReady(playerMsg, endPoint);
                            break;
                        case GameState.GameSync:
                            SyncPlayersInLobby(lobbies[playerMsg.Lobby.Id], playerMsg);
                            break;
					}
                }
            }
        }
    }
}
