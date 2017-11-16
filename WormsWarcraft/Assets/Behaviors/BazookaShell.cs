using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Rigidbody2D))]
public class BazookaShell : NetworkBehaviour
{
    [SerializeField] [SyncVar] public NetworkInstanceId spawnedBy;
    [SerializeField] [SyncVar] public Vector3 initialVelocity;

    private new Rigidbody2D rigidbody2D;
    
    public override void OnStartClient()
    {
        GameObject obj = ClientScene.FindLocalObject(this.spawnedBy);
        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), obj.GetComponent<Collider2D>());

        if (this.rigidbody2D == null) this.rigidbody2D = this.GetComponent<Rigidbody2D>();
        this.rigidbody2D.velocity = initialVelocity;
    }

    private void Update()
    {
        var forward = this.rigidbody2D.velocity.normalized;
        this.transform.rotation = Quaternion.Euler(0, 0, (Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg));
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!this.isServer) return;

        var combat = collision.gameObject.GetComponent<Combat>();
        if (combat != null) combat.TakeDamage(30, this.gameObject);
        Destroy(this.gameObject);
    }
}
