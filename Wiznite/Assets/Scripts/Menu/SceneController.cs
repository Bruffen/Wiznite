﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Menu
{
    public class SceneController : MonoBehaviour
    {
        public void LoadMenu()
        {
            SceneManager.LoadScene(0);
        }

        public void LoadLobby()
        {
            SceneManager.LoadScene(1);
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}