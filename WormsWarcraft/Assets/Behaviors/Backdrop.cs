using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Backdrop : MonoBehaviour
{
    [SerializeField] public Vector3 offsetPosition = Vector3.zero;

    private void LateUpdate()
    {
        this.transform.position = (Camera.main.transform.position / 2) + this.offsetPosition;
    }
}
