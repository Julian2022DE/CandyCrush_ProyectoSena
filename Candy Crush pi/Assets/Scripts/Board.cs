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
    public Tile tileinicial;
    public Tile tilefinal;private void Start()
    {
        Creatvoard();
        Organizadorcamara();
        LlenarAleatorio();
    }
    void Creatvoard()
    {
        board = new Tile[ancho,alto];
        for(int i = 0; i < ancho; i++)  //i == x , j == y
        {
            for(int j = 0; j < alto; j++)
            {
                GameObject go = Instantiate(prefile);
                go.name = "tile" + i + "," + j;
                go.transform.position = new Vector3(i, j, 0);
                go.transform.parent = transform;
                Tile tiles = go.GetComponent<Tile>();
                tiles.funciones = this;
                tiles.Inicializar(i, j);
                board[i, j] = tiles;
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
        piezas[x, y] = go;
    }
    void LlenarAleatorio()
    {
        piezas = new PiezasDeJuego[ancho, alto];

        for (int i = 0; i < ancho; i++)
        {
            for (int j = 0; j < alto; j++)
            {
                GameObject go = PiezaAleatoria();
                Piezaposicion(go.GetComponent<PiezasDeJuego>(), i, j);

            }
        }
    }
    public void InicialTile(Tile inicial)
    {
        if (tileinicial == null)
        {
            tileinicial = inicial;
        }
    }
    public void FinalTile(Tile final)
    {
        if (tileinicial != null)
        {
            tilefinal = final;
        }
    }
    public void RealiceTile()
    {
        if (tileinicial != null && tilefinal != null)
        {
            SwichPieces(tileinicial, tilefinal);
        }
    }
    public void SwichPieces(Tile inicial, Tile final)
    {
        PiezasDeJuego gpinicial = piezas[inicial.indiceX, inicial.indiceY];
        PiezasDeJuego gpfinal = piezas[final.indiceX, inicial.indiceY];

        gpinicial.Movepieces(tilefinal.indiceX, tilefinal.indiceY, 0.5f);
        gpfinal.Movepieces(tileinicial.indiceX, tileinicial.indiceY, 0.5f);
    }
}
