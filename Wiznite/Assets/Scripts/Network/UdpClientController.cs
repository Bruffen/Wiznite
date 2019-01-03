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
        private IPEndPoint endPoint;
        private Thread thread;
        private ThreadManager threadManager;

        public UdpClientController()
        {
            int port = 7777;
            udpClient = new UdpClient();
            endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
            udpClient.Connect(endPoint);
            threadManager = ScriptableObject.CreateInstance<ThreadManager>();
        }

        public void CreatePlayer(string name)
        {
            Player = new Player();
            Player.UdpClient = udpClient;
            Player.Name = name;
            Player.GameState = GameState.LobbyDisconnected;
            SendPlayerMessage();

            byte[] answer = udpClient.Receive(ref endPoint);
            string answerJson = Encoding.ASCII.GetString(answer);
            Player answerPlayer = JsonConvert.DeserializeObject<Player>(answerJson);
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

        private void Listen()
        {
            while (threadManager.Running)
            {
                Debug.Log("Thread Started");
                byte[] answer = udpClient.Receive(ref endPoint);
                string answerJson = Encoding.ASCII.GetString(answer);
                try
                {
                    Message message = JsonConvert.DeserializeObject<Message>(answerJson);

                    Player.Messages.Add(message);
                    Debug.Log(message.ToString());
                }
                catch (System.Exception e)
                {
                    Debug.Log(e);
                    throw;
                }
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


            byte[] answer = udpClient.Receive(ref endPoint);
            string answerJson = Encoding.ASCII.GetString(answer);
            Player answerPlayer = JsonConvert.DeserializeObject<Player>(answerJson);

            Player.Lobby.Id = answerPlayer.Lobby.Id;

            thread = new Thread(new ThreadStart(Listen));
            thread.Start();
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

            byte[] answer = udpClient.Receive(ref endPoint);
            string answerJson = Encoding.ASCII.GetString(answer);
            Player answerPlayer = JsonConvert.DeserializeObject<Player>(answerJson);

            if (answerPlayer != null)
            {
                Player.GameState = answerPlayer.GameState;
                Player.Lobby = answerPlayer.Lobby;
                return true;
            }
            return false;
        }
    }

    public class ThreadManager : ScriptableObject
    {
        public bool Running;

        void Start()
        {
            Running = true;
        }

        void OnDestroy()
        {
            Running = false;
        }
    }
}
