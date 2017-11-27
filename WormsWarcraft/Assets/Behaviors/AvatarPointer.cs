using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AvatarPointer : NetworkBehaviour
{
    [SerializeField] [SyncVar] public GameObject hudGobj;

    private PlayerHUD playerHud
    {
        get
        {
            if (this.hudGobj == null) return null;
            return this.hudGobj.GetComponent<PlayerHUD>();
        }
    }

    void LateUpdate()
    {
        var hud = this.playerHud;
        if (hud == null || hud.selectedAvatar < 0 || hud.selectedAvatar >= hud.avatars.Length) return;

        var currentAvatar = hud.avatars[hud.selectedAvatar];
        if (currentAvatar == null) return;
        this.transform.position = currentAvatar.transform.position;

        if (hud.isLocalPlayer)
        {
            var cam = Camera.main;
            var camController = cam.GetComponent<CameraController>();
            if (camController != null) camController.currentFollowObject = this.gameObject;
        }
    }
}
