using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PiezasDeJuego : MonoBehaviour
{
    public int cordenadax;
    public int cordenaday;

    public void Cordenadas(int x, int y)
    {
        cordenadax = x;
        cordenaday = y;
    }
}
