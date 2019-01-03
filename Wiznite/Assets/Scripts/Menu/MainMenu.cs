﻿using System.Collections;
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
        public Text PlayerTab;

        //Find Lobby
        public DynamicLobbyList dynamicLobbyList;

        //Create Lobby
        public Text LobbyName;

        void Start()
        {
            udp = ClientInformation.UdpClientController;
            PlayerTab.text = udp.Player.Name;
            //playerName = PlayerTab.text;
        }

        public void UpdateName(string input)
        {
            udp.Player.Name = input;
            PlayerTab.text = input;
            //TODO Send server new name maybe???
        }

        public void CreateLobby()
        {
            string lobbyName = LobbyName.text == "" ? (udp.Player.Name + "'s Game") : LobbyName.text;
            ClientInformation.UdpClientController.NewLobby(lobbyName);
        }

        public void CreateLobbyList()
        {
            dynamicLobbyList.CreateLobbyList(udp.LobbyList());
        }

        public void JoinSelectedLobby()
        {
            if (dynamicLobbyList.LobbySelected != null)
            {
                if (udp.JoinExistingLobby(dynamicLobbyList.LobbySelected.ID))
                {
                    Debug.Log("Joining " + udp.Player.Lobby.Name);
                    GetComponent<SceneController>().LoadLobby();
                }
            }
        }
    }
}