using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(SpriteRenderer))]
public class Explosion : NetworkBehaviour
{
    [SerializeField] public float radius = 32;
    [SerializeField] public float damageAmount = 30;
    [SerializeField] public float pushbackAmount = 0;

    [SerializeField] public float timeToDestroy = 1;

    private float life = 0;

    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        this.spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        this.life += Time.deltaTime;
        var opacity = Mathf.Max(1 - (this.life / this.timeToDestroy), 0);
        if (opacity <= 0)
        {
            Destroy(this.gameObject);
            return;
        }
        var prevColor = this.spriteRenderer.material.color;
        this.spriteRenderer.material.color = new Color(prevColor.r, prevColor.g, prevColor.b, opacity);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var destructibleTerrain = collision.gameObject.GetComponent<DestructibleTerrain>();
        if (destructibleTerrain != null) destructibleTerrain.ApplyExplosion(this);

        var combat = collision.gameObject.GetComponent<Combat>();
        if (combat != null) combat.TakeDamage(this.damageAmount, this.pushbackAmount, this.gameObject);
    }
}
