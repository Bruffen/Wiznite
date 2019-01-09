using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UdpNetwork;
using UnityEngine;
using UnityEngine.UI;

namespace Menu
{
    public class PlayerCreation : MonoBehaviour
    {
        private string playerName;
        public Text PlayerTab;
        public GameObject FailedConnection;

        void Start()
        {
            ClientInformation.UdpClientController = new UdpClientController();
        }

        public void UpdateName(string input)
        {
            playerName = input;
            PlayerTab.text = playerName;
        }

        void Update()
        {
            if (FailedConnection.activeSelf && Input.GetMouseButton(0))
                FailedConnection.SetActive(false);
        }

        public void CreatePlayer()
        {
            playerName = playerName == "" || playerName == null ? "Unnamed Player" : playerName;
            ClientInformation.UdpClientController.CreatePlayer(playerName);
            if (ClientInformation.UdpClientController.IsConnected)
                GetComponent<SceneController>().LoadMenu();
            else
            {
                FailedConnection.SetActive(true);
                //Thread thread = new Thread(new ThreadStart(DisableFail));
                //thread.Start();
            }
        }

        void DisableFail()
        {
            Debug.Log("Thread start");
            float timer = 0.0f;
            while (timer < 2.0f)
            {
                timer += Time.deltaTime; //deltaTime can only be called in main thread ;-;
            }

            FailedConnection.SetActive(false);
            Debug.Log("Thread finish");
        }
    }
}