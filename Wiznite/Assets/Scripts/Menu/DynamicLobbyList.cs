using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common;
using UnityEngine;
using UnityEngine.UI;

namespace Menu
{
    public class DynamicLobbyList : MonoBehaviour
    {
        public GameObject LobbyInfoPrefab;
        private List<GuidButtonPair> lobbyList;

        public void CreateLobbyList(List<Lobby> lobbies)
        {
            lobbyList = new List<GuidButtonPair>();

            foreach (Lobby lobby in lobbies)
            {
                GameObject btnLobby = Instantiate(LobbyInfoPrefab);
                btnLobby.transform.SetParent(this.transform, false);
                Text[] texts = btnLobby.GetComponentsInChildren<Text>();
                texts[0].text = lobby.Name;
                texts[1].text = lobby.PlayerCount.ToString() + "/4";

                GuidButtonPair guidButton = new GuidButtonPair(lobby.Id, btnLobby);
                btnLobby.GetComponent<Button>().onClick.AddListener(() => SelectLobby(guidButton));
                lobbyList.Add(guidButton);
            }
        }

        private void SelectLobby(GuidButtonPair guidButton)
        {
            Debug.Log("I was clicked: " + guidButton.ID + ", " + guidButton.Button.GetComponentsInChildren<Text>()[0].text);
        }
    }

    public class GuidButtonPair
    {
        public Guid ID;
        public GameObject Button;

        public GuidButtonPair(Guid id, GameObject button)
        {
            ID = id;
            Button = button;
        }
    }
}