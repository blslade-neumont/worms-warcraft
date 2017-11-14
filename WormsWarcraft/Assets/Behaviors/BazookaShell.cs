using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Rigidbody2D))]
public class BazookaShell : NetworkBehaviour
{
    public GameObject ignoreCollision;

    private new Rigidbody2D rigidbody2D;

    private void Start()
    {
        this.rigidbody2D = this.GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        var forward = this.rigidbody2D.velocity.normalized;
        this.transform.rotation = Quaternion.Euler(0, 0, (Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg));
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!this.isServer) return;
        if (collision.gameObject == this.ignoreCollision) return;

        var combat = collision.gameObject.GetComponent<Combat>();
        combat.TakeDamage(30, this.gameObject);
        Destroy(this.gameObject);
    }
}
