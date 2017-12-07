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
    [SerializeField] public Transform projectileStartTransform;

    [SerializeField] [SyncVar] public int index = -1;
    [SerializeField] [SyncVar] public GameObject hudGobj;

    [SerializeField] public SpriteRenderer spriteRenderer;
    [SerializeField] public Sprite team0Sprite;
    [SerializeField] public Sprite team1Sprite;
    [SerializeField] public Sprite team0GraveSprite;
    [SerializeField] public Sprite team1GraveSprite;

    [SerializeField] public bool isFacingRight = true;

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
        if (this.spriteRenderer == null) this.spriteRenderer = GetComponent<SpriteRenderer>();

        if (!this.isServer) return;

        combat = GetComponent<Combat>();
        combat.Kill += onKill;
    }

    public void CmdDrown()
    {
        this.onKill(this, EventArgs.Empty);
    }
    private void onKill(object sender, EventArgs e)
    {
        var diedInWater = (!(sender is Combat)) || ((Combat)sender).health > 0;
        this.isAlive = false;
        this.RpcKill(diedInWater);
    }
    [ClientRpc]
    public void RpcKill(bool inWater)
    {
        this.isAlive = false;
        var hud = this.playerHud;
        if (inWater) hud.PlaySplashSound(this);
        else hud.PlayDeathSound(this);
        if (hud == null || !hud.isLocalPlayer || !this.isSelected) return;
        hud.SelectNextAliveAvatar();
    }

    private float nextFireDelay = .5f;
    void Update()
    {
        var hud = this.playerHud;
        if (hud == null) return;

        if (this.spriteRenderer)
        {
            Sprite sprite;
            if (this.isAlive) sprite = hud.teamIdx == 0 ? this.team0Sprite : this.team1Sprite;
            else sprite = hud.teamIdx == 0 ? this.team0GraveSprite : this.team1GraveSprite;
            if (sprite != null)
            {
                this.spriteRenderer.sprite = sprite;
                this.spriteRenderer.flipX = this.isAlive && !this.isFacingRight;
            }
        }

        if (!this.isAlive) return;

        this.nextFireDelay -= Time.deltaTime;

        if (this.index >= 0 && this.index < hud.avatars.Length) hud.avatars[this.index] = this;
        if (!hud.isLocalPlayer || !this.isSelected) return;

        var xchange = Input.GetAxis("MoveHorizontal") * Time.deltaTime * this.moveSpeed;
        var ychange = Input.GetAxis("AimVertical") * Time.deltaTime * this.moveSpeed;
        if (xchange > 0) this.setFacingRight(true);
        else if (xchange < 0) this.setFacingRight(false);
        this.transform.Translate(xchange, ychange, 0);

        if (this.transform.position.y < 0)
        {
            this.isAlive = false;
            this.CmdDrown();
            return;
        }

        var fire = Input.GetButton("Fire");
        //TODO: check if the avatar is on the ground
        if (fire && this.nextFireDelay <= 0)
        {
            this.CmdFire(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            this.nextFireDelay = this.fireDelay;
        }
    }

    private void setFacingRight(bool val)
    {
        if (val == this.isFacingRight) return;
        this.isFacingRight = val;
        this.CmdSetFacingRight(val, false);
    }
    [Command]
    public void CmdSetFacingRight(bool val, bool ignoreAuthority)
    {
        this.isFacingRight = val;
        this.RpcSetFacingRight(val, ignoreAuthority);
    }
    [ClientRpc]
    public void RpcSetFacingRight(bool val, bool ignoreAuthority)
    {
        var hud = this.playerHud;
        if (!hud) return;
        if (!ignoreAuthority && hud.isLocalPlayer) return;
        this.isFacingRight = val;
    }

    [Command]
    public void CmdFire(Vector2 target)
    {
        if (this.nextFireDelay > 0) return;
        var forward = (new Vector3(target.x, target.y, 0) - this.transform.position).normalized;
        var bazookaShellGobj = Instantiate(
            this.bazookaShellPrefab,
            this.projectileStartTransform.position,
            Quaternion.Euler(0, 0, (Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg))
        );
        bazookaShellGobj.GetComponent<Rigidbody2D>().velocity = forward * 4;
        var bazookaShell = bazookaShellGobj.GetComponent<BazookaShell>();
        bazookaShell.spawnedBy = this.netId;
        bazookaShell.initialVelocity = forward * 4;
        Destroy(bazookaShellGobj, 10.0f);
        NetworkServer.Spawn(bazookaShellGobj);
    }
}
