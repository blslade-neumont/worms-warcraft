using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleController : MonoBehaviour
{
    [SerializeField] public float flashRate = 1;
    [SerializeField] public float visibilityPercentage = 0.8f;

    [SerializeField] public Text text;

    [SerializeField] public string sceneName;

    private float aliveTime = 0;
    
    private void Start()
    {
        if (this.text == null) this.text = this.GetComponent<Text>();
    }

    private void Update()
    {
        this.aliveTime += Time.deltaTime;
        var isVisible = ((this.aliveTime / this.flashRate) % 1) < this.visibilityPercentage;
        if (this.text != null) this.text.enabled = isVisible;

        if (Input.anyKey && !string.IsNullOrEmpty(this.sceneName)) SceneManager.LoadScene(this.sceneName);
    }
}
