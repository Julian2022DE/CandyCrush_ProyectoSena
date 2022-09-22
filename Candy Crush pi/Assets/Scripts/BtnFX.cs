using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BtnFX : MonoBehaviour
{
    public AudioSource Source;
    public AudioClip hoverFx;
    public AudioClip clickFx;

    public void HoverSound()
    {
        Source.PlayOneShot(hoverFx);
    }
    public void ClickSound()
    {
        Source.PlayOneShot(clickFx);
    }
}
