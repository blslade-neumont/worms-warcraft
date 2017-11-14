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
        this.combat.Kill += Combat_Kill;
    }

    private void OnDestroy()
    {
        this.combat.Kill -= Combat_Kill;
    }
    private void Combat_Kill(object sender, EventArgs e)
    {
        Destroy(this.gameObject);
    }
}
