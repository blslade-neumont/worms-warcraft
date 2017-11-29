using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class MapSpawner : NetworkBehaviour
{
    [SerializeField] public WormsWarcraftNetworkManager networkManager;
    [SerializeField] public GameObject mapPrefab;
    [SerializeField] public GameObject waterPrefab;

    [NonSerialized] public GameObject mapInstance;
    [NonSerialized] public GameObject waterInstance;
    [NonSerialized] public PlayerSpawnPositions spawnPositions;

    public override void OnStartServer()
    {
        base.OnStartServer();
        
        var map = Instantiate(mapPrefab, Vector3.zero, Quaternion.identity);
        mapInstance = map;
        networkManager.spawnPositions = spawnPositions = mapInstance.GetComponent<PlayerSpawnPositions>();
        NetworkServer.Spawn(map);
        
        var water = Instantiate(waterPrefab, Vector3.zero, Quaternion.identity);
        waterInstance = water;
        NetworkServer.Spawn(water);
    }
}
