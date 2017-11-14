using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Combat))]
public class PlayerAvatar : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 4;
    [SerializeField] private float fireDelay = .5f;

    [SerializeField] [SyncVar] public int index = -1;
    [SerializeField] [SyncVar] public GameObject hudGobj;

    [SerializeField] public bool isAlive = true;

    [SerializeField] public GameObject bazookaShellPrefab;

    private Combat combat;

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

    private void Start()
    {
        if (!this.isServer) return;
        combat = GetComponent<Combat>();
        combat.Kill += onKill;
    }

    private void onKill(object sender, EventArgs e)
    {
        this.isAlive = false;
        this.RpcKill();
    }
    [ClientRpc]
    public void RpcKill()
    {
        this.isAlive = false;
        var hud = this.playerHud;
        if (hud == null || !hud.isLocalPlayer || !this.isSelected) return;
        this.GetComponent<Combat>().OnKill();
        hud.SelectNextAliveAvatar();
    }

    private float nextFireDelay = .5f;
    void Update()
    {
        if (!this.isAlive) return;
        this.nextFireDelay -= Time.deltaTime;

        var hud = this.playerHud;
        if (hud == null) return;
        if (this.index >= 0 && this.index < hud.avatars.Length) hud.avatars[this.index] = this;
        if (!hud.isLocalPlayer || !this.isSelected) return;

        var xchange = Input.GetAxis("MoveHorizontal") * Time.deltaTime * this.moveSpeed;
        var ychange = Input.GetAxis("AimVertical") * Time.deltaTime * this.moveSpeed;
        this.transform.Translate(xchange, ychange, 0);

        var fire = Input.GetButton("Fire");
        if (fire && this.nextFireDelay <= 0)
        {
            this.CmdFire(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            this.nextFireDelay = this.fireDelay;
        }
    }
    
    [Command]
    public void CmdFire(Vector2 target)
    {
        if (this.nextFireDelay > 0) return;
        var forward = (new Vector3(target.x, target.y, 0) - this.transform.position).normalized;
        var bazookaShellGobj = Instantiate(
            this.bazookaShellPrefab,
            this.transform.position,
            Quaternion.Euler(0, 0, (Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg))
        );
        NetworkServer.Spawn(bazookaShellGobj);
        bazookaShellGobj.GetComponent<Rigidbody2D>().velocity = forward * 4;
        Physics2D.IgnoreCollision(this.GetComponent<Collider2D>(), bazookaShellGobj.GetComponent<Collider2D>());
        bazookaShellGobj.GetComponent<BazookaShell>().ignoreCollision = this.gameObject;
        Destroy(bazookaShellGobj, 2.0f);
    }
}
