using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerAvatar : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 4;
    [SerializeField] [SyncVar] public int index = -1;
    [SerializeField] [SyncVar] public bool isAlive = true;
    [SerializeField] [SyncVar] public GameObject hudGobj;

    private PlayerHUD playerHud
    {
        get
        {
            if (this.hudGobj == null) return null;
            return this.hudGobj.GetComponent<PlayerHUD>();
        }
    }

    public bool isSelected
    {
        get
        {
            var hud = this.playerHud;
            return hud != null && hud.selectedAvatar != -1 && hud.avatars[hud.selectedAvatar] == this;
        }
    }

    void Update()
    {
        var hud = this.playerHud;
        if (hud == null) return;
        if (this.index >= 0 && this.index < hud.avatars.Length) hud.avatars[this.index] = this;
        if (!hud.isLocalPlayer || !this.isSelected) return;

        var xchange = Input.GetAxis("Horizontal") * Time.deltaTime * this.moveSpeed;
        var ychange = Input.GetAxis("Vertical") * Time.deltaTime * this.moveSpeed;
        this.transform.Translate(xchange, ychange, 0);
    }
}
