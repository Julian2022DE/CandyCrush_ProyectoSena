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
    [Range(0f, 0.5f)] public float swaptime = 3f;

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
                LlenarMatrizAleatoria(i, j);
            }
        }
        bool estallena = false;
        int interaciones = 0;
        int interaccionesmaximas = 100;
        while(!estallena)
        {
            List<PiezasDeJuego> coincidencias = EncontrarTodasLasCoincidencias();
            if(coincidencias.Count == 0)
            {
                estallena = true;
                break;
            }
            else
            {
                RemplazarConPiezasAleatorias(coincidencias);
            }
            if(interaciones > interaccionesmaximas)
            {
                estallena = true;
                Debug.LogWarning("se alcanzo el numero maximo de interacciones");
            }
            interaciones++;
        }

    }

    private void RemplazarConPiezasAleatorias(List<PiezasDeJuego> coincidencias)
    {
        foreach(PiezasDeJuego gamepiece in coincidencias)
        {
            ClearPiecesat(gamepiece.cordenadax, gamepiece.cordenaday);
            LlenarMatrizAleatoria(gamepiece.cordenadax, gamepiece.cordenaday);
        }
    }

    void LlenarMatrizAleatoria(int X, int Y)
    {
        GameObject go = PiezaAleatoria();
        Piezaposicion(go.GetComponent<PiezasDeJuego>(), X, Y);
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
        if (tileinicial != null && EsVecino(tileinicial, final) == true)
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
        StartCoroutine(Swichtilecourutine(inicial, final));
    }

    IEnumerator Swichtilecourutine(Tile inicial, Tile final)
    {
        PiezasDeJuego gpinicial = piezas[inicial.indiceX, inicial.indiceY];
        PiezasDeJuego gpfinal = piezas[final.indiceX, final.indiceY];

        if(gpinicial != null && gpfinal != null)
        {
            gpinicial.Movepieces(final.indiceX, final.indiceY, swaptime);
            gpfinal.Movepieces(inicial.indiceX, inicial.indiceY, swaptime);

            yield return new WaitForSeconds(swaptime);

            List<PiezasDeJuego> Coincidencias_ini = EncontratCoincidenciasEn(inicial.indiceX, inicial.indiceY);
            List<PiezasDeJuego> Coincidencias_fin = EncontratCoincidenciasEn(final.indiceX, final.indiceY);


            if (Coincidencias_ini.Count == 0 && Coincidencias_fin.Count == 0)
            {
                gpinicial.Movepieces(inicial.indiceX, inicial.indiceY, swaptime);
                gpfinal.Movepieces(final.indiceX, final.indiceY, swaptime);
            }
            /*ResaltarPiezasEn(gpinicial.cordenadax, gpinicial.cordenaday);
            ResaltarPiezasEn(gpfinal.cordenadax, gpfinal.cordenaday);*/

            ClearPiecesat(Coincidencias_ini);
            ClearPiecesat(Coincidencias_fin);
        }
            
    }
    public bool EsVecino(Tile _Inicial, Tile _Final)
    {
        if (Mathf.Abs(_Inicial.indiceX - _Final.indiceX) == 1 && _Inicial.indiceY == _Final.indiceY)
        {
            return true;
        }
        if (Mathf.Abs(_Inicial.indiceY - _Final.indiceY) == 1 && _Inicial.indiceX == _Final.indiceX)
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
            siguientey = starty + (int)Mathf.Clamp(direcciondelbuscado.y, -1, 1) * i;

            if(!EstaEnRango(siguientex, siguientey))
            {
                break;
            }
            PiezasDeJuego siguientepieza = piezas[siguientex, siguientey];
            if(siguientepieza == null)
            {
                break;
            }
            else
            {
                if (piezainicial.ficha == siguientepieza.ficha && !coincidencias.Contains(siguientepieza))
                {
                    coincidencias.Add(siguientepieza);
                }
                else
                {
                    break;
                }
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
        List<PiezasDeJuego> abajo = EncontrarCoincidencias(startX, startY, Vector2.down, 2);

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
        List<PiezasDeJuego> derecha = EncontrarCoincidencias(startX, startY, Vector2.right, 2);
        List<PiezasDeJuego> izquierda = EncontrarCoincidencias(startX, startY, Vector2.left, 2);

        if (derecha == null)
        {
            derecha = new List<PiezasDeJuego>();
        }
        if (izquierda == null)
        {
            izquierda = new List<PiezasDeJuego>();
        }
        var listascombinadas = derecha.Union(izquierda).ToList();
        return listascombinadas.Count >= cantidadminima ? listascombinadas : null;
    }
    public void resaltar()
    {
        for (int i = 0; i < ancho; i++)
        {
            for (int j = 0; j < alto; j++)
            {
                 EncontratCoincidenciasEn(i, j);
            }
        }
    }

    void ResaltarPiezasEn(int _X, int _Y)
    {
        var listascombinadas = EncontratCoincidenciasEn(_X, _Y);
        if(listascombinadas.Count > 0)
        {
            foreach(PiezasDeJuego p in listascombinadas)
            {
                ResaltarTile(p.cordenadax, p.cordenaday, p.GetComponent<SpriteRenderer>().color);
            }
        }
    }
    
    private List<PiezasDeJuego> EncontratCoincidenciasEn(int i, int j)
    {
        List<PiezasDeJuego> horizontal = BusquedaHorizontal(i, j, 3);
        List<PiezasDeJuego> Vertical = BusquedaVertical(i, j, 3);
        if (horizontal == null)
        {
            horizontal = new List<PiezasDeJuego>();
        }
        if (Vertical == null)
        {
            Vertical = new List<PiezasDeJuego>();
        }
        var listascombinadas = horizontal.Union(Vertical).ToList();
        return listascombinadas;
    }

    private List<PiezasDeJuego> EncontrarTodasLasCoincidencias()
    {
        List<PiezasDeJuego> todasLasCoincidencias = new List<PiezasDeJuego>();
        for (int i = 0; i < ancho; i++)   
        {
            for (int j = 0; j < alto; j++)
            {
                var coincidencias = EncontratCoincidenciasEn(i, j);
                todasLasCoincidencias = todasLasCoincidencias.Union(coincidencias).ToList();
            }
        }
        return todasLasCoincidencias;
    }

    public void ResaltarTile(int _X, int _Y, Color color)
    {
        SpriteRenderer sr = board[_X, _Y].GetComponent<SpriteRenderer>();
        sr.color = color;
    }
   
    void ClearBoard()
    {
        for(int i = 0; i < ancho; i++)
        { 
            for (int j = 0; j < alto; j++)
            {
                ClearPiecesat(i,j);
            }
        }
    }

    private void ClearPiecesat(int x, int y)

    {
        PiezasDeJuego placetoclear = piezas[x, y];
        if(placetoclear != null)
        {
            piezas[x, y] = null;
            Destroy(placetoclear.gameObject);
        }

    }
    private void ClearPiecesat(List<PiezasDeJuego> gamepieces)
    {
        foreach(PiezasDeJuego go in gamepieces)
        {
            ClearPiecesat(go.cordenadax, go.cordenaday);
        }
    }
} 

