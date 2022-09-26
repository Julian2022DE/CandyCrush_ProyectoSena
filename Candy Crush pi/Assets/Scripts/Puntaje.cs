using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;


public class Puntaje : MonoBehaviour
{
    private int puntos;

    private int multiplicador;

    private int puntajeAlmacenado;

    private TextMeshProUGUI TextMesh;
    

    private void Start()
    {
        TextMesh = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        //puntos += Time.deltaTime;
        TextMesh.text = puntos.ToString("0");
    }

    public void SumatoriaPuntos(int puntosEntrada)
    {
        puntos += puntosEntrada;

          /*  if (puntos > 100 && puntos <= 200)
            {
                SceneManager.LoadScene("LVL 2");
            }
            if (puntos > 200 && puntos <= 300)
            {
                SceneManager.LoadScene("LVL 3");
            }
            if (puntos > 300 && puntos <= 400)
            {
                SceneManager.LoadScene("LVL 4");
            }
            if (puntos > 400 && puntos <= 500)
            {
                SceneManager.LoadScene("LVL 5");
            }*/
    }
} 
