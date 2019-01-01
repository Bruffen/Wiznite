using Common;
using Newtonsoft.Json;
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

            byte[] msg = CreatePlayerMessage();
            udpClient.Send(msg, msg.Length);

            byte[] answer = udpClient.Receive(ref endPoint);
            string answerJson = Encoding.ASCII.GetString(answer);
            Player answerPlayer = JsonConvert.DeserializeObject<Player>(answerJson);
        }

        private byte[] CreatePlayerMessage()
        {
            string playerJson = JsonConvert.SerializeObject(Player);
            byte[] msg = Encoding.ASCII.GetBytes(playerJson);

            return msg;
        }

        private void Listen()
        {
            while (threadManager.Running)
            {
                Debug.Log("Thread Started");
                byte[] answer = udpClient.Receive(ref endPoint);
                string answerJson = Encoding.ASCII.GetString(answer);
                Message message = JsonConvert.DeserializeObject<Message>(answerJson);

                Player.Messages.Add(message);
                Debug.Log(message.ToString());
            }
        }

        public void NewLobby(string lobbyName)
        {
            Player.GameState = GameState.LobbyCreation;

            Lobby lobby = new Lobby();
            lobby.Name = lobbyName;
            Player.Lobby = lobby;

            byte[] msg = CreatePlayerMessage();
            udpClient.Send(msg, msg.Length);

            byte[] answer = udpClient.Receive(ref endPoint);
            string answerJson = Encoding.ASCII.GetString(answer);
            Player answerPlayer = JsonConvert.DeserializeObject<Player>(answerJson);

            Player.Lobby.Id = answerPlayer.Lobby.Id;

            thread = new Thread(new ThreadStart(Listen));
            thread.Start();
        }

        public List<Lobby> LobbyList()
        {
            Player.GameState = GameState.LobbiesRequest;

            byte[] msg = CreatePlayerMessage();
            udpClient.Send(msg, msg.Length);

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
