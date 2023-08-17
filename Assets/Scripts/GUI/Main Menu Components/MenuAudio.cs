using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuAudio : MonoBehaviour
{
    public AudioSource menuAudioSource;

    public AudioClip mouseClickSound;
    public AudioClip mouseHoverSound;

    public void OnMouseDown()
    {
        menuAudioSource.PlayOneShot(mouseClickSound);
    }

    public void OnMouseEnter()
    {
        menuAudioSource.PlayOneShot(mouseHoverSound);
    }
}
