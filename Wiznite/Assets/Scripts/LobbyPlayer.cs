using System.Collections;
using System.Collections.Generic;
using Common;
using UnityEngine;

public class LobbyPlayer
{
    public Player Player;
    public GameObject gameObject;

    public LobbyPlayer(Player player)
    {
        Player = player;
        gameObject = MonoBehaviour.Instantiate(GameObject.CreatePrimitive(PrimitiveType.Cube));
    }
}
