using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int indiceX;
    public int indiceY;
    public Board funciones;

    private void Start()
    {
        
    }

    public void Inicializar(int cambioX, int cambioY)
    {
        indiceX = cambioX;
        indiceY = cambioY;
    }
    public void OnMouseDown()
    {
        funciones.InicialTile(this);
    }
    public void OnMouseEnter()
    {
        funciones.FinalTile(this);
    }
    public void OnMouseUp()
    {
        funciones.RealiceTile();
    }
}

