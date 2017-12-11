using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Random = System.Random;

[RequireComponent(typeof(Combat))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerAvatar : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 4;
    [SerializeField] private float fireDelay = .5f;
    [SerializeField] private float aimDebounce = 1f;
    [SerializeField] public Transform projectileStartTransform;
    [SerializeField] public Vector2 longJumpVelocity = new Vector2(4, 1.6f);
    [SerializeField] public Transform[] floorRaycastCheckPoints;

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

    [SerializeField] public new Rigidbody2D rigidbody2D;
    [SerializeField] public new Collider2D collider2D;

    public static string[] OrcNames = new string[] {
        "Morbash", "Karthurg", "Ogharod", "Durbag", "Snugug", "Slog", "Pargu", "Yar", "Argug", "Quimghig",
        "Drikdarok", "Kharag", "Olur", "Ogharod", "Farod", "Argug", "Moth", "Turbag", "Filge", "Bugrash",
        "Stugbu", "Aguk", "Umhra", "Urlgan", "Milug", "Vegum", "Marfu", "Alog", "Urag", "Sornaraugh"
    };
    public static string[] HumanNames = new string[] {
        "Carlo", "Masson", "Ramsey", "Gino", "Malcolm", "Kenton", "Neo", "Isaiah", "Dereck", "Adelfo",
        "Raymundo", "Jochim", "Linden", "Derrall", "Cheney", "Dolf", "Glenn", "Tyrell", "Philip", "Iram",
        "Harcourt", "Wesley", "Delray", "Beaman", "Travis", "Regnauld", "Gustavo", "Gunnar", "Alfric", "Lex"
    };

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

    private string _name;
    public string Name
    {
        get
        {
            if (this._name == null)
            {
                var teamIdx = 0;
                var hud = this.playerHud;
                if (hud != null) teamIdx = hud.teamIdx;
                this._name = chooseRandomName(teamIdx);
            }
            return this._name;
        }
    }
    private string chooseRandomName(int teamIdx)
    {
        switch (teamIdx)
        {
        case 0:
            return OrcNames[new Random().Next(OrcNames.Length)];

        case 1:
            return HumanNames[new Random().Next(HumanNames.Length)];

        default:
            goto case 1;
        }
    }

    private RaycastHit2D[] raycastHits;
    private ContactFilter2D floorContactFilter;
    private void Start()
    {
        if (this.spriteRenderer == null) this.spriteRenderer = GetComponent<SpriteRenderer>();
        if (this.rigidbody2D == null) this.rigidbody2D = GetComponent<Rigidbody2D>();
        if (this.collider2D == null) this.collider2D = GetComponent<Collider2D>();

        if (!this.isServer) return;

        combat = GetComponent<Combat>();
        combat.Kill += onKill;

        this.raycastHits = new RaycastHit2D[8];
        this.floorContactFilter = new ContactFilter2D().NoFilter();
    }

    [Command]
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
    private float timeSinceLastMove = 0;
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

        var isFalling = false;
        var isMoving = false;
        var isFiring = false;

        var isOnFloor = false;
        foreach (var checkpoint in this.floorRaycastCheckPoints)
        {
            var hitCount = Physics2D.Raycast(checkpoint.position, Vector2.down, this.floorContactFilter, this.raycastHits, .02f);
            for (int q = 0; q < hitCount; q++)
            {
                var hit = this.raycastHits[q];
                if (hit.collider == this.collider2D) continue;
                if (hit.distance > .02f) continue;
                isOnFloor = true;
                break;
            }
            if (isOnFloor) break;
        }
        if (!isOnFloor) isFalling = true;
        if (isFalling)
        {
            //Debug.Log("Falling!");
            //Don't do anything if you're falling...
        }
        else
        {
            isFiring = Input.GetButton("Fire");
            if (isFiring)
            {
                //Debug.Log("Firing!");
                //Not implemented...
            }
            else
            {
                var xchange = Input.GetAxis("MoveHorizontal") * this.moveSpeed;
                if (xchange != 0) isMoving = true;
                if (xchange > 0) this.setFacingRight(true);
                else if (xchange < 0) this.setFacingRight(false);
                if (isMoving)
                {
                    var moveForce = new Vector2(xchange, Math.Abs(xchange) / 10);
                    this.rigidbody2D.velocity = moveForce;
                }

                var jumpAxis = Input.GetAxis("Jump");
                if (!isMoving && jumpAxis > .5f)
                {
                    isMoving = true;
                    var horiz = this.isFacingRight ? 1 : -1;
                    var jumpVelocity = new Vector2(horiz * this.longJumpVelocity.x, this.longJumpVelocity.y);
                    this.rigidbody2D.velocity = jumpVelocity;
                }
            }
        }

        if (isFalling || isMoving) this.timeSinceLastMove = 0;
        else if (Math.Max(Math.Abs(Input.GetAxis("AimVertical")) - .5, 0) != 0) this.timeSinceLastMove = this.aimDebounce;
        else this.timeSinceLastMove += Time.deltaTime;
        var showAimTarget = isFiring || (!isFalling && !isMoving && this.timeSinceLastMove >= this.aimDebounce);
        var isAiming = showAimTarget && !isFiring;
        //if (isAiming) Debug.Log("Aiming!");

        //var ychange = Input.GetAxis("AimVertical") * Time.deltaTime * this.moveSpeed;
        //this.transform.Translate(xchange, ychange, 0);

        if (this.transform.position.y < 0)
        {
            this.isAlive = false;
            this.CmdDrown();
            return;
        }
        
        if (isFiring && this.nextFireDelay <= 0)
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
