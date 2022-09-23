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

    Transform tileParent;
    Transform gamePieceParent;


    public AudioSource Source;
    public AudioClip audioFX;
    public AudioClip destryaudio;
    public GameObject objpuntos;

   
    private void Start()
    {
        SetParents();

        m_allTiles = new Tile[width, height];
        m_allGamePieces = new PiezasDeJuego[width, height];

        SetupTiles();
        SetupCamera();
        FillBoard(10, .5f);
    }

    private void SetParents()
    {
        if (tileParent == null)
        {
            tileParent = new GameObject().transform;
            tileParent.name = "Tiles";
            tileParent.parent = this.transform;
        }

        if (gamePieceParent == null)
        {
            gamePieceParent = new GameObject().transform;
            gamePieceParent.name = "GamePieces";
            gamePieceParent = this.transform;
        }
    }

    private void SetupCamera()
    {
        Camera.main.transform.position = new Vector3((float)(width - 1) / 2f, (float)(height - 1) / 2f, -10f);

        float aspectRatio = (float)Screen.width / (float)Screen.height;
        float verticalSize = (float)height / 2f + (float)borderSize;
        float horizontalSize = ((float)width / 2f + (float)borderSize) / aspectRatio;
        Camera.main.orthographicSize = verticalSize > horizontalSize ? verticalSize : horizontalSize;
    }

    private void SetupTiles()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                GameObject tile = Instantiate(tilePrefab, new Vector2(i, j), Quaternion.identity);
                tile.name = $"Tile({i},{j})";

                if (tileParent != null)
                {
                    tile.transform.parent = tileParent;
                }
                m_allTiles[i, j] = tile.GetComponent<Tile>();
                m_allTiles[i, j].Init(i,j);
            }
        }
    }


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
                        PiezasDeJuego gamePiece = FillRandomAt(i, j);
                        addpieces.Add(gamePiece);
                    }
                    else
                    {
                        PiezasDeJuego piece = FillRandomAt(i, j, falseoffset, moveTime);
                        addpieces.Add(piece);
                    }
                }
            }
        }
        int maxInterations = 20;
        int interations = 0;

        bool isFilled = false;

        while (!isFilled)
        {
            List<PiezasDeJuego> matches = FindAllMatches();
            if (matches.Count == 0)
            {
                isFilled = true;
                break;
            }
            else
            {
                matches = matches.Intersect(addpieces).ToList();
                if (falseoffset == 0)
                {
                    ReplacedWhithRandom(matches);
                }
                else
                {
                    ReplacedWhithRandom(matches, falseoffset, moveTime);
                }
            }
            if (interations > maxInterations)
            {
                isFilled = true;
                Debug.LogWarning($"Board.fillBoard alcanzo el maximo de interacciones");
            }
            interations++;
        }

    }
    public void ClickedTile(Tile tile)
    {
        if (m_clickedTile == null)
        {
            m_clickedTile = tile;
        }
    }
    public void DragTile(Tile tile)
    {
        if (m_clickedTile != null && IsNextTo(m_clickedTile, tile) == true)
        {
            m_targetTile = tile;
        }
    }
    public void RealeaseTile()
    {
        if (m_clickedTile != null && m_targetTile != null)
        {
            SwichTiles(m_clickedTile, m_targetTile);
            m_clickedTile = null;
            m_targetTile = null;
        }
    }
    public void SwichTiles(Tile m_clickedTile, Tile m_targetTile)
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
            }
        }
    }
    void ClearAndRefillBoard(List<PiezasDeJuego> gamepieces)
    {
        StartCoroutine(ClearAndReRfillBoardRoutine(gamepieces));
    }

    public List<PiezasDeJuego> FindMatches(int StartX, int StartY, Vector2 searchDirections, int minLemght = 3)
    {
        List<PiezasDeJuego> matches = new List<PiezasDeJuego>();
        PiezasDeJuego startpiece = null;

        if (IsWithBounds(StartX, StartY))
        {
            startpiece = m_allGamePieces[StartX, StartY];
        }
        if (startpiece != null)
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

            if (!IsWithBounds(nextX, nextY))
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
                if (NextPiece.tipoFicha == startpiece.tipoFicha && !matches.Contains(NextPiece))
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

    public  List<PiezasDeJuego> FindVerticalMatches(int startX, int startY, int minLeght = 3)
    {
        List<PiezasDeJuego> upwardMatches = FindMatches(startX, startY, Vector2.up, 2);
        List<PiezasDeJuego> downwardMatches = FindMatches(startX, startY, Vector2.down, 2);

        if(upwardMatches == null)
        {
            upwardMatches = new List<PiezasDeJuego>();
        }
        if(downwardMatches == null)
        {
            downwardMatches = new List<PiezasDeJuego>();
        }
        var combinematches = upwardMatches.Union(downwardMatches).ToList();
        return combinematches.Count >= minLeght ? combinematches : null;
    }
    public List<PiezasDeJuego> FindHorizontalMatches(int startX, int startY, int cantidadminima = 3)
    {
        List<PiezasDeJuego> RightMatches = FindMatches(startX, startY, Vector2.right, 2);
        List<PiezasDeJuego> leftMatches = FindMatches(startX, startY, Vector2.left, 2);

        if (RightMatches == null)
        {
            RightMatches = new List<PiezasDeJuego>();

        }
        if (leftMatches == null)
        {
            leftMatches = new List<PiezasDeJuego>();

        }
        var combineMatches = RightMatches.Union(leftMatches).ToList();
        return combineMatches.Count >= cantidadminima ? combineMatches : null;
    }
    List<PiezasDeJuego> FindMatchesAt(int x, int y, int cantidaddepiezas = 3)
    {
        List<PiezasDeJuego> HorizontalMatches = FindHorizontalMatches(x, y, cantidaddepiezas);
        List<PiezasDeJuego> verticalMatches = FindVerticalMatches(x, y, cantidaddepiezas);
        if (HorizontalMatches == null)
        {
            HorizontalMatches = new List<PiezasDeJuego>();
        }
        if (verticalMatches == null)
        {
            verticalMatches = new List<PiezasDeJuego>();
        }
        var combinedMatches = HorizontalMatches.Union(verticalMatches).ToList();
        return combinedMatches;
    }
    List<PiezasDeJuego> FindMatcesAt(List<PiezasDeJuego> gamepieces, int minimleght = 3)
    {
        List<PiezasDeJuego> matches = new List<PiezasDeJuego>();
        foreach (PiezasDeJuego gp in gamepieces)
        {
            matches = matches.Union(FindMatchesAt(gp.xIndex, gp.yIndex)).ToList();
        }
        return matches;
    }
    bool IsNextTo(Tile start, Tile end)
    {
        if (Mathf.Abs(start.indiceX - end.indiceX) == 1 && start.indiceY == end.indiceY)
        {
            return true;
        }
        if (Mathf.Abs(start.indiceY - end.indiceY) == 1 && start.indiceX == end.indiceX)
        {
            return true;
        }
        return false;
    }
     public List<PiezasDeJuego> FindAllMatches()
    { 
        List<PiezasDeJuego> CombinedMatches = new List<PiezasDeJuego>();
        for (int i = 0; i < width; i++)   
        {
            for (int j = 0; j < height; j++)
            {
                var coincidencias = FindMatchesAt(i, j);
                CombinedMatches = CombinedMatches.Union(coincidencias).ToList();
            }
        }
        return CombinedMatches;
    }
    public void HighlightTileOff(int x, int y)
    {
        SpriteRenderer spriterenderer = m_allTiles[x, y].GetComponent<SpriteRenderer>();
        spriterenderer.color = new Color(spriterenderer.color.r, spriterenderer.color.g, spriterenderer.color.b, 0);
    }

    public void HighlightTileOn(int _X, int _Y, Color color)
    {
        SpriteRenderer sr = m_allTiles[_X, _Y].GetComponent<SpriteRenderer>();
        sr.color = color;
    }

    public void HighlightMatchesAtt(int x, int y)
    {
        HighlightTileOff(x, y);
        var combinedMatches = FindMatchesAt(x, y);
        if (combinedMatches.Count > 0)
        {
            foreach (PiezasDeJuego piece in combinedMatches)
            {
                HighlightTileOn(piece.xIndex, piece.yIndex, piece.GetComponent<SpriteRenderer>().color);
            }
        }
    }
    void HighlightMatches()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                HighlightMatchesAtt(i, j);
            }
        }
    }

    void HighlightPieces(List<PiezasDeJuego> gamepieces)
    {
        foreach (PiezasDeJuego piece in gamepieces)
        {
            if (piece != null)
            {
                HighlightTileOn(piece.xIndex, piece.yIndex, piece.GetComponent<SpriteRenderer>().color);
            }
        }
    }
    private void ClearPiecesAt(int x, int y)

    {
        PiezasDeJuego placetoclear = m_allGamePieces[x, y];
        if (placetoclear != null)
        {
            m_allGamePieces[x, y] = null;
            Destroy(placetoclear.gameObject);


            // Se destruye cuando se hace match al caer despues de otro match
            AudioSource.PlayClipAtPoint(destryaudio, gameObject.transform.position);


        }
        HighlightTileOff(x, y);
    }
    private void ClearPiecesAt(List<PiezasDeJuego> gamepieces)
    {
        foreach (PiezasDeJuego go in gamepieces)
        {
            if (go != null)
            {
                ClearPiecesAt(go.xIndex, go.yIndex);
            }
        }
    }
    GameObject GetRandomPiece()
    {
        int Numeroaleatorio = Random.Range(0, gamePiecesPrefabs.Length);
        return gamePiecesPrefabs[Numeroaleatorio];
    }
    public void PlaceGamePiece(PiezasDeJuego gamepiece, int x, int y)
    {
        gamepiece.transform.position = new Vector2(x, y);
        gamepiece.transform.rotation = Quaternion.identity;

        if(IsWithBounds(x, y))
        {
            m_allGamePieces[x, y] = gamepiece;
        }
        gamepiece.SetCord(x, y);
    }
    bool IsWithBounds(int _x, int _y)
    {
        return (_x < width && _x >= 0 && _y >= 0 && _y < height);
    }
   public PiezasDeJuego FillRandomAt(int x, int y, int falseOffset = 0, float moveTime = .1f)
    {
        PiezasDeJuego randomPiece = Instantiate(GetRandomPiece(), Vector2.zero, Quaternion.identity).GetComponent<PiezasDeJuego>();

        if (randomPiece != null)
        {
            randomPiece.Init(this);
            PlaceGamePiece(randomPiece, x, y);

            if (falseOffset != 0)
            {
                randomPiece.transform.position = new Vector2(x, y + falseOffset);
                randomPiece.Move(x, y, moveTime);
            }

            randomPiece.transform.parent = gamePieceParent;
        }

        return randomPiece;
    }


    void ReplacedWhithRandom(List<PiezasDeJuego> gamepieces, int falseOffset = 0, float moveTime = .1f)
    {
        foreach (PiezasDeJuego piece in gamepieces)
        {
            ClearPiecesAt(piece.xIndex, piece.yIndex);
            if(falseOffset == 0)
            {
                FillRandomAt(piece.xIndex, piece.yIndex);
            }
            else
            {
                FillRandomAt(piece.xIndex, piece.yIndex, falseOffset, moveTime);
            }
        }
    }
    List<PiezasDeJuego> CollapseColumn(int column, float collapsetime = 0.1f)
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
                        m_allGamePieces[column, j].SetCord(column, i);
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
    List<PiezasDeJuego> CollapseColumn(List<PiezasDeJuego> gamepieces)
    {
        List<PiezasDeJuego> MovinginPieces = new List<PiezasDeJuego>();
        List<int> columnToCollapse = GetColumn(gamepieces);
        foreach(int column in columnToCollapse)
        {
            MovinginPieces = MovinginPieces.Union(CollapseColumn(column)).ToList();

        }
        return MovinginPieces;
    }
    private List<int> GetColumn(List<PiezasDeJuego> gamepieces)
    {
        List<int> CollumsIndex = new List<int>();
        foreach  (PiezasDeJuego gamepiece in gamepieces)
        {
            if (!CollumsIndex.Contains(gamepiece.xIndex)) 
            {
                CollumsIndex.Add(gamepiece.xIndex);
            }
        }
        return CollumsIndex;
    }
    IEnumerator ClearAndReRfillBoardRoutine(List<PiezasDeJuego> gamepieces)
    {
        m_playerInputEnabled = true;
        List<PiezasDeJuego> matches = gamepieces;

        do
        {
            yield return StartCoroutine(ClearAndColapseColumn(gamepieces));
            yield return null;
            yield return StartCoroutine(refilroutine());
        }
        while (matches.Count != 0);
        m_playerInputEnabled = true;
    }
    IEnumerator ClearAndColapseColumn(List<PiezasDeJuego> gamepieces)
    {
        List<PiezasDeJuego> movepieces = new List<PiezasDeJuego>();
        List<PiezasDeJuego> matches = new List<PiezasDeJuego>();
        HighlightPieces(gamepieces);
        yield return new WaitForSeconds(.5f);

        bool isfinishied = false;
        while(!isfinishied)
        {
            ClearPiecesAt(gamepieces);
            AudioSource.PlayClipAtPoint(audioFX, gameObject.transform.position); // al rellenar la matriz hace pop
            yield return new WaitForSeconds(1f);  
            movepieces = CollapseColumn(gamepieces);
            while(!IsCollapse(gamepieces))
            {
                yield return new WaitForEndOfFrame();
            }
            
            matches = FindMatcesAt(movepieces);

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
        yield return null;
    }
    IEnumerator refilroutine()
    {
        FillBoard(10, .5f);

        yield return null;
    }
    bool IsCollapse(List<PiezasDeJuego> gamePieces)
    {
        foreach  (PiezasDeJuego gp  in gamePieces)
        {
            if(gp != null)
            {
                if(gp.transform.position.y - (float)gp.yIndex > 0.001f)
                {
                    return false;
                }
            }
        }
        return true;
    }
} 
