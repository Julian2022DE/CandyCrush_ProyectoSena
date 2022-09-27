using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class Timer : MonoBehaviour
{
    public int min, seg;
    public TMP_Text tiempo;
    public float restante;
    public bool enMarcha;
    Puntaje puntos;

    private void Awake() //tiempo seleccionable 
    {
        restante = (min * 60) + seg;
    }
    private void Update()
    {
        if (enMarcha)
        {
            restante -= Time.deltaTime;
            if (restante < 1) // en caso de que el timer baje de 1 seg pierda 
            {
                SceneManager.LoadScene("Game over");
            }

            int tempMin = Mathf.FloorToInt(restante / 60);
            int tempSeg = Mathf.FloorToInt(restante % 60);

            tiempo.text = string.Format("{00:00} : {01:00}", tempMin, tempSeg);
            

        }
    }

}
