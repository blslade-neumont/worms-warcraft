using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerSpawnPositions : MonoBehaviour
{
    [SerializeField] public Transform rootTransform;

    [SerializeField] public Transform[] team1;
    [SerializeField] public Transform[] team2;
}
