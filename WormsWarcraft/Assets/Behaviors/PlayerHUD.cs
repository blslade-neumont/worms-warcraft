using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
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

    [SerializeField] [SyncVar] public int teamIdx = 0;

    public override void OnStartLocalPlayer()
    {
        this.CmdSpawnAvatars();
    }

    private bool hasSpawnedAvatars = false;
    [Command]
    public void CmdSpawnAvatars()
    {
        hasSpawnedAvatars = true;
        var avatarPointerGobj = Instantiate(
            avatarPointerPrefab,
            Vector3.zero,
            Quaternion.identity
        );
        NetworkServer.SpawnWithClientAuthority(avatarPointerGobj, this.gameObject);
        avatarPointerGobj.GetComponent<AvatarPointer>().hudGobj = this.gameObject;
        
        for (var q = 0; q < 4; q++)
        {
            var spawnPos = this.avatarSpawnPositions != null && this.avatarSpawnPositions.Length > 0 ? this.avatarSpawnPositions[q % this.avatarSpawnPositions.Length].position : Vector3.zero;
            var avatarGobj = Instantiate(avatarPrefab, spawnPos, Quaternion.identity);
            NetworkServer.SpawnWithClientAuthority(avatarGobj, this.gameObject);
            avatars[q] = avatarGobj.GetComponent<PlayerAvatar>();
            avatars[q].hudGobj = this.gameObject;
            avatars[q].index = q;
        }
        this.CmdSelectAvatar(0, true);
    }

    private Transform[] avatarSpawnPositions;
    public void SetSpawnPositions(Transform[] transforms)
    {
        if (!this.isServer) throw new NotSupportedException("Don't call PlayerHUD.SetSpawnPositions on the client!");

        this.avatarSpawnPositions = transforms;
        if (hasSpawnedAvatars && transforms.Length > 0)
        {
            for (var q = 0; q < this.avatars.Length; q++)
            {
                var avatar = this.avatars[q];
                avatar.transform.position = transforms[q % transforms.Length].position;
            }
        }
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
