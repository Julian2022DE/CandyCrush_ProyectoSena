using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int indiceX;
    public int indiceY;
    public Board funciones;
    public AudioSource Source;
    public AudioClip audioFX;
    public GameObject [] pref;
    

    internal void Init(int cambioX, int cambioY)
    {
        indiceX = cambioX;
        indiceY = cambioY;
    }
    public void OnMouseDown()
    {
        funciones.ClickedTile(this);
    }
    public void OnMouseEnter()
    {
        funciones.DragTile(this);
    }
    public void OnMouseUp()
    {
        funciones.RealeaseTile();
        AudioSource.PlayClipAtPoint(audioFX, gameObject.transform.position);
        // al selecionar una ficha suena
    }
}

