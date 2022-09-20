using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Board : MonoBehaviour
{
    public int alto;
    public int ancho;
    public float margen;
    public Tile[,] board;
    public GameObject prefile;
    public Camera cam;
    public GameObject[] prefpuntos;
    public PiezasDeJuego[,] piezas;
    public Tile tileInicial;
    public Tile tileFinal;
    [Range(0f, 0.5f)] public float swapTime = 3f;
    public bool puedeMover = true;
    public AudioSource Source;
    public AudioClip audioFX;
    public AudioClip destryaudio;
    public Animator animaciondestruir;




    private void Start()
    {
        piezas = new PiezasDeJuego[ancho, alto];
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
        go.transform.position = new Vector3(x, y, 0f);
        go.Cordenadas(x, y);

        piezas[x, y] = go;
    }
    void LlenarAleatorio()
    {
        List<PiezasDeJuego> addpieces = new List<PiezasDeJuego>();

        for (int i = 0; i < ancho; i++)
        {
            for (int j = 0; j < alto; j++)
            {
                if(piezas[i,j] == null)
                {
                PiezasDeJuego gamePiece = LlenarMatrizAleatoria(i, j);
                addpieces.Add(gamePiece);
                }
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
                coincidencias = coincidencias.Intersect(addpieces).ToList();    
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
    PiezasDeJuego LlenarMatrizAleatoria(int X, int Y)
    {
        GameObject go = PiezaAleatoria();
        Piezaposicion(go.GetComponent<PiezasDeJuego>(), X, Y);
        return go.GetComponent<PiezasDeJuego>();
    }
    public void InicialTile(Tile inicial)
    {
        if (tileInicial == null)
        {
            tileInicial = inicial;
        }
    }
    public void FinalTile(Tile final)
    {
        if (tileInicial != null && EsVecino(tileInicial, final) == true)
        {
            tileFinal = final;
        }
    }
    public void RealiceTile()
    {
        if (tileInicial != null && tileFinal != null)
        {
            SwichPieces(tileInicial, tileFinal);
            tileInicial = null;
            tileFinal = null;
        }
    }
    public void SwichPieces(Tile inicial, Tile final)
    {
        StartCoroutine(Swichtilecourutine(inicial, final));
    }
    IEnumerator Swichtilecourutine(Tile inicial, Tile final)
    {
        if (puedeMover)
        {
            puedeMover = false;
        
            PiezasDeJuego gpinicial = piezas[inicial.indiceX, inicial.indiceY];
            PiezasDeJuego gpfinal = piezas[final.indiceX, final.indiceY];


            if (gpinicial != null && gpfinal != null)
            {
                gpinicial.Movepieces(final.indiceX, final.indiceY, swapTime);
                gpfinal.Movepieces(inicial.indiceX, inicial.indiceY, swapTime);
               


                yield return new WaitForSeconds(swapTime);

                List<PiezasDeJuego> Coincidencias_ini = EncontratCoincidenciasEn(inicial.indiceX, inicial.indiceY);
                List<PiezasDeJuego> Coincidencias_fin = EncontratCoincidenciasEn(final.indiceX, final.indiceY);

                if (Coincidencias_ini.Count == 0 && Coincidencias_fin.Count == 0)
                {
                    gpinicial.Movepieces(inicial.indiceX, inicial.indiceY, swapTime);
                    gpfinal.Movepieces(final.indiceX, final.indiceY, swapTime);
                    yield return new WaitForSeconds(swapTime);
                }
                else
                {
                    Coincidencias_ini = Coincidencias_ini.Union(Coincidencias_fin).ToList();
                   
                    AudioSource.PlayClipAtPoint(destryaudio, gameObject.transform.position);   // al destruir un match suena       
                    ClearAndRefillBoard(Coincidencias_ini);
                }
            }
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
        if (piezainicial != null)
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
            if (piezas == null)
            {
                animaciondestruir.SetBool("A", true);
            }
        }
        if (Vertical == null)
        {
            Vertical = new List<PiezasDeJuego>();
            if (piezas == null)
            {
                animaciondestruir.SetBool("A", true);
            }
        }
        var listascombinadas = horizontal.Union(Vertical).ToList();
        
        return listascombinadas;
    }
    List<PiezasDeJuego> EncontratCoincidenciasEn(List<PiezasDeJuego> gamepieces, int minimleght = 3)
    {
        List<PiezasDeJuego> matches = new List<PiezasDeJuego>();
        foreach(PiezasDeJuego gp in gamepieces)
        {
            matches = matches.Union(EncontratCoincidenciasEn(gp.cordenadax, gp.cordenaday)).ToList();
           
        }
        return matches;
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
            
            // Se destruye cuando se hace match al caer despues de otro match
            AudioSource.PlayClipAtPoint(destryaudio, gameObject.transform.position);

        }

    }
    private void ClearPiecesat(List<PiezasDeJuego> gamepieces)
    {
        foreach(PiezasDeJuego go in gamepieces)
        {
            if(go != null)
            {
            ClearPiecesat(go.cordenadax, go.cordenaday);
            }
        }
    }
    List<PiezasDeJuego> CollapsarColumnas(int column, float collapsetime = 0.1f)
    {
        List<PiezasDeJuego> movingpieces = new List<PiezasDeJuego>();

        for (int i = 0; i < alto -1; i++)
        {
            if(piezas[column, i] == null)
            {
                for (int j = i + 1; j < alto; j++)
                {
                    if (piezas[column, j] != null)
                    {
                        piezas[column, j].Movepieces(column, i, collapsetime * (j - i));
                        piezas[column, i] = piezas[column, j];
                        piezas[column, j].Cordenadas(column, i);
                        if (!movingpieces.Contains(piezas[column, i]))
                        {
                            movingpieces.Add(piezas[column, i]);
                        }
                        piezas[column, j] = null;
                        break;
                    }
                }
            }
        }
        return movingpieces;
    }
    List<PiezasDeJuego> CollapsarColumnas(List<PiezasDeJuego> gamepieces)
    {
        List<PiezasDeJuego> MovinginPieces = new List<PiezasDeJuego>();
        List<int> columnToCollapse = GetColumn(gamepieces);
        foreach(int column in columnToCollapse)
        {
            MovinginPieces = MovinginPieces.Union(CollapsarColumnas(column)).ToList();

        }
        return MovinginPieces;
    }
    List<int> GetColumn(List<PiezasDeJuego> gamepieces)
    {
        List<int> CollumsIndex = new List<int>();
        foreach  (PiezasDeJuego gamepiece in gamepieces)
        {
            if (!CollumsIndex.Contains(gamepiece.cordenadax)) 
            {
                CollumsIndex.Add(gamepiece.cordenadax);
            }
        }
        return CollumsIndex;
    }
    void ClearAndRefillBoard(List<PiezasDeJuego> gamepieces)
    {
        StartCoroutine(ClearAndReRfillBoardRoutine(gamepieces));
    }
    IEnumerator ClearAndReRfillBoardRoutine(List<PiezasDeJuego> gamepieces)
    {
        yield return StartCoroutine(ClearAndColapseColumn(gamepieces));
        yield return null;
        yield return StartCoroutine(refilroutine());
        puedeMover = true;
    }
    IEnumerator ClearAndColapseColumn(List<PiezasDeJuego> gamepieces)
    {
        List<PiezasDeJuego> movepieces = new List<PiezasDeJuego>();
        List<PiezasDeJuego> matches = new List<PiezasDeJuego>();

        bool isfinishied = false;
        while(!isfinishied)
        {
            ClearPiecesat(gamepieces);
            AudioSource.PlayClipAtPoint(audioFX, gameObject.transform.position); // al rellenar la matriz hace pop
            yield return new WaitForSeconds(1f);  
            movepieces = CollapsarColumnas(gamepieces);
            while(!IsCollapse(gamepieces))
            {
                yield return new WaitForEndOfFrame();
            }
            
            matches = EncontratCoincidenciasEn(movepieces);

            if(matches.Count == 0)
            {
                isfinishied = true;
                break;
                
            }
            else
            {
                yield return StartCoroutine(ClearAndColapseColumn(matches));
            }
        
        }
    }
    IEnumerator refilroutine()
    {
        LlenarAleatorio();
        yield return null;
    }
    bool IsCollapse(List<PiezasDeJuego> gamePieces)
    {
        foreach  (PiezasDeJuego gp  in gamePieces)
        {
            if(gp != null)
            {
                if(gp.transform.position.y - (float)gp.cordenaday > 0.001f)
                {
                    return false;



                    
                }
            }
        }
        return true;
    }
} 
