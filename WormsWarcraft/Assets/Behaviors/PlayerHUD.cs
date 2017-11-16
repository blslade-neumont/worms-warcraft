using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Random = System.Random;
using System.Linq;
using System;

public class PlayerHUD : NetworkBehaviour
{
    public PlayerHUD()
    {
        this.avatars = new PlayerAvatar[4];
    }

    [SerializeField] public int selectedAvatar = -1;

    [NonSerialized] public PlayerAvatar[] avatars;

    [SerializeField] public GameObject avatarPointerPrefab;
    [SerializeField] public GameObject avatarPrefab;

    public override void OnStartLocalPlayer()
    {
        this.CmdSpawnAvatars();
    }
    [Command]
    public void CmdSpawnAvatars()
    {
        var avatarPointerGobj = Instantiate(
            avatarPointerPrefab,
            Vector3.zero,
            Quaternion.identity
        );
        NetworkServer.SpawnWithClientAuthority(avatarPointerGobj, this.gameObject);
        avatarPointerGobj.GetComponent<AvatarPointer>().hudGobj = this.gameObject;

        var rnd = new Random();
        for (var q = 0; q < 4; q++)
        {
            var avatarGobj = Instantiate(
                avatarPrefab,
                new Vector3((float)(rnd.NextDouble() * 4) - 2, (float)(rnd.NextDouble() * 3) + 2.0f, 0),
                Quaternion.identity
            );
            NetworkServer.SpawnWithClientAuthority(avatarGobj, this.gameObject);
            avatars[q] = avatarGobj.GetComponent<PlayerAvatar>();
            avatars[q].hudGobj = this.gameObject;
            avatars[q].index = q;
        }
        this.CmdSelectAvatar(0, true);
    }

    void Update()
    {
        if (!this.isLocalPlayer) return;

        if (Input.GetButtonDown("NextAvatar"))
        {
            var reverse = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            this.SelectNextAliveAvatar(reverse ? -1 : 1);
        }
    }

    public bool SelectNextAliveAvatar(int direction = 1)
    {
        if (this.avatars.Length == 0) return false;
        var nextAvatar = this.selectedAvatar;
        if (nextAvatar == -1) nextAvatar = 0;
        var completedIterations = 0;
        do
        {
            nextAvatar += direction;
            if (nextAvatar < 0) nextAvatar += this.avatars.Length;
            else if (nextAvatar >= this.avatars.Length) nextAvatar -= this.avatars.Length;
            completedIterations++;
        }
        while (!this.avatars[nextAvatar].isAlive && completedIterations < this.avatars.Length);
        this.selectedAvatar = nextAvatar;
        this.CmdSelectAvatar(nextAvatar, false);
        return this.avatars[this.selectedAvatar].isAlive;
    }
    [Command]
    public void CmdSelectAvatar(int selectedAvatar, bool ignoreAuthority)
    {
        this.selectedAvatar = selectedAvatar;
        this.RpcSelectAvatar(selectedAvatar, ignoreAuthority);
    }
    [ClientRpc]
    public void RpcSelectAvatar(int selectedAvatar, bool ignoreAuthority)
    {
        if (!ignoreAuthority && isLocalPlayer) return;
        this.selectedAvatar = selectedAvatar;
    }
}
