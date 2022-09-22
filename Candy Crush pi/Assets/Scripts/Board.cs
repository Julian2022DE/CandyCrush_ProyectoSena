using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class Board : MonoBehaviour
{
    public int width;
    public int height;

    public int borderSize;

    public GameObject tilePrefab;
    public GameObject[] gamePiecesPrefabs;

    public float swapTime = 3f;

    Tile[,] m_allTiles;
    public PiezasDeJuego[,] m_allGamePieces;
    public Camera cam;


    [SerializeField] Tile m_clickedTile;
    [SerializeField] Tile m_targetTile;
    
    bool m_playerInputEnabled = true;

    public AudioSource Source;
    public AudioClip audioFX;
    public AudioClip destryaudio;
    public GameObject objpuntos;


    void FillBoard(int falseoffset = 0, float moveTime = .1f)
    {
        List<PiezasDeJuego> addpieces = new List<PiezasDeJuego>();

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (m_allGamePieces[i, j] == null)
                {
                    if (falseoffset == 0)
                    {
                        PiezasDeJuego gamePiece = FillRamdomAt(i, j);
                        addpieces.Add(gamePiece);
                    }
                    else
                    {
                        PiezasDeJuego piece = FillRamdomAt(i, j, falseoffset, moveTime);
                        addpieces.Add(piece);
                    }
                }
            }
        }
        int maxInterations = 20;
        int interations = 0;

        bool isFilled = false;

        while(!isFilled)
        {
            List<PiezasDeJuego> matches = FindAllMatches();
            if(matches.Count == 0)
            {
                isFilled = true;
                break;
            }
            else
            {
                matches = matches.Intersect(addpieces).ToList();
                if(falseoffset == 0)
                {
                    replaceWhithRandom(matches);
                }
                else
                {
                    replaceWhithRandom(matches, falseoffset,moveTime);
                }
            }
            if(interations > maxInterations)
            {
                isFilled = true;
                Debug.LogWarning($"Board.fillBoard alcanzo el maximo de interacciones");
            }
            interations++;
        }
        
    }


    private void Start()
    {
        m_allGamePieces = new PiezasDeJuego[width, height];
        Creatvoard();
        Organizadorcamara();
        FillBoard();
        
    }
    void Creatvoard()
    {
        m_allTiles = new Tile[width, height];
        for (int i = 0; i < width; i++)  //i == x , j == y
        {
            for (int j = 0; j < height; j++)
            {
                GameObject go = Instantiate(tilePrefab);
                go.name = "tile" + i + "," + j;
                go.transform.position = new Vector3(i, j, 0);
                go.transform.parent = transform;
                Tile tiles = go.GetComponent<Tile>();
                tiles.funciones = this;
                tiles.Inicializar(i, j);
                m_allTiles[i, j] = tiles;
            }
        }
    }
    void Organizadorcamara()
    {
        cam.transform.position = new Vector3(((float)width / 2) - 0.5f, ((float)height / 2) - 0.5f, -10);
        float alt = (((float)height / 2) + borderSize);
        float an = ((((float)width / 2) + borderSize) / Screen.width * Screen.height);
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
        int Numeroaleatorio = Random.Range(0, gamePiecesPrefabs.Length);
        GameObject go = Instantiate(gamePiecesPrefabs[Numeroaleatorio]);
        go.GetComponent<PiezasDeJuego>().board = this;
        return go;
    }
    public void Piezaposicion(PiezasDeJuego go, int x, int y)
    {
        go.transform.position = new Vector3(x, y, 0f);
        go.Cordenadas(x, y);

        m_allGamePieces[x, y] = go;
    
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
    public void InicialTile(Tile tile)
    {
        if (m_clickedTile == null)
        {
            m_clickedTile = tile;
        }
    }
    public void FinalTile(Tile tile)
    {
        if (m_clickedTile != null && EsVecino(m_clickedTile, tile) == true)
        {
            m_targetTile = tile;
        }
    }
    public void RealeaseTile()
    {
        if (m_clickedTile != null && m_targetTile != null)
        {
            SwichPieces(m_clickedTile, m_targetTile);
            m_clickedTile = null;
            m_targetTile = null;
        }
    }
    public void SwichPieces(Tile m_clickedTile, Tile m_targetTile)
    {
        StartCoroutine(SwitchTileCorrutine(m_clickedTile, m_targetTile));
    }
    IEnumerator SwitchTileCorrutine(Tile clickedTile, Tile targetTile)
    {
        if (m_playerInputEnabled)
        {      
            PiezasDeJuego clickedPiece = m_allGamePieces[clickedTile.indiceX, clickedTile.indiceY];
            PiezasDeJuego targetPiece = m_allGamePieces[targetTile.indiceX, targetTile.indiceY];


            if (clickedPiece != null && targetPiece != null)
            {
                clickedPiece.Move(targetTile.indiceX, targetTile.indiceY, swapTime);
                targetPiece.Move(clickedTile.indiceX, clickedTile.indiceY, swapTime);

                yield return new WaitForSeconds(swapTime);

                List<PiezasDeJuego> clickedPiecesMatches = FindMatchesAt(clickedTile.indiceX, clickedTile.indiceY);
                List<PiezasDeJuego> targetPiecesMatches = FindMatchesAt(targetTile.indiceX, targetTile.indiceY);

                if (clickedPiecesMatches.Count == 0 && targetPiecesMatches.Count == 0)
                {
                    clickedPiece.Move(clickedTile.indiceX, clickedTile.indiceY, swapTime);
                    targetPiece.Move(targetTile.indiceX, targetTile.indiceY, swapTime);

                    yield return new WaitForSeconds(swapTime);

                    ClearAndRefillBoard(clickedPiecesMatches.Union(targetPiecesMatches).ToList());
                }
             /*   else
                {

                    clickedPiecesMatches = clickedPiecesMatches.Union(targetPiecesMatches).ToList();
                    objpuntos.GetComponent<Timer>().puntos += 10;
                    foreach (PiezasDeJuego pieza in clickedPiecesMatches)
                    {                      
                        pieza.GetComponentInChildren<Animator>().SetBool("A", true);
                    }
                    yield return new WaitForSeconds(.5f);
                    AudioSource.PlayClipAtPoint(destryaudio, gameObject.transform.position); // al destruir un match suena 
                    ClearAndRefillBoard(clickedPiecesMatches);

                }*/
            }
        }  
    }
    void ClearAndRefillBoard(List<PiezasDeJuego> gamepieces)
    {
        StartCoroutine(ClearAndReRfillBoardRoutine(gamepieces));
    }

    List<PiezasDeJuego> FindMatches(int StartX, int StartY, Vector2 searchDirections, int minLemght = 3)
    {
        List<PiezasDeJuego> matches = new List<PiezasDeJuego>();
        PiezasDeJuego startpiece = null;

        if(IsWhithBounds(StartX, StartY))
        {
            startpiece = m_allGamePieces[StartX, StartY];
        }
        if(startpiece != null)
        {
            matches.Add(startpiece);
        }
        else
        {
            return null;
        }

        int nextX;
        int nextY;


        int maxValue = width > height ? width : height;
        
      

        for (int i = 1; i < maxValue - 1; i++)
        {
            nextX = StartX + (int)Mathf.Clamp(searchDirections.x, -1, 1) * i;
            nextY = StartY + (int)Mathf.Clamp(searchDirections.y, -1, 1) * i;

            if (!iswithBounds(nextX, nextY))
            {
                break;
            }
            PiezasDeJuego NextPiece = m_allGamePieces[nextX, nextY];
            if (NextPiece == null)
            {
                break;
            }
            else
            {
                if (NextPiece.ficha == NextPiece.ficha == startpiece.matchValue && !matches.Contains(NextPiece))
                {

                    matches.Add(NextPiece);


                }
                else
                {

                    break;
                }

            }

        }
        if (matches.Count >= minLemght)
        {
            return matches;
        }
        return null;
    }
}
    private void RemplazarConPiezasAleatorias(List<PiezasDeJuego> coincidencias)
    {
        foreach(PiezasDeJuego gamepiece in coincidencias)
        {
            ClearPiecesat(gamepiece.cordenadax, gamepiece.cordenaday);
            FillRamdomAt(gamepiece.cordenadax, gamepiece.cordenaday);
        }
    }
    PiezasDeJuego FillRamdomAt(int X, int Y)
    {
        GameObject go = PiezaAleatoria();
        Piezaposicion(go.GetComponent<PiezasDeJuego>(), X, Y);
        return go.GetComponent<PiezasDeJuego>();
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
        return (_x < width && _x >= 0 && _y >= 0 && _y < height);
    }
    List<PiezasDeJuego> EncontrarCoincidencias(int startx, int starty, Vector2 direcciondelbuscado, int cantidaddepiezas = 3)
    {
        //Crear una lista que coincide encontradas
        List<PiezasDeJuego> coincidencias = new List<PiezasDeJuego>();

        //crear referencia al gamepiece inicial 
        PiezasDeJuego piezainicial = null;
        if(EstaEnRango(startx, starty))
        {
            piezainicial = m_allGamePieces[startx, starty];
        }
        if (piezainicial != null)
        {
            coincidencias.Add(piezainicial);
        }
        else
        {
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
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                 FindMatchesAt(i, j);
            }
        }
    }
    void ResaltarPiezasEn(int _X, int _Y)
    {
        var listascombinadas = FindMatchesAt(_X, _Y);
        if(listascombinadas.Count > 0)
        {
            foreach(PiezasDeJuego p in listascombinadas)
            {
                ResaltarTile(p.cordenadax, p.cordenaday, p.GetComponent<SpriteRenderer>().color);

            }
        }
    }
    
    List<PiezasDeJuego> EncontratCoincidenciasEn(List<PiezasDeJuego> gamepieces, int minimleght = 3)
    {
        List<PiezasDeJuego> matches = new List<PiezasDeJuego>();
        foreach(PiezasDeJuego gp in gamepieces)
        {
            matches = matches.Union(FindMatchesAt(gp.cordenadax, gp.cordenaday)).ToList();
        }
        return matches;
    }
    private List<PiezasDeJuego> EncontrarTodasLasCoincidencias()
    {
        List<PiezasDeJuego> todasLasCoincidencias = new List<PiezasDeJuego>();
        for (int i = 0; i < width; i++)   
        {
            for (int j = 0; j < height; j++)
            {
                var coincidencias = FindMatchesAt(i, j);
                todasLasCoincidencias = todasLasCoincidencias.Union(coincidencias).ToList();
                

            }
        }
        return todasLasCoincidencias;
    }
    public void ResaltarTile(int _X, int _Y, Color color)
    {
        SpriteRenderer sr = m_allTiles[_X, _Y].GetComponent<SpriteRenderer>();
        sr.color = color;
    }  
    void ClearBoard()
    {
        for(int i = 0; i < width; i++)
        { 
            for (int j = 0; j < height; j++)
            {
                ClearPiecesat(i,j);
            }
        }
    }
    private void ClearPiecesat(int x, int y)

    {
        PiezasDeJuego placetoclear = m_allGamePieces[x, y];
        if(placetoclear != null)
        {
            m_allGamePieces[x, y] = null;
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

        for (int i = 0; i < height -1; i++)
        {
            if(m_allGamePieces[column, i] == null)
            {
                for (int j = i + 1; j < height; j++)
                {
                    if (m_allGamePieces[column, j] != null)
                    {
                        m_allGamePieces[column, j].Move(column, i, collapsetime * (j - i));
                        m_allGamePieces[column, i] = m_allGamePieces[column, j];
                        m_allGamePieces[column, j].Cordenadas(column, i);
                        if (!movingpieces.Contains(m_allGamePieces[column, i]))
                        {
                            movingpieces.Add(m_allGamePieces[column, i]);
                        }
                        m_allGamePieces[column, j] = null;

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
    IEnumerator ClearAndReRfillBoardRoutine(List<PiezasDeJuego> gamepieces)
    {
        yield return StartCoroutine(ClearAndColapseColumn(gamepieces));
        yield return null;
        yield return StartCoroutine(refilroutine());
        m_playerInputEnabled = true;
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
        FillBoard();

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
