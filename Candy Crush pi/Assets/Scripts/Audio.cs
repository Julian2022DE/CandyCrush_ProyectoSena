using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Audio : MonoBehaviour
{
    public AudioSource source;
    public AudioClip audioFX;



    private void OnMouseUp()
    {
        AudioSource.PlayClipAtPoint(audioFX, gameObject.transform.position);
    }
}

