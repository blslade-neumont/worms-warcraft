using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterLevel : MonoBehaviour
{
    [SerializeField] public float wavelength = 10;
    [SerializeField] public Transform lowPoint;
    [SerializeField] public Transform highPoint;

    private float currentAge = 0;

    private void Update()
    {
        this.currentAge += Time.deltaTime;
        var lerpAmt = (Mathf.Sin(this.currentAge / wavelength) + 1) / 2;
        Debug.Log(lerpAmt);
        this.transform.position = Vector3.Lerp(lowPoint.position, highPoint.position, lerpAmt);
    }
}
