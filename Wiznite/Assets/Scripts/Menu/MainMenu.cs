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
        private UdpClientController udp;

        //Player
        private string playerName;
        public Text PlayerTab;

        //Find Lobby

        //Create Lobby
        public Text LobbyName;

        void Start()
        {
            ClientInformation.UdpClientController = new UdpClientController();
            udp = ClientInformation.UdpClientController;
            playerName = PlayerTab.text;
        }

        public void UpdateName(string input)
        {
            playerName = input;
            PlayerTab.text = playerName;
        }

        public void CreateLobby()
        {
            string lobbyName = LobbyName.text == "" ? (playerName + "'s Game") : LobbyName.text;
            ClientInformation.UdpClientController.NewLobby(lobbyName, playerName);
        }
    }
}
