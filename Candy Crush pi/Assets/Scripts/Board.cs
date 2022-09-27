using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class Board : MonoBehaviour
{
	public int width;
	public int height;
	// Declara borde de la Camara
	public float borderSize;
	// declarar variables de prefabs a usar 
	public GameObject tilePrefab;
	public GameObject[] gamePiecesPrefabs;
	//declarar tiempo de cambiar ficha
	public float swapTime = .3f;
	//declara las variables de puntaje llamando el script
	public Puntaje m_puntaje;
	// se declara las matrices de los arrays 
	Tile[,] m_allTiles;
	GamePiece[,] m_allGamePieces;
	// declaramos variables de tiles selecionados
	[SerializeField] Tile m_clickedTile;
	[SerializeField] Tile m_targetTile;
	//bool para controlar el movimiento
	bool m_playerInputEnabled = true;
	//transform para controlar las cosas
	Transform tileParent;
	Transform gamePieceParent;
	//contador combos
	int myCount = 0;
	// audios
	AudioSource source;
	public AudioClip audioFX;
	public AudioClip destroyAudio;

	public int cantidademovimientos;

	private void Start() 
	{
		SetParents();

		m_allTiles = new Tile[width, height];
		m_allGamePieces = new GamePiece[width, height];

		SetupTiles();
		SetupCamera();
		FillBoard(10, .5f);
	}
	private void SetParents() //padre de los tiles y de los prefabs en general 
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
	private void SetupCamera() //organiza la camara 
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
				m_allTiles[i, j].Init(i, j, this);
			}
		}
	} // instancia los tiles 
	private void FillBoard(int falseOffset = 0, float moveTime = .1f)
	{
		List<GamePiece> addedPieces = new List<GamePiece>();

		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				if (m_allGamePieces[i, j] == null)
				{
					if (falseOffset == 0)
					{
						GamePiece piece = FillRandomAt(i, j);
						addedPieces.Add(piece);
					}
					else
					{
						GamePiece piece = FillRandomAt(i, j, falseOffset, moveTime);
						addedPieces.Add(piece);
					}
				}
			}
		}
		int maxIterations = 20;
		int iterations = 0;

		bool isFilled = false;

		while (!isFilled)
		{
			List<GamePiece> matches = FindAllMatches();

			if (matches.Count == 0)
			{
				isFilled = true;
				break;
			}
			else
			{
				matches = matches.Intersect(addedPieces).ToList();

				if (falseOffset == 0)
				{
					ReplaceWithRandom(matches);
				}
				else
				{
					ReplaceWithRandom(matches, falseOffset, moveTime);
				}
			}

			if (iterations > maxIterations)
			{
				isFilled = true;
				Debug.LogWarning($"Board.FillBoard alcanzo el maximo de interacciones, abortar");
			}

			iterations++;
		}
	} // Genera las piezas random
	public void ClickedTile(Tile tile)
	{
		if (m_clickedTile == null)
		{
			m_clickedTile = tile;
		}
	} // selecciona un tile al dar click o llamada tile inicial
	public void DragToTile(Tile tile)
	{
		if (m_clickedTile != null && IsNextTo(tile, m_clickedTile))
		{
			m_targetTile = tile;
		}
	} // Tile de intercambio
	public void ReleaseTile()
	{
		if (m_clickedTile != null && m_targetTile != null)
		{
			SwitchTiles(m_clickedTile, m_targetTile);
		}
		m_clickedTile = null;
		m_targetTile = null;
	} //Cambiar el sitio
	private void SwitchTiles(Tile m_clickedTile, Tile m_targetTile)
	{
		StartCoroutine(SwitchTilesRoutine(m_clickedTile, m_targetTile));
	} // llama la ienumerator swichtiles
	IEnumerator SwitchTilesRoutine(Tile clickedTile, Tile targetTile)
	{
		if (m_playerInputEnabled)
		{
			GamePiece clickedPiece = m_allGamePieces[clickedTile.xIndex, clickedTile.yIndex];
			GamePiece targetPiece = m_allGamePieces[targetTile.xIndex, targetTile.yIndex];

			if (clickedPiece != null && targetPiece != null)
			{
				clickedPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime);
				targetPiece.Move(clickedPiece.xIndex, clickedPiece.yIndex, swapTime);
				
				yield return new WaitForSeconds(swapTime);

				List<GamePiece> clickedPieceMatches = FindMatchesAt(clickedTile.xIndex, clickedTile.yIndex);
				List<GamePiece> targetPieceMatches = FindMatchesAt(targetTile.xIndex, targetTile.yIndex);

				if (clickedPieceMatches.Count != 0 && targetPieceMatches.Count != 0)
				{
					bool Combo1_ = Figure(clickedTile.xIndex, clickedTile.yIndex);
					bool combo1 = Figure(targetPiece.xIndex, targetPiece.yIndex);
				}

				if (clickedPieceMatches.Count == 0 && targetPieceMatches.Count == 0)
				{
					clickedPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime);
					targetPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime);

					

					yield return new WaitForSeconds(swapTime);
				}
				else
				{
					CleatAndRefillBoard(clickedPieceMatches.Union(targetPieceMatches).ToList());
					AudioSource.PlayClipAtPoint(destroyAudio, gameObject.transform.position);
				}
				cantidademovimientos--;
			}
			if(cantidademovimientos <= 0)
            {
				SceneManager.LoadScene("Game Over");
            }
		}
	} // genera los matches junto a el audio de distroy match movimientos y combo de 2 macths en 1
	private void CleatAndRefillBoard(List<GamePiece> gamePieces)
	{
		myCount = 0;
		StartCoroutine(ClearAndRefillRoutine(gamePieces));
	}// puntaje  = 0 y llama la funcion de limpiar el board 
	List<GamePiece> FindMatches(int startX, int startY, Vector2 searchDirection, int minLenght = 3)
	{
		List<GamePiece> matches = new List<GamePiece>();
		GamePiece startPiece = null;

		if (IsWithBounds(startX, startY))
		{
			startPiece = m_allGamePieces[startX, startY];
		}

		if (startPiece != null)
		{
			
			matches.Add(startPiece);
		}
		else
		{
			return null;
		}

		int nextX;
		int nextY;

		int maxValue = width > height ? width : height;

		for (int i = 1; i < maxValue; i++)
		{
			nextX = startX + (int)Mathf.Clamp(searchDirection.x, -1, 1) * i;
			nextY = startY + (int)Mathf.Clamp(searchDirection.y, -1, 1) * i;

			if (!IsWithBounds(nextX, nextY))
			{
				break;
			}

			GamePiece nextPiece = m_allGamePieces[nextX, nextY];

			if (nextPiece == null)
			{
				break;
			}
			else
			{
				if (nextPiece.tipoFicha == startPiece.tipoFicha && !matches.Contains(nextPiece))
				{
					matches.Add(nextPiece);
				}
				else
				{
					break;
				}
			}
		}

		if (matches.Count >= minLenght)
		{
			return matches;
		}
		else
		{
			return null;
		}
	} // lista de encontrar los matches en la lista
	List<GamePiece> FindVerticalMatches(int startX, int startY, int minLenght = 3)
	{
		List<GamePiece> upwardMatches = FindMatches(startX, startY, Vector2.up, 2);
		List<GamePiece> downwardMatches = FindMatches(startX, startY, Vector2.down, 2);

		if (upwardMatches == null)
		{
			upwardMatches = new List<GamePiece>();
		}
		if (downwardMatches == null)
		{
			downwardMatches = new List<GamePiece>();
		}

		var combinedMatches = upwardMatches.Union(downwardMatches).ToList();
		return combinedMatches.Count >= minLenght ? combinedMatches : null;
	} // encuenta los matches verticales juntando la variable izquierda y derecha 
	List<GamePiece> FindHorizontalMatches(int startX, int startY, int minLenght = 3)
	{
		List<GamePiece> rightMatches = FindMatches(startX, startY, Vector2.right, 2);
		List<GamePiece> leftMatches = FindMatches(startX, startY, Vector2.left, 2);

		if (rightMatches == null)
		{
			rightMatches = new List<GamePiece>();
		}
		if (leftMatches == null)
		{
			leftMatches = new List<GamePiece>();
		}

		var combinedMatches = rightMatches.Union(leftMatches).ToList();
		return combinedMatches.Count >= minLenght ? combinedMatches : null;
	} //encuenta los matches Horizontales juntando la variable arriba abajo
	private List<GamePiece> FindMatchesAt(int x, int y, int minLenght = 3)
	{
		List<GamePiece> horizontalMatches = FindHorizontalMatches(x, y, minLenght);
		List<GamePiece> verticalMatches = FindVerticalMatches(x, y, minLenght);
		

		if (horizontalMatches == null)
		{
			horizontalMatches = new List<GamePiece>();
		}
		if (verticalMatches == null)
		{
			verticalMatches = new List<GamePiece>();
		}
		var combinedMatches = horizontalMatches.Union(verticalMatches).ToList();
		if (horizontalMatches.Count != 0 && verticalMatches.Count != 0)
		{
			int cantidadPuntos = 100;
			m_puntaje.SumatoriaPuntos(cantidadPuntos);
		}
		return combinedMatches;
	} //genera bien los matches en ambas vectores horizontal y vertical y los combos "ojala de aqui a terminar pueda diferenciar la T y la L en caso de que no me da la solucion si lee esto plis"
	public bool Figure(int x, int y, int minLenght = 3)
	{
		List<GamePiece> horizontalMatches = FindHorizontalMatches(x, y, minLenght);
		List<GamePiece> verticalMatches = FindVerticalMatches(x, y, minLenght);

		if (horizontalMatches == null)
		{
			horizontalMatches = new List<GamePiece>();
		}
		if (verticalMatches == null)
		{
			verticalMatches = new List<GamePiece>();
		}
		if (horizontalMatches.Count == 0 && verticalMatches.Count != 0)
		{
			return false;
		}
		if (horizontalMatches.Count != 0 && verticalMatches.Count == 0)
		{
			return false;
		}
		if (horizontalMatches.Count != 0 && verticalMatches.Count != 0)
		{
			return true;
		}
		else
		{
			return false;
		}
	} // Base de el combo en 2 matches
	private bool IsNextTo(GamePiece a, GamePiece b)
    {

		if(Mathf.Abs(a.xIndex - b.xIndex) == 1 && a.yIndex == b.yIndex)
        {	
			return true;
        }
		if(Mathf.Abs(a.yIndex - b.yIndex) == 1 && a.xIndex == b.xIndex)
		{
			return true;
		}
		return false;
	} // sobrecargamos com gamepiece la funcion isnextto fr los tiles para buscar la L y la T
	List<GamePiece> FindMatchesAt(List<GamePiece> gamePieces, int minLenght = 3)
	{
		List<GamePiece> matches = new List<GamePiece>();

		foreach (GamePiece piece in gamePieces)
		{
			matches = matches.Union(FindMatchesAt(piece.xIndex, piece.yIndex, minLenght)).ToList();
		}

		return matches;
	} // busca en una lista de gamepiece con el foreach los matches
	private bool IsNextTo(Tile start, Tile end)
	{
		if (Mathf.Abs(start.xIndex - end.xIndex) == 1 && start.yIndex == end.yIndex)
		{
			return true;
		}
		if (Mathf.Abs(start.yIndex - end.yIndex) == 1 && start.xIndex == end.xIndex)
		{
			return true;
		}
		return false;
	} // detecta si es vecino o no para comprobar si se puede match y no 
	private List<GamePiece> FindAllMatches()
	{
		List<GamePiece> combinedMatches = new List<GamePiece>();

		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				var matches = FindMatchesAt(i, j);
				combinedMatches = combinedMatches.Union(matches).ToList();
			}
		}
		return combinedMatches;
	} // resumen busca todos los matches 
	void HighlightTileOff(int x, int y)
	{
		SpriteRenderer spriteRender = m_allTiles[x, y].GetComponent<SpriteRenderer>();
		spriteRender.color = new Color(spriteRender.color.r, spriteRender.color.g, spriteRender.color.b, 0);
	} //quita el resaltado del match(yo lo tengo desactivado)
	void HighlightTileOn(int x, int y, Color col)
	{
		SpriteRenderer spriteRenderer = m_allTiles[x, y].GetComponent<SpriteRenderer>();
		spriteRenderer.color = col;
	} // resalta todos los tiles
	void HighlightMatchesAt(int x, int y)
	{
		HighlightTileOff(x, y);
		var combinedMatches = FindMatchesAt(x, y);

		if (combinedMatches.Count > 0)
		{
			foreach (GamePiece piece in combinedMatches)
			{
				HighlightTileOn(piece.xIndex, piece.yIndex, piece.GetComponent<SpriteRenderer>().color);
			}
		}
	} // resalta el match con un foreach
	void HighlightMatches()
	{
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				HighlightMatchesAt(i, j);
			}
		}
	} // analiza el HighlightMatches y revisa toda la matriz 
	void HighlightPieces(List<GamePiece> gamepieces)
	{
		foreach (GamePiece piece in gamepieces)
		{
			if (piece != null)
			{
				HighlightTileOn(piece.xIndex, piece.yIndex, piece.GetComponent<SpriteRenderer>().color);
			}
		}
	} //Gamepieces relaltadas si la pieza es diferente a nula analizas los dos indices 
	void ClearPieceAt(int x, int y) // limpiar las piezas y las destruye 
	{
		GamePiece pieceToClear = m_allGamePieces[x, y];

		if (pieceToClear != null)
		{
			m_allGamePieces[x, y] = null;

			Destroy(pieceToClear.gameObject);
		}

		HighlightTileOff(x, y);
	}
	void ClearPieceAt(List<GamePiece> gamePieces) // analiza pieza x pieza en ambos indices y busca donde ejecutar el anterior clearpiecesat
	{
		foreach (GamePiece piece in gamePieces)
		{
			if (piece != null)
			{
				ClearPieceAt(piece.xIndex, piece.yIndex);
			}
		}
	}
	void ClearBoard()
	{
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				ClearPieceAt(i, j);
			}
		}
	} // instancia la anterior funcion en toda la matriz 
	GameObject GetRandomPiece() 
	{
		int randomInx = Random.Range(0, gamePiecesPrefabs.Length);

		if (gamePiecesPrefabs[randomInx] == null)
		{
			Debug.LogWarning($"La clase Board en el array de prefabs en la posicion {randomInx} no contiene una pieza valida");
		}

		return gamePiecesPrefabs[randomInx];
	} 
	public void placeGamePiece(GamePiece gamePiece, int x, int y)
	{
		if (gamePiece == null)
		{
			Debug.LogWarning($"gamePiece invalida");
			return;
		}

		gamePiece.transform.position = new Vector2(x, y);
		gamePiece.transform.rotation = Quaternion.identity;

		if (IsWithBounds(x, y))
		{
			m_allGamePieces[x, y] = gamePiece;
		}

		gamePiece.SetCoord(x, y);
	}  // debugea si el maych es invalido
	private bool IsWithBounds(int x, int y) // retornamos el width y el heoght
	{
		return (x >= 0 && x < width && y >= 0 && y < height);
	}
	GamePiece FillRandomAt(int x, int y, int falseOffset = 0, float moveTime = .1f)
	{
		GamePiece randomPiece = Instantiate(GetRandomPiece(), Vector2.zero, Quaternion.identity).GetComponent<GamePiece>();

		if (randomPiece != null)
		{
			randomPiece.Init(this);
			placeGamePiece(randomPiece, x, y);

			if (falseOffset != 0)
			{
				randomPiece.transform.position = new Vector2(x, y + falseOffset);
				randomPiece.Move(x, y, moveTime);
			}

			randomPiece.transform.parent = gamePieceParent;
		}

		return randomPiece;
	} // se ejecuta cuando ay un match invalido y vuelve a la posicion inicial
	void ReplaceWithRandom(List<GamePiece> gamePieces, int falseOffset = 0, float moveTime = .1f)
	{
		foreach (GamePiece piece in gamePieces)
		{
			ClearPieceAt(piece.xIndex, piece.yIndex);

			if (falseOffset == 0)
			{
				FillRandomAt(piece.xIndex, piece.yIndex);
			}
			else
			{
				FillRandomAt(piece.xIndex, piece.yIndex, falseOffset, moveTime);
			}
		}
	}  // en caso de que al principio se hagan match analiza y remplaza por una ficha random
	List<GamePiece> CollapseColumn(int column, float collapseTime = .1f)
	{
		List<GamePiece> movingPieces = new List<GamePiece>();

		for (int i = 0; i < height - 1; i++)
		{
			if (m_allGamePieces[column, i] == null)
			{
				for (int j = i + 1; j < height; j++)
				{
					if (m_allGamePieces[column, j] != null)
					{
						m_allGamePieces[column, j].Move(column, i, collapseTime * (j - i));
						m_allGamePieces[column, i] = m_allGamePieces[column, j];
						m_allGamePieces[column, i].SetCoord(column, i);

						if (!movingPieces.Contains(m_allGamePieces[column, i]))
						{
							movingPieces.Add(m_allGamePieces[column, i]);
						}

						m_allGamePieces[column, j] = null;
						break;
					}
				}
			}
		}

		return movingPieces;
	} // colapsa las columnas y añade nuevas piezas cuando se genera el match
	List<GamePiece> CollapseColumn(List<GamePiece> gamePieces) // sobrecarga las columnas y toma una lista de gamepieces y analiza ficha x ficha 
	{
		List<GamePiece> movingPieces = new List<GamePiece>();
		List<int> columnsToColapse = GetColumns(gamePieces);

		foreach (int column in columnsToColapse)
		{
			movingPieces = movingPieces.Union(CollapseColumn(column)).ToList();
		}

		return movingPieces;
	}
	private List<int> GetColumns(List<GamePiece> gamePieces)
	{
		List<int> columns = new List<int>();

		foreach (GamePiece piece in gamePieces)
		{
			if (!columns.Contains(piece.xIndex))
			{
				columns.Add(piece.xIndex);
			}
		}

		return columns;
	} // un entero y agarra las columnas en una nueva lista al momento de remplazar la columna con nuevas fichas 
	IEnumerator ClearAndRefillRoutine(List<GamePiece> gamePieces) // genera puntuacion y animacion
	{
		m_playerInputEnabled = true;
		List<GamePiece> matches = gamePieces;

		do
		{
			//esta es la parte donde se activa la animacion al hacer match manual//
			foreach (GamePiece piece in matches)
			{

				if (matches.Count == 3)
				{
					int cantidadPuntos = 5;

					m_puntaje.SumatoriaPuntos(cantidadPuntos);

				}
				if (matches.Count == 4)
				{
					int cantidadPuntos = 10;

					m_puntaje.SumatoriaPuntos(cantidadPuntos);

				}
				if (matches.Count == 5)
				{
					int cantidadPuntos = 15;

					m_puntaje.SumatoriaPuntos(cantidadPuntos);

				}
				if (matches.Count >= 6)
				{
					int cantidadPuntos = 30;

					m_puntaje.SumatoriaPuntos(cantidadPuntos);

				}

				piece.GetComponentInChildren<Animator>().SetBool("A", true);

			}

			yield return StartCoroutine(ClearAndCollapseRoutine(matches));
			yield return null;
			yield return StartCoroutine(RefillRoutine());
			matches = FindAllMatches();
			yield return new WaitForSeconds(.5f);
		}
		while (matches.Count != 0);
		m_playerInputEnabled = true;
	}
	IEnumerator ClearAndCollapseRoutine(List<GamePiece> gamePieces)
	{
		myCount++;
		List<GamePiece> movingPieces = new List<GamePiece>();
		List<GamePiece> matches = new List<GamePiece>();
		HighlightPieces(gamePieces);
		yield return new WaitForSeconds(.5f);
		bool isFinished = false;


		while (!isFinished)
		{
			ClearPieceAt(gamePieces);
			yield return new WaitForSeconds(.25f);

			movingPieces = CollapseColumn(gamePieces);
			while (!IsCollapsed(gamePieces))
			{
				yield return null;
			}
			AudioSource.PlayClipAtPoint(audioFX, gameObject.transform.position);

			yield return new WaitForSeconds(.5f);

			matches = FindMatchesAt(movingPieces);

			if (matches.Count == 0)
			{
				isFinished = true;
				break;
			}
			else
			{
				foreach (GamePiece piece in matches)
				{


					if (matches.Count == 3)
					{
						int cantidadPuntos = 10 * myCount;

						m_puntaje.SumatoriaPuntos(cantidadPuntos);

					}
					if (matches.Count == 4)
					{
						int cantidadPuntos = 20 * myCount;

						m_puntaje.SumatoriaPuntos(cantidadPuntos);

					}
					if (matches.Count == 5)
					{
						int cantidadPuntos = 30 * myCount;

						m_puntaje.SumatoriaPuntos(cantidadPuntos);

					}
					if (matches.Count >= 6)
					{
						int cantidadPuntos = 40 * myCount;

						m_puntaje.SumatoriaPuntos(cantidadPuntos);

					}

					piece.GetComponentInChildren<Animator>().SetBool("A", true);
				}

				yield return StartCoroutine(ClearAndCollapseRoutine(matches));
			}
		}
		yield return null;
	} //general la puntuacion extra cuando hace u n math cuando cae de otro match
	IEnumerator RefillRoutine()
	{
		FillBoard(10, .5f);
		yield return null;
	} //llama la funcion fill board  
	public bool IsCollapsed(List<GamePiece> gamePieces)
	{
		foreach (GamePiece piece in gamePieces)
		{
			if (piece != null)
			{
				if (piece.transform.position.y - (float)piece.yIndex > 0.001f)
				{
					return false;
				}
			}
		}

		return true;
	} // colapsa No me dio tiempo me hecharon kjakajkaja
}
