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

        private bool roundStart;

        public bool RoundStart
        {
            get { return roundStart; }
            set { roundStart = value; }
        }

        public bool SyncPlayers;
        public bool IsConnected;
        private Dictionary<Guid, LobbyPlayer> lobbyPlayers;
        public List<LobbyPlayer> GetLobbyPlayers() { return lobbyPlayers.Values.ToList(); }

        private UnityMonoTaskHandler handler;


        public UdpClientController()
        {
            IsConnected = false;
            int port = 7777;
            udpClient = new UdpClient();
            endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
            udpClient.Client.SendTimeout = 5000;
            udpClient.Client.ReceiveTimeout = 5000;
            udpClient.Connect(endPoint);
            SyncPlayers = false;
            roundStart = false;
            handler = new UnityMonoTaskHandler();
        }

        private void ProcessMessage(Message msg)
        {
            switch (msg.MessageType)
            {
                case MessageType.LobbyStatus:
                    Debug.Log("Syncing lobby data");
                    SyncLobby(msg);
                    break;
                case MessageType.GameStart:
                    Debug.Log("SceneLoaded");
                    ChangeState(GameState.GameSync);
                    roundStart = true;
                    break;
                case MessageType.RoundEnd:
                    RoundEnd();
                    break;
                case MessageType.PlayerMovement:
                    Debug.Log("Move you Fuck");
                    ProcessMovement(msg);
                    break;
                case MessageType.PlayerAttack:
                    ProcessAttack(msg);
                    break;
            }
        }

        public void RestartRound()
        {

        }

        public void ProcessMovement(Message m)
        {
            handler.Move(m, Player.Id, lobbyPlayers);
        }

        public void ProcessAttack(Message m)
        {
            PlayerInfo p = JsonConvert.DeserializeObject<PlayerInfo>(m.Description);

            if (p.Id != Player.Id)
            {
                handler.Attack(p.Id, lobbyPlayers);
            }
        }

        public void ChangeState(GameState state)
        {
            Player.GameState = GameState.GameSync;
        }

        public void RoundEnd()
        {
            foreach (var tmp in GetLobbyPlayers())
            {
                if (tmp.Player.numberWins > 3)
                {
                    Player.GameState = GameState.GameEnd;
                    SendPlayerMessageMulticast();
                }
            }
        }

        public void CreatePlayer(string name)
        {
            Player = new Player();
            Player.UdpClient = udpClient;
            Player.Name = name;
            Player.GameState = GameState.LobbyDisconnected;
            Player.Messages = new Queue<Message>();
            try
            {
                SendPlayerMessage();

                Player answerPlayer = ReceivePlayerMessage();
                IsConnected = true;
                Player.Id = answerPlayer.Id;
            }
            catch (Exception) { }
        }

        /*
         * Creates Player Json message and sends it to server
         * It's done a lot so this method is to make things easier and cleaner
         */
        public void SendPlayerMessage()
        {
            string playerJson = JsonConvert.SerializeObject(Player);
            byte[] msg = Encoding.ASCII.GetBytes(playerJson);
            udpClient.Send(msg, msg.Length);
        }

        public void SendPlayerReadyMessage()
        {
            if (Player.GameState == GameState.LobbyReady)
                Player.GameState = GameState.LobbyUnready;
            else
                Player.GameState = GameState.LobbyReady;
            SendPlayerMessageMulticast();
        }

        public void SendPlayerLeaveMessage()
        {
            Player.GameState = GameState.LobbyDisconnecting;
            SendPlayerMessage();
        }

        public void SendMessage(Message msg)
        {
            string msgJson = JsonConvert.SerializeObject(msg);
            byte[] tmp_msg = Encoding.ASCII.GetBytes(msgJson);
            udpClient.Send(tmp_msg, tmp_msg.Length, endPoint);
        }

        public void SendPlayerMessageMulticast()
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
                    if (p.Id == Player.Id)
                    {
                        if (p.GameState.Equals(GameState.GameStarted))
                            Player.GameState = GameState.GameStarted;
                        lobbyPlayers.Add(Player.Id, new LobbyPlayer(Player));
                        Console.WriteLine(lobbyPlayers[p.Id].Player.GameState);
                    }
                    else
                        lobbyPlayers.Add(p.Id, new LobbyPlayer(p));
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
            udpClient.Client.SendTimeout = 5000;
            udpClient.Client.ReceiveTimeout = 5000;
            udpClient.Connect(endPoint);
        }
    }

    public class UnityMonoTaskHandler : MonoBehaviour
    {
        public void Move(Message m, Guid id, Dictionary<Guid, LobbyPlayer> lobbyPlayers)
        {
            PlayerInfo p = JsonConvert.DeserializeObject<PlayerInfo>(m.Description);

            if (p.Id != id)
            {
                GameObject obj = lobbyPlayers[p.Id].gameObject;

                Vector3 pos = new Vector3(p.X, p.Y, p.Z);

                obj.GetComponent<Slave>().GoToPosition(pos);
                Quaternion rot = Quaternion.Euler(p.RotX, p.RotY, p.RotZ);
                obj.transform.rotation = rot;
            }
        }

        public void Attack(Guid id, Dictionary<Guid, LobbyPlayer> lobbyPlayers)
        {
            GameObject obj = lobbyPlayers[id].gameObject;

            obj.GetComponent<Slave>().MakeAttack();

        }


    }

}
