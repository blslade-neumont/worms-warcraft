using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class WormsWarcraftNetworkManager : NetworkManager
{
    [NonSerialized] public PlayerSpawnPositions spawnPositions;

    private int playerCount = 0;
    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        var playerGobj = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        var player = playerGobj.GetComponent<PlayerHUD>();
        NetworkServer.AddPlayerForConnection(conn, playerGobj, playerControllerId);
        player.teamIdx = playerCount;
        player.SetSpawnPositions(playerCount == 0 ? spawnPositions.team1 : spawnPositions.team2);
        playerCount++;
    }
    public override void OnServerRemovePlayer(NetworkConnection conn, PlayerController player)
    {
        base.OnServerRemovePlayer(conn, player);
        playerCount--;
    }
}
