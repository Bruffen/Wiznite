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
        private SceneController scnCtrl;

        void Start()
        {
            scnCtrl = GetComponent<SceneController>();
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
                            UpdateCanvas(PlayerCanvas[0], p.Player);
                            break;
                        case 1:
                            UpdateCanvas(PlayerCanvas[1], p.Player);
                            break;
                        case 2:
                            UpdateCanvas(PlayerCanvas[2], p.Player);
                            break;
                        case 3:
                            UpdateCanvas(PlayerCanvas[3], p.Player);
                            break;
                    }
                }

                client.SyncPlayers = false;
            }

            if (client.RoundStart)
            {
                scnCtrl.LoadMap();
            }
        }

        private void UpdateCanvas(GameObject canvas, Player p)
        {
            canvas.SetActive(true);
            canvas.transform.GetChild(0).GetChild(1).gameObject.GetComponent<Text>().text = p.Name;
            if (p.GameState == GameState.LobbyReady || p.GameState == GameState.GameStarted)
                SetReady(canvas.transform, true);
            else
                SetReady(canvas.transform, false);
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
            RemoveClientFromLobby();
        }

        public void GetReady()
        {
            client.SendPlayerReadyMessage();
        }

        public void RemoveClientFromLobby()
        {
            client.SendPlayerLeaveMessage();
        }
    }
}
