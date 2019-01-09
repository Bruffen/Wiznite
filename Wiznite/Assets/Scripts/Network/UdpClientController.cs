using Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public bool SyncPlayers;
        private Dictionary<Guid, LobbyPlayer> lobbyPlayers;
        public List<LobbyPlayer> GetLobbyPlayers() { return lobbyPlayers.Values.ToList(); }

        public UdpClientController()
        {
            int port = 7777;
            udpClient = new UdpClient();
            endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
            udpClient.Connect(endPoint);
            SyncPlayers = false;
        }

        private void ProcessMessage(Message msg)
        {
            switch (msg.MessageType)
            {
                case MessageType.LobbyNewPlayer:
                    Debug.Log("Syncing lobby data");
                    SyncLobby(msg);
                    break;
            }
        }

        public void CreatePlayer(string name)
        {
            Player = new Player();
            Player.UdpClient = udpClient;
            Player.Name = name;
            Player.GameState = GameState.LobbyDisconnected;
            Player.Messages = new Queue<Message>();
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

        private void SendPlayerMessageMulticast()
        {
            string playerJson = JsonConvert.SerializeObject(Player);
            byte[] msg = Encoding.ASCII.GetBytes(playerJson);
            udpClient.Send(msg, msg.Length, endPoint);
        }

        private Player ReceivePlayerMessage()
        {
            byte[] answer = udpClient.Receive(ref endPoint);
            string answerJson = Encoding.ASCII.GetString(answer);
            return JsonConvert.DeserializeObject<Player>(answerJson);
        }

        private void Listen()
        {
            try
            {
                //Join multicast for lobby communication
                udpClient.Close();
                udpClient = new UdpClient();
                multicastEndPoint = new IPEndPoint(IPAddress.Any, Player.Lobby.MulticastPort);
                udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                udpClient.Client.Bind(multicastEndPoint);
                udpClient.JoinMulticastGroup(IPAddress.Parse(Player.Lobby.MulticastIP));
                Debug.Log("Thread started listenning on " + Player.Lobby.MulticastIP);
                //udpClient.Connect(multicastEndPoint);

                while (true)
                {
                    //udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), udpClient);
                    byte[] msg = udpClient.Receive(ref multicastEndPoint);
                    string msgJson = Encoding.ASCII.GetString(msg);

                    Message message = JsonConvert.DeserializeObject<Message>(msgJson);
                    if (message != null)
                    {
                        Debug.Log("Message received: " + message.Description);
                        ProcessMessage(message);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.StackTrace);
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            Debug.Log("Got here");
            UdpClient u = (UdpClient)ar;
            byte[] msg = u.EndReceive(ar, ref multicastEndPoint);

            string msgJson = Encoding.ASCII.GetString(msg);
            try
            {
                Message message = JsonConvert.DeserializeObject<Message>(msgJson);
                if (message != null)
                {
                    Player.Messages.Enqueue(message);
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
            Player.LobbyPos = answerPlayer.LobbyPos;
            Player.Lobby = answerPlayer.Lobby;
            lobbyPlayers = new Dictionary<Guid, LobbyPlayer>();
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
                Player.LobbyPos = answerPlayer.LobbyPos;
                lobbyPlayers = new Dictionary<Guid, LobbyPlayer>();
                StartThread();
                return true;
            }
            return false;
        }

        public void FetchLobbyData()
        {
            Player.GameState = GameState.LobbySync;
            SendPlayerMessageMulticast();
        }

        private void SyncLobby(Message message)
        {
            lobbyPlayers = new Dictionary<Guid, LobbyPlayer>();
            Player[] players = JsonConvert.DeserializeObject<Player[]>(message.Description);
            foreach (Player p in players)
            {
                if (p != null)
                {
                    LobbyPlayer lp = new LobbyPlayer(p);
                    lobbyPlayers.Add(p.Id, lp);
                }
            }
            SyncPlayers = true;
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

            thread.Abort();
            thread.Join(500);
            thread = null;

            udpClient = new UdpClient();
            udpClient.Connect(endPoint);
        }
    }
}
