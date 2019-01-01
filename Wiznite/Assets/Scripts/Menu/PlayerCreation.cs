using System.Collections;
using System.Collections.Generic;
using UdpNetwork;
using UnityEngine;
using UnityEngine.UI;

namespace Menu
{
    public class PlayerCreation : MonoBehaviour
    {
        private string playerName;
        public Text PlayerTab;

        void Start()
        {
            ClientInformation.UdpClientController = new UdpClientController();
        }

        public void UpdateName(string input)
        {
            playerName = input;
            PlayerTab.text = playerName;
        }

        public void CreatePlayer()
        {
            playerName = playerName == "" || playerName == null ? "Unnamed Player" : playerName;
            ClientInformation.UdpClientController.CreatePlayer(playerName);
        }
    }
}