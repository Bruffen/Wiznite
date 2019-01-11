using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UdpNetwork;
using UnityEngine.SceneManagement;
using Common;
using Newtonsoft.Json;
using UnityEngine.UI;

public class MapController : MonoBehaviour
{
    public int SceneNumber;
    public float TimePerRound;
    float timePassed;
    bool roundEnd = true;

    public GameObject parentMain, parentSlave;
    public List<Transform> spawners;
    public List<Image> HealthBars;

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
            udp_tmp.gameObject.GetComponent<PlayerHealth>().HealthBar = HealthBars[udp_tmp.Player.LobbyPos];
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

        if (udp.SlaveMoved)
        {
            if (udp.CurrentMessage.MessageType.Equals(MessageType.PlayerMovement))
            {
                udp.SlaveMoved = false;
                MoveSlave(udp.CurrentMessage);
            }
        }
        if (udp.SlaveAttacked)
        {
            if (udp.CurrentMessage.MessageType.Equals(MessageType.PlayerAttack))
            {
                Debug.Log("SlaveAttacked");
                AttackSlave(udp.CurrentMessage);
                udp.SlaveAttacked = false;
            }
        }
        if (udp.Hit)
        {
            if (udp.CurrentMessage.MessageType.Equals(MessageType.PlayerHit))
            {
                ProcessHealth(udp.CurrentMessage);
                udp.Hit = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            roundEnd = true;
            ChangePlayerState();
            SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
        }
    }

    public void ChangePlayerState()
    {
        udp.Player.GameState = GameState.LobbyConnecting;
    }

    public void ProcessHealth(Message m)
    {
        PlayerInfo p = JsonConvert.DeserializeObject<PlayerInfo>(m.Description);

        foreach (var tmp in udp.GetLobbyPlayers())
        {
            if (tmp.Player.Id.Equals(p.Id))
            {
                GameObject obj = tmp.gameObject;
                obj.GetComponent<PlayerHealth>().TakeDamage(10);
                //GameObject.Find("In_GameUI").transform.GetChild(tmp.Player.LobbyPos - 1).GetChild(2).GetComponent<Image>().fillAmount = obj.GetComponent<PlayerHealth>().HealthBar.fillAmount;
            }
        }
    }

    void MoveSlave(Message m)
    {
        PlayerInfo p = JsonConvert.DeserializeObject<PlayerInfo>(m.Description);
        foreach (var tmp in udp.GetLobbyPlayers())
        {
            if (tmp.Player.Id != udp.Player.Id && tmp.Player.Id == p.Id)
            {
                GameObject obj = tmp.gameObject;
                Vector3 pos = new Vector3(p.X, p.Y, p.Z);
                obj.GetComponent<Slave>().GoToPosition(pos);
                Quaternion rot = Quaternion.Euler(p.RotX, p.RotY, p.RotZ);
                obj.transform.rotation = rot;
            }
        }
    }

    void AttackSlave(Message m)
    {
        PlayerInfo p = JsonConvert.DeserializeObject<PlayerInfo>(m.Description);

        foreach (var tmp in udp.GetLobbyPlayers())
        {
            if (tmp.Player.Id != udp.Player.Id && tmp.Player.Id == p.Id)
            {
                GameObject obj = tmp.gameObject;
                obj.GetComponent<Slave>().MakeAttack();
            }
        }
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
        if (!roundEnd)
            udp.CloseThread();
    }
}
