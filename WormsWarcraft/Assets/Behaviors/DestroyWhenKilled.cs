using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DestroyWhenKilled : MonoBehaviour
{
    [SerializeField] public Combat combat;

    private void Start()
    {
        if (this.combat == null) this.combat = this.GetComponent<Combat>();
    }

    private void Update()
    {
        if (this.combat.health <= 0) Destroy(this.gameObject);
    }
}
