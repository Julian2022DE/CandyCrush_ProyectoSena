using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int indiceX;
    public int indiceY;

    private void Start()
    {
        
    }

    public void Inicializar(int cambioX, int cambioY)
    {
        indiceX = cambioX;
        indiceY = cambioY;
    }
}
