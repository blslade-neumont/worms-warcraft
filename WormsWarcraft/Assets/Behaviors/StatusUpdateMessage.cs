using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class StatusUpdateMessage : MonoBehaviour
{
    [SerializeField] public float timeToLive = 2;

    private string _text;
    public string Text
    {
        get
        {
            return this._text;
        }
        set
        {
            this._text = value;
            if (this.uiText) this.uiText.text = value;
        }
    }

    private Text uiText;

    private void Start()
    {
        if (uiText == null)
        {
            uiText = GetComponent<Text>();
            uiText.text = Text;
        }
    }
    private void Update()
    {
        this.timeToLive -= Time.deltaTime;
        if (this.timeToLive <= 0)
        {
            Destroy(this.gameObject);
            return;
        }
        this.transform.Translate(0, (Time.deltaTime * 50) * Mathf.Max(0, timeToLive - .5f), 0);
    }
}
