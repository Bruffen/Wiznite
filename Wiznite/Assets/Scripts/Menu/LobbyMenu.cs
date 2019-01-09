using Common;
using System.Collections;
using System.Collections.Generic;
using UdpNetwork;
using UnityEngine;
using UnityEngine.UI;

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
                            PlayerCanvas[0].transform.GetChild(0).GetChild(1).gameObject.GetComponent<Text>().text = p.Player.Name;
                            if (p.Player.GameState == GameState.LobbyReady)
                                SetReady(PlayerCanvas[0].transform, true);
                            else
                                SetReady(PlayerCanvas[0].transform, false);
                            break;
                        case 1:
                            PlayerCanvas[1].SetActive(true);
                            PlayerCanvas[1].transform.GetChild(0).GetChild(1).gameObject.GetComponent<Text>().text = p.Player.Name;
                            if (p.Player.GameState == GameState.LobbyReady)
                                SetReady(PlayerCanvas[1].transform, true);
                            else
                                SetReady(PlayerCanvas[1].transform, false);
                            break;
                        case 2:
                            PlayerCanvas[2].SetActive(true);
                            PlayerCanvas[2].transform.GetChild(0).GetChild(1).gameObject.GetComponent<Text>().text = p.Player.Name;
                            if (p.Player.GameState == GameState.LobbyReady)
                                SetReady(PlayerCanvas[2].transform, true);
                            else
                                SetReady(PlayerCanvas[2].transform, false);
                            break;
                        case 3:
                            PlayerCanvas[3].SetActive(true);
                            PlayerCanvas[3].transform.GetChild(0).GetChild(1).gameObject.GetComponent<Text>().text = p.Player.Name;
                            if (p.Player.GameState == GameState.LobbyReady)
                                SetReady(PlayerCanvas[3].transform, true);
                            else
                                SetReady(PlayerCanvas[3].transform, false);
                            break;
                    }
                }

                client.SyncPlayers = false;
            }
        }

        private void SetReady(Transform t, bool isReady)
        {
            t.GetChild(2).GetChild(1).gameObject.SetActive(isReady);
            t.GetChild(2).GetChild(2).gameObject.SetActive(!isReady);
        }

        public void KillLobbyThread()
        {
            client.CloseThread();
        }

        private void OnDestroy()
        {
            KillLobbyThread();
        }

        public void GetReady()
        {
            client.SendPlayerReadyMessage();
        }
    }
}
