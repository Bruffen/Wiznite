using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Common;
using UdpNetwork;

namespace Menu
{
    public class MainMenu : MonoBehaviour
    {
        private UdpClientController client;

        //Player
        public Text PlayerTab;

        //Find Lobby
        public DynamicLobbyList dynamicLobbyList;

        //Create Lobby
        public Text LobbyName;

        void Start()
        {
            client = ClientInformation.UdpClientController;
            PlayerTab.text = client.Player.Name;
            //playerName = PlayerTab.text;
        }

        public void UpdateName(string input)
        {
            client.Player.Name = input;
            PlayerTab.text = input;
            //TODO Send server new name maybe???
        }

        public void CreateLobby()
        {
            string lobbyName = LobbyName.text == "" ? (client.Player.Name + "'s Game") : LobbyName.text;
            ClientInformation.UdpClientController.NewLobby(lobbyName);
        }

        public void CreateLobbyList()
        {
            dynamicLobbyList.CreateLobbyList(client.LobbyList());
        }

        public void JoinSelectedLobby()
        {
            if (dynamicLobbyList.LobbySelected != null)
            {
                if (client.JoinExistingLobby(dynamicLobbyList.LobbySelected.ID))
                {
                    Debug.Log("Joining " + client.Player.Lobby.Name);
                    GetComponent<SceneController>().LoadMap();
                }
            }
        }
    }
}
