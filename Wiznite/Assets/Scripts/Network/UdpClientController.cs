using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Common;
using System.Net.Sockets;
using Newtonsoft.Json;
using System.Text;
using System.Net;

namespace UdpNetwork
{
    public class UdpClientController
    {
        public Player Player;
        private UdpClient udpClient;
        private IPEndPoint endPoint;

        public UdpClientController()
        {
            int port = 7777;
            udpClient = new UdpClient();
            endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
            udpClient.Connect(endPoint);
        }

        public void NewLobby(string lobbyName, string playerName)
        {
            Player = new Player();
            Player.Name = playerName;
            Player.GameState = GameState.LobbyCreation;
            Player.UdpClient = udpClient;

            Lobby lobby = new Lobby();
            lobby.Name = lobbyName;
            Player.Lobby = lobby;

            string playerJson = JsonConvert.SerializeObject(Player);
            byte[] msg = Encoding.ASCII.GetBytes(playerJson);

            udpClient.Send(msg, msg.Length);
        }
    }
}
