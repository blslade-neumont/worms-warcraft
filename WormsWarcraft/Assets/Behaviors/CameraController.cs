using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [SerializeField] public GameObject currentFollowObject;
    [SerializeField] public Transform minY;

    [SerializeField] public float minZoom = 2f;
    [SerializeField] public float maxZoom = 6f;

    private new Camera camera;
    private float currentZoom = 3;

    private void Start()
    {
        this.camera = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        var zoomChange = -Input.GetAxis("Zoom") / 100;
        this.currentZoom += zoomChange * Time.deltaTime;
        this.currentZoom = Mathf.Clamp(this.currentZoom, minZoom, maxZoom);
        this.camera.orthographicSize = this.currentZoom;

        Vector3 posTo = this.transform.position;
        if (currentFollowObject != null)
        {
            posTo = currentFollowObject.transform.position;
        }

        if (posTo != null)
        {
            var vertExtent = this.camera.orthographicSize;
            var horizExtent = (Screen.width / Screen.height) * vertExtent;
            var minYVal = minY.transform.position.y + (vertExtent / 2);
            if (posTo.y < minYVal) posTo.y = minYVal;
            posTo.z = this.transform.position.z;
            this.transform.position = posTo;
        }
    }
}
