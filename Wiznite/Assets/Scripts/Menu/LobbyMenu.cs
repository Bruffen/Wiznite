using System.Collections;
using System.Collections.Generic;
using UdpNetwork;
using UnityEngine;

namespace Menu
{
    public class LobbyMenu : MonoBehaviour
    {
        public GameObject Player1Canvas, Player2Canvas, Player3Canvas, Player4Canvas;
        private UdpClientController client;

        void Start()
        {
            client = ClientInformation.UdpClientController;
        }

        public void KillLobbyThread()
        {
            client.CloseThread();
        }
    }
}
