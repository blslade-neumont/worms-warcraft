using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TempPlayer : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 4;

    void Start()
    {
        
    }
    
    void Update()
    {
        if (!this.isLocalPlayer) return;

        var xchange = Input.GetAxis("Horizontal") * Time.deltaTime * this.moveSpeed;
        var ychange = Input.GetAxis("Vertical") * Time.deltaTime * this.moveSpeed;
        this.transform.Translate(xchange, ychange, 0);
    }
}
