using System.Collections;
using System.Collections.Generic;
using UdpNetwork;
using UnityEngine;

namespace Menu
{
    public class LobbyMenu : MonoBehaviour
    {
        public GameObject[] PlayerCanvas;
        private UdpClientController client;

        void Start()
        {
            client = ClientInformation.UdpClientController;
            client.FetchLobbyData();
        }

        void Update()
        {
            if (client.SyncPlayers)
            {
                foreach (GameObject canvas in PlayerCanvas)
                    canvas.SetActive(false);
                List<LobbyPlayer> players = client.GetLobbyPlayers();
                foreach (LobbyPlayer p in players)
                {
                    switch (p.Player.LobbyPos)
                    {
                        case 0:
                            PlayerCanvas[0].SetActive(true);
                            break;
                        case 1:
                            PlayerCanvas[1].SetActive(true);
                            break;
                        case 2:
                            PlayerCanvas[2].SetActive(true);
                            break;
                        case 3:
                            PlayerCanvas[3].SetActive(true);
                            break;
                    }
                }
            }
        }

        public void KillLobbyThread()
        {
            client.CloseThread();
        }

        private void OnDestroy()
        {
            KillLobbyThread();
        }
    }
}
