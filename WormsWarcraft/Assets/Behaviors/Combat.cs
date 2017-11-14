using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Combat : NetworkBehaviour
{
    [SerializeField] [SyncVar] public float health = 100;
    [SerializeField] public float maxHealth = 100;

    public bool TakeDamage(float damageAmount, object source)
    {
        if (!this.isServer) return false;
        if (this.health > 0)
        {
            var newHealth = this.health - damageAmount;
            if (newHealth <= 0) newHealth = 0;
            this.health = newHealth;
            if (newHealth == 0) this.OnKill();
        }
        return true;
    }

    protected void OnKill()
    {
        var kill = this.Kill;
        if (kill != null) kill(this, EventArgs.Empty);
    }
    public event EventHandler Kill;
}
