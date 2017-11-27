using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class GameController : NetworkBehaviour
{
    [SerializeField] public GameObject mapPrefab;

    public override void OnStartServer()
    {
        base.OnStartServer();
        var map = Instantiate(mapPrefab);
        map.transform.position = new Vector3(0, 0, 0);
        NetworkServer.Spawn(map);
    }
}
