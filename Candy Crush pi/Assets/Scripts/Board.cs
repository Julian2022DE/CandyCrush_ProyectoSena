using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public int alto;
    public int ancho;
    public int margen;

    public Tile[,] board;
    public GameObject prefile;
    public Camera cam;
    public GameObject[] prefpuntos;
    public PiezasDeJuego[,] piezas;

    private void Start()
    {
        Creatvoard();
        Organizadorcamara();
        LlenarAleatorio();


    }

    void Creatvoard()
    {
        board = new Tile[alto, ancho];

        for(int i = 0; i < alto; i++)
        {
            for(int j = 0; j < ancho; j++)
            {
                GameObject go = Instantiate(prefile);
                go.name = "tile" + i + "," + j;
                go.transform.position = new Vector3(j, i, 0);
                go.transform.parent = transform;
                Tile tiles = go.GetComponent<Tile>();

                board[i, j] = tiles;

                tiles.Inicializar(i, j);
            }
        }
    }
    void Organizadorcamara()
    {
        cam.transform.position = new Vector3(((float)ancho / 2) -0.5f, ((float) alto / 2) -0.5f, -10);

        float alt = (((float)alto / 2) + margen );
        float an = ((((float)ancho / 2) + margen) / Screen.width * Screen.height);

        if(alt > an)
        {
            cam.orthographicSize = alt;
        }
        else
        {
            cam.orthographicSize = an;
        }
    }

    GameObject PiezaAleatoria()
    {   
        int Numeroaleatorio = Random.Range(0, prefpuntos.Length);
        GameObject go = Instantiate(prefpuntos[Numeroaleatorio]);
        return go;
    }
    void Piezaposicion(PiezasDeJuego go, int x, int y)
    {
        go.transform.position = new Vector3(x, y, 0);
        go.Cordenadas(x, y);
    }
    void LlenarAleatorio()
    {
        piezas = new PiezasDeJuego[ancho, alto];

        for (int i = 0; i < alto; i++)
        {
            for (int j = 0; j < ancho; j++)
            {
                GameObject go = PiezaAleatoria();
                go.transform.position = new Vector3(j, i, 0);

                Piezaposicion(go.GetComponent<PiezasDeJuego>(), j, i);
            }
        }
    }
}
