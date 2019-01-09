using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UdpNetwork;
using UnityEngine.SceneManagement;
using Common;

public class MapController : MonoBehaviour
{
    public int SceneNumber;
    public float TimePerRound;
    float timePassed;
    bool roundEnd = true;
    UdpClientController udp = ClientInformation.UdpClientController;


    void Update()
    {
        if (timePassed > TimePerRound)
        {
            RestartScene();
        }
        else
            timePassed += Time.deltaTime;
    }

    void InstantiatePlayer(Player p)
    {

    }

    void RestartScene()
    {
        roundEnd = true;
        SceneManager.LoadScene(SceneNumber);
    }

    void OnDestroy()
    {
        if (!roundEnd)
            udp.CloseThread();
    }
}
