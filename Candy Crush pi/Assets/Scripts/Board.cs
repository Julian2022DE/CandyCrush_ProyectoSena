using UnityEngine;

public class Board : MonoBehaviour
{
    public int alto;
    public int ancho;

    public Tile[,] board;
    public GameObject prefile;

    private void Start()
    {
        creatvoard();
    }

    void creatvoard()
    {
        board = new Tile[ancho, alto];

        for(int i = 0; i < alto; i++)
        {
            for(int j = 0; j < ancho; j++)
            {
                GameObject go = Instantiate(prefile);
                go.name = "tiles" + i + "," + j;
                go.transform.position = new Vector3(i, j, 0);
                go.transform.parent = transform;
                Tile tiles = go.GetComponent<Tile>();

                board[i, j] = tiles;

                tiles.inicializar(i, j);
            }
        }
    }
}
