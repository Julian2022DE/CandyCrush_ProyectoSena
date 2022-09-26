using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int xIndex;
    public int yIndex;
    AudioSource source;
    public AudioClip audioFX;

    Board m_board;


    public void Init(int cambioX, int cambioY, Board board)
    {
        xIndex = cambioX;
        yIndex = cambioY;

        m_board = board;
    }

    public void OnMouseDown()
    {
        m_board.ClickedTile(this);
    }

    public void OnMouseEnter()
    {
        m_board.DragToTile(this);
    }

    public void OnMouseUp()
    {
        m_board.ReleaseTile();
        AudioSource.PlayClipAtPoint(audioFX, gameObject.transform.position);
    }

}
