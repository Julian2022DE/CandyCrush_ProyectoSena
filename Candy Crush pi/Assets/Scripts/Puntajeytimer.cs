using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class Puntajeytimer : MonoBehaviour
{
    public int min, seg;
    public TMP_Text tiempo;

    private float restante;
    public bool enMarcha;

    private void Awake()
    {
        restante = (min * 60) + seg;
    }
    private void Update()
    {
        if (enMarcha)
        {
            restante -= Time.deltaTime;
            if (restante < 1)
            {

            }

            int tempMin = Mathf.FloorToInt(restante / 60);
            int tempSeg = Mathf.FloorToInt(restante % 60);

            tiempo.text = string.Format("{00:00} : {01:00}", tempMin, tempSeg);
        }
    }
}
