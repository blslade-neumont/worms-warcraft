using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Rigidbody2D))]
public class BazookaShell : NetworkBehaviour
{
    [SerializeField] [SyncVar] public NetworkInstanceId spawnedBy;
    [SerializeField] [SyncVar] public Vector3 initialVelocity;

    [SerializeField] public GameObject explosionPrefab;

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

    private bool hasCreatedExplosion = false;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Solid")) return;

        if (!this.isServer) return;

        if (!hasCreatedExplosion)
        {
            hasCreatedExplosion = true;
            var explosion = Instantiate(this.explosionPrefab);
            explosion.transform.position = this.transform.position;
            NetworkServer.Spawn(explosion);
            Destroy(this.gameObject);
        }
    }
}
