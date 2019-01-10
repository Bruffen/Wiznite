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

    public GameObject parentMain, parentSlave;
    public List<Transform> spawners;

    UdpClientController udp = ClientInformation.UdpClientController;

    private void Start()
    {
        udp.Player.GameState = GameState.GameSync;
        foreach (var udp_tmp in udp.GetLobbyPlayers())
        {
            if (udp_tmp.Player.Id.Equals(udp.Player.Id))
            {
                udp_tmp.gameObject = InstantiatePlayer(udp_tmp.Player);
            }
            else
            {
                udp_tmp.gameObject = InstantiateSlave(udp_tmp.Player);
            }
        }
    }

    void Update()
    {
        if (timePassed > TimePerRound)
        {
            //RestartScene();
        }
        else
            timePassed += Time.deltaTime;
    }

    GameObject InstantiatePlayer(Player p)
    {
        return Instantiate(parentMain, spawners[p.LobbyPos].position, Quaternion.identity);
    }

    GameObject InstantiateSlave(Player p)
    {
        return Instantiate(parentSlave, spawners[p.LobbyPos].position, Quaternion.identity);
    }

    void RestartScene()
    {
        roundEnd = true;
        SceneManager.LoadScene(SceneNumber);
    }

    void OnDestroy()
    {
        //if (!roundEnd)
        udp.CloseThread();
    }
}
