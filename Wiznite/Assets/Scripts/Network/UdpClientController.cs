using Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace UdpNetwork
{
    public class UdpClientController
    {
        public Player Player;
        private UdpClient udpClient;
        private IPEndPoint endPoint, multicastEndPoint;
        private Thread thread;

        public UdpClientController()
        {
            int port = 7777;
            udpClient = new UdpClient();
            endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
            udpClient.Connect(endPoint);
        }

        public void CreatePlayer(string name)
        {
            Player = new Player();
            Player.UdpClient = udpClient;
            Player.Name = name;
            Player.GameState = GameState.LobbyDisconnected;
            Player.Messages = new List<Message>();
            SendPlayerMessage();

            Player answerPlayer = ReceivePlayerMessage();
            Player.Id = answerPlayer.Id;
        }

        /*
         * Creates Player Json message and sends it to server
         * It's done a lot so this method is to make things easier and cleaner
         */
        private void SendPlayerMessage()
        {
            string playerJson = JsonConvert.SerializeObject(Player);
            byte[] msg = Encoding.ASCII.GetBytes(playerJson);
            udpClient.Send(msg, msg.Length);
        }

        private Player ReceivePlayerMessage()
        {
            byte[] answer = udpClient.Receive(ref endPoint);
            string answerJson = Encoding.ASCII.GetString(answer);
            return JsonConvert.DeserializeObject<Player>(answerJson);
        }

        private void Listen()
        {
            //Join multicast for lobby communication
            Debug.Log("Thread started");
            udpClient.Close();
            udpClient = new UdpClient();
            multicastEndPoint = new IPEndPoint(IPAddress.Any, Player.Lobby.MulticastPort);
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpClient.Client.Bind(multicastEndPoint);
            udpClient.JoinMulticastGroup(IPAddress.Parse(Player.Lobby.MulticastIP));

            while (true)
            {
                udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), udpClient);
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            UdpClient u = (UdpClient)ar;
            byte[] msg = u.EndReceive(ar, ref multicastEndPoint);

            string msgJson = Encoding.ASCII.GetString(msg);
            try
            {
                Message message = JsonConvert.DeserializeObject<Message>(msgJson);
                if (message != null)
                {
                    Player.Messages.Add(message);
                    Debug.Log("Message received: " + message.Description);
                }
            }
            catch (System.Exception e)
            {
                Debug.Log(e.StackTrace);
            }
        }

        /*
         * Create new lobby with name and send it the server
         * Receive server's answer with lobby's generated Id
         */
        public void NewLobby(string lobbyName)
        {
            Player.GameState = GameState.LobbyCreation;

            Lobby lobby = new Lobby();
            lobby.Name = lobbyName;
            Player.Lobby = lobby;
            SendPlayerMessage();

            Player answerPlayer = ReceivePlayerMessage();
            Player.Lobby = answerPlayer.Lobby;
            StartThread();
        }

        /*
         * Asks server for a Message with all lobbies listed
         */
        public List<Lobby> LobbyList()
        {
            Player.GameState = GameState.LobbiesRequest;
            SendPlayerMessage();

            Player.GameState = GameState.LobbyDisconnected;

            byte[] answer = udpClient.Receive(ref endPoint);
            string answerJson = Encoding.ASCII.GetString(answer);
            Message message = JsonConvert.DeserializeObject<Message>(answerJson);
            if (message.MessageType == MessageType.ListLobbies)
            {
                return JsonConvert.DeserializeObject<List<Lobby>>(message.Description);
            }
            return null;
        }

        /*
         * Asks server permission to join asked lobby
         * Answer will be null if lobby is already full and therefore return false
         */
        public bool JoinExistingLobby(Guid lobbyID)
        {
            Player.GameState = GameState.LobbyConnecting;
            Player.Lobby = new Lobby { Id = lobbyID };
            SendPlayerMessage();

            Player answerPlayer = ReceivePlayerMessage();

            if (answerPlayer != null)
            {
                Player.GameState = answerPlayer.GameState;
                Player.Lobby = answerPlayer.Lobby;
                StartThread();
                return true;
            }
            return false;
        }

        private void StartThread()
        {
            thread = new Thread(new ThreadStart(Listen));
            thread.Start();
        }

        /*
         * Kills listenning thread
         * Redoes unicast udp connection
         */
        public void CloseThread()
        {
            udpClient.Close();
            udpClient = new UdpClient();
            udpClient.Connect(endPoint);

            thread.Abort();
            thread.Join(500);
            thread = null;
        }
    }
}
