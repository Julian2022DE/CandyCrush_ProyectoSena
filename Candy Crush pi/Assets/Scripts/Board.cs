using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
    public Tile tilefinal;
    private void Start()
    {
        Creatvoard();
        Organizadorcamara();
        LlenarAleatorio();
    }
    void Creatvoard()
    {
        board = new Tile[ancho, alto];
        for (int i = 0; i < ancho; i++)  //i == x , j == y
        {
            for (int j = 0; j < alto; j++)
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
        cam.transform.position = new Vector3(((float)ancho / 2) - 0.5f, ((float)alto / 2) - 0.5f, -10);
        float alt = (((float)alto / 2) + margen);
        float an = ((((float)ancho / 2) + margen) / Screen.width * Screen.height);
        if (alt > an)
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
        go.GetComponent<PiezasDeJuego>().board = this;
        return go;
    }
    public void Piezaposicion(PiezasDeJuego go, int x, int y)
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

        tileinicial = null;
        tilefinal = null;
        }
    }
    public void SwichPieces(Tile inicial, Tile final)
    {
        PiezasDeJuego gpinicial = piezas[inicial.indiceX, inicial.indiceY];
        PiezasDeJuego gpfinal = piezas[final.indiceX, final.indiceY];


        gpinicial.Movepieces(final.indiceX, final.indiceY, 0.5f);
        gpfinal.Movepieces(inicial.indiceX, inicial.indiceY, 0.5f);

    }
    public bool EsVecino(Tile _Inicial, Tile _Final)
    {
        if (Mathf.Abs(_Inicial.indiceX - _Final.indiceY) == 1 && _Inicial.indiceY == _Final.indiceX)
        {
            return true;
        }
        if (Mathf.Abs(_Inicial.indiceY - _Final.indiceX) == 1 && _Inicial.indiceX == _Final.indiceY)
        {
            return true;
        }
        return false;
    }
    bool EstaEnRango(int _x, int _y)
    {
        return (_x < ancho && _x >= 0 && _y >= 0 && _y < alto);
    }
    List<PiezasDeJuego> EncontrarCoincidencias(int startx, int starty, Vector2 direcciondelbuscado, int cantidaddepiezas = 3)
    {
        //Crear una lista que coincide encontradas
        List<PiezasDeJuego> coincidencias = new List<PiezasDeJuego>();

        //crear referencia al gamepiece inicial 
        PiezasDeJuego piezainicial = null;
        if(EstaEnRango(startx, starty))
        {
            piezainicial = piezas[startx, starty];
        }
        if(piezainicial != null)
        {
            coincidencias.Add(piezainicial);
        }
        else
        {
            return null;
        }
        int siguientex;
        int siguientey;
        int valormaximo = ancho > alto ? ancho : alto;

        for(int i = 1; i < valormaximo -1; i++)
        {
            siguientex = startx + (int)Mathf.Clamp(direcciondelbuscado.x, -1, 1) * i;
            siguientey = starty + (int)Mathf.Clamp(direcciondelbuscado.x, -1, 1) * i;

            if(!EstaEnRango(siguientex, siguientey))
            {
                break;
            }
            PiezasDeJuego siguientepieza = piezas[siguientex, siguientey];
            if (piezainicial.ficha == siguientepieza.ficha && !coincidencias.Contains(siguientepieza))
            {
                coincidencias.Add(siguientepieza);
            }
            else
            {
                break;
            }
        }
        if(coincidencias.Count >= cantidaddepiezas)
        {
            return coincidencias;
        }
        return null;
    }
    List<PiezasDeJuego> BusquedaVertical(int startX, int startY, int cantidadminima = 3)
    {
        List<PiezasDeJuego> arriba = EncontrarCoincidencias(startX, startY, Vector2.up, 2);
        List<PiezasDeJuego> abajo = EncontrarCoincidencias(startX, startY, Vector2.up, 2);

        if(arriba == null)
        {
            arriba = new List<PiezasDeJuego>();
        }
        if(abajo == null)
        {
            abajo = new List<PiezasDeJuego>();
        }
        var listascombinadas = arriba.Union(abajo).ToList();
        return listascombinadas.Count >= cantidadminima ? listascombinadas : null;
    }
    List<PiezasDeJuego> BusquedaHorizontal (int startX, int startY, int cantidadminima = 3)
    {
        List<PiezasDeJuego> izquiera = EncontrarCoincidencias(startX, startY, Vector2.up, 2);
        List<PiezasDeJuego> derecha = EncontrarCoincidencias(startX, startY, Vector2.up, 2);

        if (izquiera == null)
        {
            izquiera = new List<PiezasDeJuego>();
        }
        if (derecha == null)
        {
            derecha = new List<PiezasDeJuego>();
        }
        var listascombinadas = izquiera.Union(derecha).ToList();
        return listascombinadas.Count >= cantidadminima ? listascombinadas : null;
    }
} 
