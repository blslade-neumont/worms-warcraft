using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Combat combat;

    void Awake()
    {
        if (combat == null) combat = GetComponent<Combat>();
    }

    private void Update()
    {
        this.transform.localScale = new Vector3(combat.health / combat.maxHealth, 1, 1);
    }
}
