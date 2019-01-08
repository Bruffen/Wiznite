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
        public ThreadManager ThreadManager;
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

            while (ThreadManager.Running)
            {
                byte[] answer = udpClient.Receive(ref multicastEndPoint);
                string answerJson = Encoding.ASCII.GetString(answer);
                try
                {
                    Message message = JsonConvert.DeserializeObject<Message>(answerJson);
                    if (message == null)
                    {
                        Debug.Log("Null message");
                    }
                    else
                    {
                        Player.Messages.Add(message);
                        Debug.Log("Message received: " + message.Description);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.Log(e.StackTrace);
                    throw;
                }
            }

            Debug.Log("Thread Killed");
            //Change back to unicast for menu communication
            udpClient.Close();
            udpClient = new UdpClient();
            udpClient.Connect(endPoint);
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
            if (ThreadManager != null)
                ScriptableObject.Destroy(ThreadManager);
            ThreadManager = ScriptableObject.CreateInstance<ThreadManager>();
            thread = new Thread(new ThreadStart(Listen));
            thread.Start();
        }
    }

    public class ThreadManager : ScriptableObject
    {
        public bool Running;

        void OnEnable()
        {
            Running = true;
        }

        //When game is stopped, kill thread so the game doesn't freeze when it's restarted
        void OnDestroy()
        {
            Running = false;
        }
    }
}
