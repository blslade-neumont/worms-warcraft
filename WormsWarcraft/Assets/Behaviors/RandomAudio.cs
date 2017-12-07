using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Random = System.Random;

[RequireComponent(typeof(AudioSource))]
public class RandomAudio : MonoBehaviour
{
    [SerializeField] private AudioClip[] clips;

    [SerializeField] public AudioSource audioSource;

    void Awake()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        var rnd = new Random();
        if (clips.Length > 0) audioSource.clip = clips[rnd.Next(clips.Length)];
        audioSource.Play();
        Destroy(this);
    }
}
