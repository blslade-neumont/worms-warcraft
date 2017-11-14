using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatBounce : MonoBehaviour
{
    [SerializeField] public float bounceMax = .5f;
    [SerializeField] public float bounceMin = 0;
    [SerializeField] public float cycleDuration = 2;

    [SerializeField] private AvatarPointer avatarPointer;

    private float currentTime = 0;

    private PlayerHUD playerHud
    {
        get
        {
            if (this.avatarPointer == null) return null;
            if (this.avatarPointer.hudGobj == null) return null;
            return this.avatarPointer.hudGobj.GetComponent<PlayerHUD>();
        }
    }
    
    void Update()
    {
        var hud = this.playerHud;
        if (hud == null || !hud.isLocalPlayer) return;

        this.updatePosition(Time.deltaTime);
    }
    
    private void updatePosition(float timeDelta)
    {
        this.currentTime += timeDelta;
        var cycleTime = ((this.currentTime % this.cycleDuration) / this.cycleDuration) * (2 * Mathf.PI);
        var posY = bounceMin + (bounceMax - bounceMin) * ((Mathf.Cos(cycleTime) + 1) / 2);
        this.transform.localPosition = new Vector3(this.transform.localPosition.x, posY, this.transform.localPosition.z);
    }
}
