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

    public int movimientosNecesarios;
    [SerializeField] private int goalScore;

    public Timer tiempo;

    private TextMeshProUGUI TextMesh;

    private void Start() //inicia en 0 el putaje con el componente textmesh
    {
        TextMesh = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        TextMesh.text = puntos.ToString("Score : 0");
    }

    public void SumatoriaPuntos(int puntosEntrada) //puntaje dentro del texto
    {
        puntos += puntosEntrada;
        if(puntos >= movimientosNecesarios)
        {
            SceneManager.LoadScene("Win");
        }
    }


}
